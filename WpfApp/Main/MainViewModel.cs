using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using WpfApp.Splash;

namespace WpfApp.Main;

public class MainViewModel : ObservableObject
{
  private readonly ILogger _logger;
  private readonly DefaultScheduler _bgScheduler;
  private readonly SynchronizationContext _mainContext;
  private IDisposable? _disposable;
  private readonly CompositeDisposable _disposables;

  private static readonly BehaviorSubject<MainState> States =
    new(new MainState());

  public MainState BindingStates => States.Value;

  public MainViewModel(ILogger<MainViewModel> logger,
    DefaultScheduler bgScheduler, SynchronizationContext mainContext,
    CompositeDisposable disposables)
  {
    _logger = logger;
    _bgScheduler = bgScheduler;
    _mainContext = mainContext;
    _disposables = disposables;
    Observable.Interval(TimeSpan.FromMilliseconds(500))
      .Subscribe(value =>
      {
        var newList = States.Value.words.ToList();
        newList.Add($"{value}");
        States.OnNext(States.Value with { words = newList });
      });
  }
}