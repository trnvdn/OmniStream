namespace OmniStream.Contracts.Data;

public record MetricEnvelope
{
    public Guid DeviceId { get; init; }
    public string MetricType { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    public object Payload { get; init; } = default!;
}