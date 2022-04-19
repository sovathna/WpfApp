using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using Domain.Interactors.DownloadDB;
using Microsoft.Extensions.Logging;

namespace WpfApp.Splash;

public sealed class SplashViewModel : IDisposable
{
  private readonly ILogger _logger;
  private readonly DefaultScheduler _bgScheduler;
  private readonly SynchronizationContext _mainContext;
  private readonly IInteractor _interactor;
  private IDisposable? _disposable;
  private readonly CompositeDisposable _disposables;

  private static readonly BehaviorSubject<SplashState> States =
    new(new SplashState());

  public SplashViewModel(ILogger<SplashViewModel> logger,
    DefaultScheduler bgScheduler, SynchronizationContext mainContext,
    IInteractor interactor)
  {
    _logger = logger;
    _bgScheduler = bgScheduler;
    _mainContext = mainContext;
    _interactor = interactor;
    _disposables = new CompositeDisposable();
    _logger.LogInformation("initialized");
  }

  private static void SetState(SplashState state)
  {
    States.OnNext(state);
  }

  public void Download()
  {
    _logger.LogInformation("download");
    var disposable = _interactor.Invoke().Subscribe(res =>
    {
      switch (res)
      {
        case Loading:
          _logger.LogInformation("result: loading");
          SetState(States.Value with
          {
            IsLoading = true, Exception = null
          });
          break;
        case Downloading result:
          _logger.LogInformation("result: {Result}", result);
          SetState(States.Value with
          {
            Downloaded = result.Downloaded, Total = result.Total,
            IsLoading = false
          });
          break;
        case Done:
          _logger.LogInformation("result: done");
          SetState(States.Value with
          {
            Downloaded = States.Value.Total, IsDone = true, IsLoading = false
          });
          break;
        case Failure result:
          _logger.LogError("result: {Exception}", result.Exception);
          SetState(States.Value with
          {
            Exception = result.Exception, IsLoading = false
          });
          break;
      }
    });
    _disposables.Add(disposable);
  }

  public void Subscribe(Action<SplashState> onNext)
  {
    _logger.LogInformation("subscribe");
    _disposable = States
      .DistinctUntilChanged()
      .SubscribeOn(_bgScheduler)
      .ObserveOn(_mainContext)
      .Subscribe(onNext);
  }

  public void Unsubscribe()
  {
    _logger.LogInformation("unsubscribe");
    _disposable?.Dispose();
  }

  public void Dispose()
  {
    _logger.LogInformation("dispose");
    Unsubscribe();
    States.Dispose();
    _disposables.Dispose();
  }
}