using Microsoft.Extensions.DependencyInjection;
using WpfApp.Splash;

namespace WpfApp;

/// <summary>
///   Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
  public MainWindow()
  {
    InitializeComponent();
    var splashPage = App.AppHost.Services.GetRequiredService<SplashPage>();
    MainFrame.Navigate(splashPage);
  }
}