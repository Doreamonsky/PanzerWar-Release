namespace ApkDownload.Shared.Downloader;

public class FileDownloaderProgressReport
{
    public long TotalBytes { get; init; }
    public long BytesDownloaded { get; set; }
    public double ProgressPercentage => (double)BytesDownloaded / TotalBytes;
}