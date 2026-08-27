using System.Text.Json;

namespace SmingCode.Utilities.Messaging.Kafka.Consumers;
using Config;
using ServiceMetadata;

internal class KafkaMessageConsumer<TBody>(
    IServiceScopeFactory _serviceScopeFactory,
    IKafkaAdminClient _kafkaAdminClient,
    MessagingConsumerDefinition<TBody> _messagingConsumerDefinition,
    IServiceMetadataProvider serviceMetadataProvider,
    KafkaOptions _kafkaOptions,
    ConsumerMiddlewareHandler middlewareHandler,
    KafkaConsumerPrerequisiteOptions _preRequisiteOptions,
    ILogger<KafkaMessageConsumer<TBody>> _logger
) : IMessageConsumer
    where TBody : notnull
{
    private IConsumer<string, string> _consumer = null!;
    private readonly string _serviceName = serviceMetadataProvider.GetMetadata().ServiceName;
    private readonly JsonSerializerOptions _jsonSerializerOptions = JsonSerializerOptions.Web;
    private readonly bool _saveRawMessages = _kafkaOptions.Consumers?.SaveRawMessages ?? false;

    public Guid ConsumerId => _messagingConsumerDefinition.ConsumerId;
    
    public async Task InitialiseEventConsumer(
        CancellationToken cancellationToken
    )
    {
        List<Type> preInitProcessHandlers = [
            .. _preRequisiteOptions.GlobalPrerequisiteHandlers,
            .. _preRequisiteOptions.GetConsumerSpecificPrerequisiteHandlers(_messagingConsumerDefinition.ConsumerId)
        ];

        if (preInitProcessHandlers.Count is not 0)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            preInitProcessHandlers.ForEach(handlerType =>
            {
                var handlerInstance = (IKafkaConsumerPreInitProcessHandler)serviceProvider.GetRequiredService(handlerType);

                handlerInstance.Run(_messagingConsumerDefinition);
            });
        }

        var topicToConsume = GetTopicToConsume();
        if (!_messagingConsumerDefinition.UseRegexPatternMatching)
        {
            await _kafkaAdminClient.CreateTopic(topicToConsume);
        }
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
                                    await ProcessKafkaEvent(
                                        cr
                                    );

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
                                catch (Exception ex)
                                {
                                    _logger.LogError(
                                        ex,
                                        "Unmanaged exception occurred in Kafka consumer for topic {KafkaTopic} whilst processing message - {TraceType}",
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
        ) ?? throw new Exception("There may be trouble ahead!");

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

    private async Task ProcessKafkaEvent(
        ConsumeResult<string, string> consumeResult
    )
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        var value = typeof(TBody) == typeof(Ignore)
            ? default
                : typeof(TBody) == typeof(string)
                    ? consumeResult.Message.Value is TBody stringValue ? stringValue : default
                    : JsonSerializer.Deserialize<TBody>(consumeResult.Message.Value, _jsonSerializerOptions);

        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace(
                "Strongly typed kafka message details are: Body ({BodyType}): {BodyValue} - {TraceType}",
                typeof(TBody),
                value,
                Constants.CONSUMER_UTILITY_TRACE_TYPE
            );
        }

        async Task handlerDelegate(MessagingConsumerContext messagingConsumerContext) =>
            await _messagingConsumerDefinition.Handler.Invoke(
                messagingConsumerContext.ServiceProvider,
                messagingConsumerContext
            );

        var context = new KafkaConsumerContext(
            this,
            consumeResult.Topic,
            consumeResult.Partition.Value,
            consumeResult.Message.Headers.GetMetadataCollection(),
            value,
            typeof(TBody),
            _messagingConsumerDefinition.CustomPropertyHandler,
            handlerDelegate,
            serviceProvider
        );

        await middlewareHandler.RunPipeline(context);
    }

    private string GetTopicToConsume()
        => _messagingConsumerDefinition.UseRegexPatternMatching
            ? $"^{_messagingConsumerDefinition.TopicToMatch}"
            : _messagingConsumerDefinition.TopicToMatch;

    private string GetClientGroupId()
        => _messagingConsumerDefinition.IsolationMode switch
        {
            IsolationMode.PerServiceInstance => Guid.NewGuid().ToString(),
            IsolationMode.PerServiceType => _serviceName,
            _ => throw new NotSupportedException($"Isolation level {_messagingConsumerDefinition.IsolationMode} not currently supported.")
        };

    private IConsumer<string, string> BuildConsumer(
        string topicToConsume,
        string clientGroupId
    )
    {
        if (_messagingConsumerDefinition.CreateTopic)
        {
            if (_messagingConsumerDefinition.UseRegexPatternMatching)
            {
                throw new InvalidOperationException(
                    "Cannot create topic when using regex pattern matching."
                );
            }

            _kafkaAdminClient.CreateTopic(topicToConsume).Wait();
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
