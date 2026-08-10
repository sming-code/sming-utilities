namespace SmingCode.Utilities.Messaging.Consumers;

[AttributeUsage(AttributeTargets.Parameter)]
public class FromMetadataAttribute(
    string metadataPropertyName
) : Attribute
{
    public string MetadataPropertyName { get; } = metadataPropertyName;
}
