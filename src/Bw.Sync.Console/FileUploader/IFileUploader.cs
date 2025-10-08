namespace Bw.Sync.Console.FileUploader;

public interface IFileUploader
{
    Task Upload(string path, Stream data, CancellationToken cancellationToken);
}