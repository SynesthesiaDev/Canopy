namespace Canopy.Utils.Future;

public class TaskTask<T> : ITask
{
    private T? result;
    private Exception? exception;

    private readonly List<Action<T>> successCallbacks = [];
    private readonly List<Action<Exception>> failCallbacks = [];
    private readonly List<Action> anyCompletionCallbacks = [];

    public bool IsComplete { get; private set; }

    public TaskTask<T> Then(Action<T> then)
    {
        if (IsComplete && exception == null)
        {
            then.Invoke(result!);
        }
        else
        {
            successCallbacks.Add(then);
        }

        return this;
    }

    public void Complete(T value)
    {
        if (IsComplete) return;

        result = value;
        IsComplete = true;
        successCallbacks.ForEach(a => a.Invoke(value));
        anyCompletionCallbacks.ForEach(a => a.Invoke());
    }

    public void Fail(Exception ex)
    {
        if (IsComplete) return;
        exception = ex;
        IsComplete = true;
        failCallbacks.ForEach(a => a.Invoke(ex));
        anyCompletionCallbacks.ForEach(a => a.Invoke());
    }


    public void OnCompleted(Action callback)
    {
        if (IsComplete) callback();
        else anyCompletionCallbacks.Add(callback);
    }

    public void OnSuccess(Action<T> callback)
    {
        if (IsComplete && exception == null && result != null) callback.Invoke(result!);
        else successCallbacks.Add(callback);
    }

    public void OnFail(Action<Exception> callback)
    {
        if (IsComplete && exception != null) callback.Invoke(exception);
        else failCallbacks.Add(callback);
    }
}

public static class Tasks
{
    public static TaskTask<T> RunAsync<T>(Func<T> action)
    {
        var future = new TaskTask<T>();

        Task.Run(() =>
        {
            try
            {
                var result = action();
                future.Complete(result);
            }
            catch (Exception ex)
            {
                future.Fail(ex);
            }
        });

        return future;
    }

    public static TaskTask<Nothing> RunAsync(Action action)
    {
        return RunAsync(() =>
        {
            action();
            return Nothing.INSTANCE;
        });
    }

    public static TaskTask<T> Completed<T>(T value)
    {
        var future = new TaskTask<T>();
        future.Complete(value);
        return future;
    }

    public static TaskTask<Nothing> All(params ITask[] futures)
    {
        var returnedPromise = new TaskTask<Nothing>();
        if (futures.Length == 0)
        {
            returnedPromise.Complete(Nothing.INSTANCE);
            return returnedPromise;
        }

        var remaining = futures.Length;
        foreach (var future in futures)
        {
            future.OnCompleted(() =>
            {
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    returnedPromise.Complete(Nothing.INSTANCE);
                }
            });
        }

        return returnedPromise;
    }
}
