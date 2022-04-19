using System;
using System.Collections.Generic;
using System.Windows.Documents;

namespace WpfApp.Main;

public readonly record struct MainState(IList<string> words)
{
  public MainState() : this(Array.Empty<string>())
  {
  }
}