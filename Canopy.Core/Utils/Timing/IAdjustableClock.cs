namespace Canopy.Utils.Timing;

public interface IAdjustableClock
{
    void Reset();

    void Start();

    void Stop();

    bool Seek(double position);

    double Rate { get; set; }

    void ResetSpeedAdjustments();
}
