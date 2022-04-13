using System;
using System.Windows;
using Domain;
using Microsoft.Extensions.Logging;

namespace WpfApp;

/// <summary>
///   Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow 
{
  private readonly ILogger _logger;
  private readonly IDownloadService _service;

  public MainWindow(IDownloadService service, ILogger<MainWindow> logger)
  {
    InitializeComponent();
    _logger = logger;
    _service = service;
    Title = "Download";
  }

  private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
  {
    _logger.LogInformation("MainWindow initialized");
    _service.GetAll()
      .Subscribe(result =>
      {
        switch (result)
        {
          case GetAllResultLoading:
            _logger.LogInformation("{Result}", result);
            break;
          case GetAllResultSuccess res:
            _logger.LogInformation("{PageContent}", res.Page);
            TextBlockSub.Text = res.Page;
            break;
          case GetAllResultFailure res:
            _logger.LogError("{ErrorMessage}", res.Exception.Message);
            break;
        }
      });
  }
}