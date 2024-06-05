using Newtonsoft.Json;

namespace ApkDownload.Shared.Downloader;

public partial class FileDownloader
{
    public async Task<ApkFileDetails> FetchFileListAsync(string url)
    {
        using var httpClient = new HttpClient();
        var json = await httpClient.GetStringAsync(url);
        return JsonConvert.DeserializeObject<ApkFileDetails>(json);
    }
}