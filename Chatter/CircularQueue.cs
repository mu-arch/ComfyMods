using System;
using System.Collections.Concurrent;

namespace Chatter {
  public class CircularQueue<T> : ConcurrentQueue<T> {
    readonly int _capacity;
    readonly Action<T> _dequeueFunc;

    public CircularQueue(int capacity, Action<T> dequeueFunc) {
      _capacity = capacity;
      _dequeueFunc = dequeueFunc;
    }

    public void EnqueueItem(T item) {
      while (Count + 1 > _capacity) {
        if (TryDequeue(out T itemToDequeue)) {
          _dequeueFunc(itemToDequeue);
        }
      }

      Enqueue(item);
    }
  }
}
