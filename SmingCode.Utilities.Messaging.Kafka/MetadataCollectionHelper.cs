using System.Text;

namespace SmingCode.Utilities.Messaging.Kafka;

internal static class MetadataCollectionHelper
{
    internal static MetadataCollection GetMetadataCollection(
        this Headers kafkaHeaders
    )
    {
        return new(
            kafkaHeaders
                .ToDictionary(
                    kafkaHeader => kafkaHeader.Key,
                    kafkaHeader => Encoding.UTF8.GetString(kafkaHeader.GetValueBytes())
                )
        );
    }

    public static Headers ToKafkaHeaders(
        this MetadataCollection metadataCollection
    ) => [
            .. metadataCollection.Select(metadataEntry =>
                new Header(metadataEntry.Key, Encoding.UTF8.GetBytes(metadataEntry.Value))
            )
        ];
}