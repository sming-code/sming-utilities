namespace SmingCode.Utilities.ProcessTracking;

public record ProcessTrackingDetail(
    string CorrelationId,
    string ProcessId,
    string ProcessName
);
