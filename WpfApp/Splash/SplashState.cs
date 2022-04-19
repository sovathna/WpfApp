using System;

namespace WpfApp.Splash;

public readonly record struct SplashState(
  bool IsLoading,
  float Downloaded,
  float Total,
  bool IsDone,
  Exception? Exception
)
{
  public SplashState() : this(false, 0, 0, false, null)
  {
  }
};