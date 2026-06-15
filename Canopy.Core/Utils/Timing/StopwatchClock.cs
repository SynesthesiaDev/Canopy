using System.Diagnostics;

namespace Canopy.Utils.Timing;

public sealed class StopwatchClock : Stopwatch, IAdjustableClock
{
    private double seekOffset;

    private double rateChangeUsed;

    private double rateChangeAccumulated;

    private double stopwatchMilliseconds => (double)ElapsedTicks / Frequency * 1000;

    private double stopwatchCurrentTime => (stopwatchMilliseconds - rateChangeUsed) * rate + rateChangeAccumulated;

    public double CurrentTime => stopwatchCurrentTime + seekOffset;

    private double rate = 1;

    public double Rate
    {
        get => rate;
        set
        {
            if (rate == value) return;

            rateChangeAccumulated += (stopwatchMilliseconds - rateChangeUsed) * rate;
            rateChangeUsed = stopwatchMilliseconds;

            rate = value;
        }
    }

    public StopwatchClock(bool start)
    {
        if (start) Start();
    }

    public new void Reset()
    {
        resetAccumulatedRate();
        base.Reset();
    }

    public new void Restart()
    {
        resetAccumulatedRate();
        base.Restart();
    }

    public bool Seek(double position)
    {
        seekOffset = position - stopwatchCurrentTime;
        return true;
    }

    public void ResetSpeedAdjustments() => Rate = 1;

    private void resetAccumulatedRate()
    {
        rateChangeAccumulated = 0;
        rateChangeUsed = 0;
    }
}
