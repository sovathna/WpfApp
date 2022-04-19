namespace Domain.Interactors.DownloadDB;

public interface IInteractor
{
  public IObservable<IResult> Invoke();
}

public interface IResult
{
}

public readonly struct Loading : IResult
{
}

public record struct Failure(Exception Exception) : IResult
{
}

public record struct Downloading(float Downloaded, float Total) : IResult
{
}

public readonly struct Done : IResult
{
}