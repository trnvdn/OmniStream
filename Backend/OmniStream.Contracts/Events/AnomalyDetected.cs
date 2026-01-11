using OmniStream.Contracts.Enum;

namespace OmniStream.Contracts.Events
{
    public record AnomalyDetected
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public Guid DeviceId { get; init; }

        public string MetricType { get; init; } = string.Empty;
        public double Value { get; init; }
        public string Message { get; init; } = string.Empty;
        public DateTime DetectedAt { get; init; } = DateTime.UtcNow;
        public AlertSeverity Severity { get; init; } = AlertSeverity.Info;
    }
}
