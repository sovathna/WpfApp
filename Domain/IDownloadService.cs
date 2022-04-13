namespace Domain;

public interface IDownloadService
{
  IObservable<IGetAllResult> GetAll();
}