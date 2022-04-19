using System.Buffers;
using System.IO.Compression;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Domain.Interactors.DownloadDB;
using Microsoft.Extensions.Logging;

namespace Data.Interactors;

public class DownloadDbInteractor : IInteractor
{
  private const string DownloadUrl =
    "https://github.com/sovathna/Khmer-Dictionary/raw/master/db/room_sqlite.zip";

  private const string DatabasePath = "data/dict.db";

  private readonly HttpClient _client;
  private readonly ILogger _logger;
  private readonly DefaultScheduler _bgScheduler;
  private readonly SynchronizationContext _mainContext;

  public DownloadDbInteractor(
    HttpClient client,
    ILogger<DownloadDbInteractor> logger,
    DefaultScheduler bgScheduler,
    SynchronizationContext mainContext
  )
  {
    _client = client;
    _logger = logger;
    _bgScheduler = bgScheduler;
    _mainContext = mainContext;
  }

  public IObservable<IResult> Invoke()
  {
    return Observable.Create(new Func<IObserver<IResult>, IDisposable>(
          observer =>
          {
            observer.OnNext(new Loading());
            if (File.Exists(DatabasePath))
            {
              observer.OnNext(new Done());
              // observer.OnNext(new Failure(new Exception("custom error")));
              observer.OnCompleted();
              return Disposable.Empty;
            }

            if (!Directory.Exists("data"))
            {
              Directory.CreateDirectory("data");
            }
            
            var request = new HttpRequestMessage();
            request.RequestUri = new Uri(DownloadUrl);
            request.Method = HttpMethod.Get;
            var response = _client.Send(request);

            var responseStream = response.Content.ReadAsStream();

            var zipStream = new ZipArchive(responseStream);
            foreach (var entry in zipStream.Entries)
            {
              var stream = entry.Open();
              var fileStream = File.Create(DatabasePath);
              var ratio = (float)entry.Length / entry.CompressedLength;
              var l = entry.Length / 1_000_000F / ratio;
              var totalRead = 0L;
              var buffer = ArrayPool<byte>.Shared.Rent(8192);
              while (true)
              {
                var read = stream.Read(buffer, 0, buffer.Length);
                if (read <= 0) break;
                fileStream.Write(buffer, 0, read);
                totalRead += read;
                var tmpRead = totalRead / 1_000_000F / ratio;
                var str =
                  $"progress: {tmpRead.ToString("F")} of {l.ToString("F")}MB";
                _logger.LogInformation("{Str}", str);
                observer.OnNext(new Downloading(tmpRead, l));
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
            observer.OnNext(new Done());
            observer.OnCompleted();
            return Disposable.Empty;
          }
        )
      ).Buffer(TimeSpan.FromMilliseconds(500))
      .Select(o => o.LastOrDefault() ?? new Loading())
      .SubscribeOn(_bgScheduler)
      .ObserveOn(_mainContext);
  }
}