namespace Domain;

public interface IGetAllResult
{
}

public readonly struct GetAllResultLoading : IGetAllResult
{
}

public readonly struct GetAllResultSuccess : IGetAllResult
{
  public GetAllResultSuccess(string page)
  {
    Page = page;
  }

  public string Page { get; init; }
}

public readonly struct GetAllResultFailure : IGetAllResult
{
  public GetAllResultFailure(Exception exception)
  {
    Exception = exception;
  }

  public Exception Exception { get; init; }
}