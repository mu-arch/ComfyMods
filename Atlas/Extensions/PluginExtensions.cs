using System.Collections;
using System.Threading.Tasks;

namespace Atlas {
  public static class TaskExtensions {
    public static IEnumerator ToIEnumerator(this Task task) {
      while (!task.IsCompleted) {
        yield return null;
      }

      if (task.IsFaulted) {
        ZLog.Log($"Task failed: {task.Exception}");
      }
    }
  }
}
