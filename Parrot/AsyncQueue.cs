using System.Collections.Generic;
using System.Threading.Tasks;

namespace Parrot {
  public class AsyncQueue<T> {
    readonly Queue<T> _queueItems = new();
    readonly Queue<TaskCompletionSource<T>> _waitingConsumers = new();

    public AsyncQueue<T> Enqueue(T item) {
      lock (_queueItems) {
        if (_waitingConsumers.Count > 0) {
          TaskCompletionSource<T> source = _waitingConsumers.Dequeue();
          source.TrySetResult(item);
        } else {
          _queueItems.Enqueue(item);
        }
      }

      return this;
    }

    public Task<T> Dequeue() {
      lock (_queueItems) {
        if (_queueItems.Count > 0) {
          return Task.FromResult(_queueItems.Dequeue());
        }

        TaskCompletionSource<T> source = new();
        _waitingConsumers.Enqueue(source);

        return source.Task;
      }
    }
  }
}
