using System;
using System.Net.Http;
using System.Reactive.Concurrency;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Data;
using Data.Interactors;
using Domain;
using Domain.Interactors.DownloadDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WpfApp.Main;
using WpfApp.Splash;

namespace WpfApp;

/// <summary>
///   Interaction logic for App.xaml
/// </summary>
public partial class App
{
  public static readonly IHost AppHost = Host.CreateDefaultBuilder()
    .ConfigureServices((_, services) =>
      {
        services.AddSingleton<SynchronizationContext>(
          new DispatcherSynchronizationContext(
            Dispatcher.CurrentDispatcher,
            DispatcherPriority.Background)
        );
        services.AddSingleton(DefaultScheduler.Instance);
        
        var client = new HttpClient();
        client.BaseAddress = new Uri("https://www.example.com/");
        client.DefaultRequestHeaders.UserAgent.ParseAdd("wpf-app");
        services.AddSingleton(client);

        services.AddSingleton<MainWindow>();
        
        //Splash Page
        services.AddTransient<IInteractor, DownloadDbInteractor>();
        services.AddTransient<SplashViewModel>();
        services.AddTransient<SplashPage>();

        services.AddSingleton<MainPage>();
        services.AddTransient<MainViewModel>();
      }
    )
    .Build();

  public App()
  {
    AppHost.RunAsync();
  }

  private void App_OnStartup(object sender, StartupEventArgs e)
  {
    var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
    mainWindow.Show();
  }
}