namespace BobsComponents;

/// <summary>One point of a <c>ProgressChart</c> series: a moment and the value measured then.</summary>
public readonly record struct BobChartPoint(DateTimeOffset At, double Value);
