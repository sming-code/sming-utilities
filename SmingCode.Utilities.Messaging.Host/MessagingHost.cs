namespace SmingCode.Utilities.Messaging.Host;

public static class MessagingHost
{
    public static MessagingApplicationBuilder CreateApplicationBuilder() => new();
    public static MessagingApplicationBuilder CreateApplicationBuilder(string[]? args) => new (args);
}
