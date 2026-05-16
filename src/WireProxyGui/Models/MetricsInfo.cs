namespace WireProxyGui.Models;

public sealed class MetricsInfo
{
    public bool IsAvailable { get; init; }
    public long RxBytes { get; init; }
    public long TxBytes { get; init; }
}
