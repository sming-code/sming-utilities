namespace SmingCode.Utilities.Kafka.Consumers;

[AttributeUsage(AttributeTargets.Parameter)]
public class FromHeaderValueAttribute(
    string headerName
) : Attribute
{
    public string HeaderName { get; } = headerName;
}
