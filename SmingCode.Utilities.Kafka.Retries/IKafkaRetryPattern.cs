namespace SmingCode.Utilities.Kafka.Retries;

public interface IKafkaRetryPattern
{
    List<int> GetRetryDelaysInSeconds();
}
