namespace Canopy.Utils.Future;

public interface ITask
{
    bool IsComplete { get; }
    void OnCompleted(Action callback);
}
