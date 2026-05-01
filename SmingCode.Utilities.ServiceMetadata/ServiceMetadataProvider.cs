namespace SmingCode.Utilities.ServiceMetadata;

internal class ServiceMetadataProvider : IServiceMetadataProvider
{
    private const string SERVICE_NAME_ENVIRONMENT_VARIABLE = "Service_Name";

    private static readonly Metadata _metadata = new(
        Environment.GetEnvironmentVariable(SERVICE_NAME_ENVIRONMENT_VARIABLE)
            ?? throw new InvalidOperationException($"{SERVICE_NAME_ENVIRONMENT_VARIABLE} environment variable has not been set."),
        Guid.NewGuid()
    );

    public Metadata GetMetadata() => _metadata;
}
