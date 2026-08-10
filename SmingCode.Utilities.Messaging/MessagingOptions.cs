namespace SmingCode.Utilities.Messaging;

internal class MessagingOptions
{
    public bool SaveRawMessages { get; set; } = false;
    public string? RawMessageFolder { get; set; }
}