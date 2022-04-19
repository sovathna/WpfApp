using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WpfApp.Main;

namespace WpfApp.Splash;

public partial class SplashPage
{
  private readonly ILogger _logger;
  private readonly SplashViewModel _viewModel;
  private bool _isNavigated;

  public SplashPage(ILogger<SplashPage> logger,
    SplashViewModel viewModel)
  {
    InitializeComponent();
    _logger = logger;
    _viewModel = viewModel;
    Loaded += OnLoaded;
    Unloaded += OnUnloaded;
    
    _viewModel.Download();
  }

  private void OnLoaded(object sender, RoutedEventArgs e)
  {
    _viewModel.Subscribe(Render);
  }

  private void OnUnloaded(object sender, RoutedEventArgs e)
  {
    _viewModel.Unsubscribe();
  }
  

  private void Render(SplashState state)
  {
    _logger.LogInformation("{State}", state);
    if (state.Exception == null)
    {
      ProgressBar.IsIndeterminate = state.IsLoading;
      ProgressBar.Maximum = state.Total;
      ProgressBar.Value = state.Downloaded;

      if (!state.IsDone || _isNavigated) return;
      _isNavigated = true;
      var mainPage = App.AppHost.Services.GetRequiredService<MainPage>();
      NavigationService?.Navigate(mainPage);
    }
    else
    {
      var result = MessageBox.Show(
        Application.Current.MainWindow!,
        "An error has occurred! Please try again.", "Error!",
        MessageBoxButton.OKCancel, MessageBoxImage.Error,
        MessageBoxResult.Cancel);
      if (result == MessageBoxResult.OK)
      {
        _viewModel.Download();
      }
      else
      {
        Application.Current.Shutdown();
      }
    }
  }
}