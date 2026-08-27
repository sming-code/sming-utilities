namespace SmingCode.Utilities.Messaging.Retries;

public class IncrementalRetryPattern(
    int initialWaitInSeconds,
    int maxNoRetries
) : IMessageRetryPattern
{
    public List<int> GetRetryDelaysInSeconds()
    {
        // We'll use bit shifting to perform the equivalent of '2 to the power of..'
        var retryTimes = Enumerable.Range(0, maxNoRetries)
            .Select(retryNo => initialWaitInSeconds * (1 << retryNo))
            .ToList();

        return retryTimes;
    }
}