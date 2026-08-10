namespace SmingCode.Utilities.Messaging.Kafka.Retries;

public interface IKafkaRetryPattern
{
    List<int> GetRetryDelaysInSeconds();
}
