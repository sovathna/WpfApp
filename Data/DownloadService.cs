using System.Buffers;
using System.IO.Compression;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Domain;
using Microsoft.Extensions.Logging;

namespace Data;

public sealed class DownloadService : IDownloadService
{
  private static readonly string DOWNLOAD_URL =
    "https://github.com/sovathna/Khmer-Dictionary/raw/master/db/room_sqlite.zip";

  private readonly HttpClient _client;
  private readonly ILogger _logger;
  private readonly DefaultScheduler _bgScheduler;
  private readonly SynchronizationContext _mainContext;

  public DownloadService(HttpClient client, ILogger<DownloadService> logger,
    DefaultScheduler bgScheduler, SynchronizationContext mainContext)
  {
    _client = client;
    _logger = logger;
    _bgScheduler = bgScheduler;
    _mainContext = mainContext;
  }

  IObservable<IGetAllResult> IDownloadService.GetAll()
  {
    return Observable.Create(new Func<IObserver<IGetAllResult>, IDisposable>(
          observer =>
          {
            observer.OnNext(new GetAllResultLoading());
            var request = new HttpRequestMessage();
            request.RequestUri = new Uri(DOWNLOAD_URL);
            request.Method = HttpMethod.Get;
            var response = _client.Send(request);

            var responseStream = response.Content.ReadAsStream();

            var zipStream = new ZipArchive(responseStream);
            foreach (var entry in zipStream.Entries)
            {
              var stream = entry.Open();
              var fileStream = File.Create("dict.db");
              var l = entry.Length / 1_000_000D;
              var totalRead = 0L;
              var buffer = ArrayPool<byte>.Shared.Rent(8192);
              while (true)
              {
                var read = stream.Read(buffer, 0, buffer.Length);
                if (read <= 0) break;
                fileStream.Write(buffer, 0, read);
                totalRead += read;
                var tmpRead = totalRead / 1_000_000D;
                var str = $"progress: {tmpRead.ToString("F")} of {l.ToString("F")}MB";
                _logger.LogInformation("{Str}", str);
                observer.OnNext(new GetAllResultSuccess(str));
              }

              stream.Dispose();
              fileStream.Flush();
              fileStream.Dispose();
              ArrayPool<byte>.Shared.Return(buffer);
              break;
            }

            zipStream.Dispose();
            responseStream.Dispose();
            response.Dispose();
            request.Dispose();

            observer.OnCompleted();
            return Disposable.Empty;
          }
        )
      ).Buffer(TimeSpan.FromMilliseconds(500))
      .Select(o => o.LastOrDefault() ?? new GetAllResultSuccess("Loading..."))
      .SubscribeOn(_bgScheduler)
      .ObserveOn(_mainContext);
  }
}