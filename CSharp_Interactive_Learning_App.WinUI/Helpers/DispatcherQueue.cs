namespace CSharp_Interactive_Learning_App.WinUI.Helpers;

public sealed class DispatcherQueue(Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue) : IDispatcherQueue
{
    public void TryEnqueue(Action action)
    {
        dispatcherQueue.TryEnqueue(() => action());
    }
}

public interface IDispatcherQueue
{
    public void TryEnqueue(Action action);
}
