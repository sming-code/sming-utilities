namespace SmingCode.Utilities.Messaging;

internal class HostedServiceOptions
{
    public int LivenessLogIntervalSeconds { get; set; } = 30;
    public bool SaveRawMessages { get; set; } = false;
    public string? RawMessageFolder { get; set; }
}