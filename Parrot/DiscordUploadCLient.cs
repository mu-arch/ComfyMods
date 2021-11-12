using System;
using System.Collections.Specialized;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Parrot {
  public class DiscordUploadClient {
    readonly WebClient _webClient = new();
    readonly Uri _webhookUri;

    readonly AsyncQueue<NameValueCollection> _uploadQueue = new();
    readonly CancellationTokenSource _uploadLoopCancellation = new();

    public DiscordUploadClient(string webhookUrl) {
      _webhookUri = new(webhookUrl);
    }

    public void Start() {
      Task.Run(UploadLoopAsync, _uploadLoopCancellation.Token).ConfigureAwait(false);
    }

    public void Stop() {
      _uploadLoopCancellation.Cancel();
    }

    public void Upload(NameValueCollection values) {
      _uploadQueue.Enqueue(values);
    }

    async Task UploadLoopAsync() {
      while (true) {
        NameValueCollection values = await _uploadQueue.Dequeue();
        await _webClient.UploadValuesTaskAsync(_webhookUri, values);
      }
    }
  }
}
