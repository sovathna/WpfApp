using System;
using System.Net.Http;
using System.Reactive.Concurrency;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Data;
using Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WpfApp;

/// <summary>
///   Interaction logic for App.xaml
/// </summary>
public partial class App 
{
  private static readonly IHost AppHost = Host.CreateDefaultBuilder()
    .ConfigureServices((_, services) =>
      {
        services.AddSingleton<SynchronizationContext>(
          new DispatcherSynchronizationContext(
            Dispatcher.CurrentDispatcher,
            DispatcherPriority.Background)
        );
        services.AddSingleton(DefaultScheduler.Instance);

        services.AddSingleton<MainWindow>();
        var client = new HttpClient();
        client.BaseAddress = new Uri("https://api.icndb.com/");
        client.DefaultRequestHeaders.UserAgent.ParseAdd("wpf-app");
        services.AddSingleton(client);
        services.AddSingleton<IDownloadService, DownloadService>();
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