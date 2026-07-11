using System.Text.Json;

namespace SmingCode.Utilities.Kafka.Consumers;
using Config;
using ServiceMetadata;

internal class KafkaConsumer<TKey, TValue>(
    IServiceScopeFactory _serviceScopeFactory,
    ITopicManager _topicManager,
    KafkaConsumerDefinition<TKey, TValue> _kafkaConsumerDefinition,
    IServiceMetadataProvider serviceMetadataProvider,
    KafkaOptions _kafkaOptions,
    ConsumerMiddlewareHandler middlewareHandler,
    ILogger<KafkaConsumer<TKey, TValue>> _logger
) : IKafkaConsumer
    where TKey : notnull
    where TValue : notnull
{
    private IConsumer<string, string> _consumer = null!;
    private readonly string _serviceName = serviceMetadataProvider.GetMetadata().ServiceName;
    private readonly JsonSerializerOptions _jsonSerializerOptions = JsonSerializerOptions.Web;
    private readonly bool _saveRawMessages = _kafkaOptions.Consumers?.SaveRawMessages ?? false;

    public void InitialiseEventConsumer(
        CancellationToken cancellationToken
    )
    {
        if (_kafkaConsumerDefinition.PreInitProcessHandlers.Count is not 0)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            _kafkaConsumerDefinition.PreInitProcessHandlers.ForEach(handlerType =>
            {
                var handlerInstance = (IKafkaConsumerPreInitProcessHandler)serviceProvider.GetRequiredService(handlerType);

                handlerInstance.Run(_kafkaConsumerDefinition);
            });
        }

        var topicToConsume = GetTopicToConsume();
        var clientGroupId = GetClientGroupId();
        _consumer = BuildConsumer(
            topicToConsume,
            clientGroupId
        );

        MetadataRefresh(_consumer.Handle);

        _consumer.Subscribe(topicToConsume);

        var consumerTask = Task.Run(() =>
        {
            try
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation(
                        "Starting consumer on topic {topicToConsume} - {TraceType}",
                        topicToConsume,
                        Constants.CONSUMER_UTILITY_TRACE_TYPE
                    );
                }

                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        var cr = _consumer.Consume(TimeSpan.FromMilliseconds(1000));

                        if (cr is not null && cr.Topic != "__consumer_offsets")
                        {
                            LogIncomingEvent(
                                cr,
                                topicToConsume
                            );

                            Task.Run(async () =>
                            {
                                try
                                {
                                    var result = await ProcessKafkaEvent(
                                        cr
                                    );

                                    if (result == KafkaEventResult.Complete)
                                    {
                                        if (_logger.IsEnabled(LogLevel.Information))
                                        {
                                            _logger.LogInformation(
                                                "Kafka consumer for topic {KafkaTopic} successfully consumed message - {TraceType}",
                                                cr.Topic,
                                                Constants.CONSUMER_UTILITY_TRACE_TYPE
                                            );
                                        }

                                        _consumer.StoreOffset(cr);
                                    }
                                    else
                                    {
                                        _logger.LogWarning(
                                            "Kafka consumer for topic {KafkaTopic} failed to complete processing - {TraceType}",
                                            cr.Topic,
                                            Constants.CONSUMER_UTILITY_TRACE_TYPE
                                        );

                                        throw new Exception();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(
                                        ex,
                                        "Kafka consumer for topic {KafkaTopic} Exception occurred whilst processing message - {TraceType}",
                                        cr.Topic,
                                        Constants.CONSUMER_UTILITY_TRACE_TYPE
                                    );

                                    throw;
                                }
                            });
                        }
                    }
                    catch (ConsumeException e)
                    {
                        //We can get this when we consume from a queue not yet created.
                        //The first message sent to that queue will then create the message
                        if (!e.Message.Contains("Broker: Unknown topic or partition"))
                        {
                            _logger.LogWarning(
                                "Subscription to topic '{topicToConsume}' has raised an exception, but will continue until stopped - {TraceType}",
                                topicToConsume,
                                Constants.CONSUMER_UTILITY_TRACE_TYPE
                            );
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Close and Release all the resources held by this consumer
                _logger.LogError(
                    "Subscription to topic '{topicToConsume}' has been stopped.",
                    topicToConsume
                );
                _consumer.Close();
                _consumer.Dispose();
            }
        }, cancellationToken);
    }

    public async Task PauseTopicPartition(
        string topicName,
        int partitionNo,
        TimeSpan delay
    )
    {
        var assignedPartitions = _consumer.Assignment;
        var matchedPartition = assignedPartitions.FirstOrDefault(
            partition => partition.Topic == topicName
                && partition.Partition == partitionNo
        );

        if (matchedPartition is null)
            throw new Exception("There may be trouble ahead!");

        _consumer.Pause([ matchedPartition ]);
        await Task.Delay(delay);
        _consumer.Seek(new(
            matchedPartition,
            _consumer.GetWatermarkOffsets(matchedPartition).High - 1
        ));
        _consumer.Resume([ matchedPartition ]);
    }

    private void LogIncomingEvent(
        ConsumeResult<string, string> consumeResult,
        string topic
    )
    {
        if (_saveRawMessages)
        {
            File.WriteAllText(
                Path.Join(_kafkaOptions.Consumers!.RawMessageFolder!, $"{Guid.NewGuid()}.json"),
                consumeResult.Message.Value
            );
        }

        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogInformation(
                "Message received from topic {topicToConsume}, Beginning processing - {TraceType}",
                topic,
                Constants.CONSUMER_UTILITY_TRACE_TYPE
            );
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace(
                    "Message details are: Headers: {Headers}, Key: {Key}, Value: {Value} - {TraceType}",
                    consumeResult.Message.Headers,
                    consumeResult.Message.Key,
                    consumeResult.Message.Value,
                    Constants.CONSUMER_UTILITY_TRACE_TYPE
                );
            }
        }
    }

    private async Task<KafkaEventResult> ProcessKafkaEvent(
        ConsumeResult<string, string> consumeResult
    )
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        var key = typeof(TKey) == typeof(Ignore)
            ? default
                : typeof(TKey) == typeof(string)
                    ? consumeResult.Message.Key is TKey stringKey ? stringKey : default
                    : JsonSerializer.Deserialize<TKey>(consumeResult.Message.Key, _jsonSerializerOptions);
        var value = typeof(TValue) == typeof(Ignore)
            ? default
                : typeof(TValue) == typeof(string)
                    ? consumeResult.Message.Value is TValue stringValue ? stringValue : default
                    : JsonSerializer.Deserialize<TValue>(consumeResult.Message.Value, _jsonSerializerOptions);

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace(
                "Strongly typed kafka message details are: Key ({KeyType}): {Key}, Value ({ValueType}): {Value} - {TraceType}",
                typeof(TKey),
                key,
                typeof(TValue),
                value,
                Constants.CONSUMER_UTILITY_TRACE_TYPE
            );
        }

        async Task<KafkaEventResult> handlerDelegate(KafkaConsumerContext kafkaConsumerContext) =>
            await _kafkaConsumerDefinition.Handler.Invoke(
                kafkaConsumerContext.ServiceProvider,
                kafkaConsumerContext
            );

        var context = new KafkaConsumerContext(
            this,
            consumeResult.Topic,
            consumeResult.Partition.Value,
            new(consumeResult.Message.Headers),
            key,
            typeof(TKey),
            value,
            typeof(TValue),
            _kafkaConsumerDefinition.CustomPropertyHandler,
            handlerDelegate,
            serviceProvider
        );

        return await middlewareHandler.RunPipeline(context);
    }

    private string GetTopicToConsume()
        => _kafkaConsumerDefinition.UseRegexPatternMatching
            ? $"^{_kafkaConsumerDefinition.TopicToMatch}"
            : _kafkaConsumerDefinition.TopicToMatch;

    private string GetClientGroupId()
        => _kafkaConsumerDefinition.IsolationMode switch
        {
            IsolationMode.PerServiceInstance => Guid.NewGuid().ToString(),
            IsolationMode.PerServiceType => _serviceName,
            _ => throw new NotSupportedException($"Isolation level {_kafkaConsumerDefinition.IsolationMode} not currently supported.")
        };

    private IConsumer<string, string> BuildConsumer(
        string topicToConsume,
        string clientGroupId
    )
    {
        if (_kafkaConsumerDefinition.CreateTopic)
        {
            if (_kafkaConsumerDefinition.UseRegexPatternMatching)
            {
                throw new InvalidOperationException(
                    "Cannot create topic when using regex pattern matching."
                );
            }

            _topicManager.CreateTopic(topicToConsume).Wait();
        }

        var kafkaServerOptions = _kafkaOptions.Server;
        var consumerBuilder = new ConsumerBuilder<string, string>(
            new ConsumerConfig
            {
                BootstrapServers = kafkaServerOptions.BootstrapServers,
                SecurityProtocol = Enum.Parse<SecurityProtocol>(kafkaServerOptions.SecurityProtocol),
                GroupId = clientGroupId,
                MetadataMaxAgeMs = 5000,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoOffsetStore = false,
                EnableAutoCommit = true,
                ApiVersionRequest = false,
                TopicMetadataRefreshIntervalMs = 5000
            });

        return consumerBuilder.Build();
    }

    private static void MetadataRefresh(Handle handle)
    {
        using var client = new DependentAdminClientBuilder(handle).Build();

        client.GetMetadata(TimeSpan.FromMilliseconds(5000));
    }
}
