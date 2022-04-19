using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace WpfApp.Main;

public partial class MainPage
{
  public MainPage(MainViewModel viewModel)
  {
    InitializeComponent();
    DataContext = viewModel;
  }
}