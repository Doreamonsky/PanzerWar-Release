using Newtonsoft.Json;

namespace ApkDownload.Shared.Downloader;

public class ApkFileDetails
{
    [JsonProperty("file_name")] public string FileName { get; set; }

    [JsonProperty("file_md5")] public string FileMd5 { get; set; }

    [JsonProperty("parts")] public List<DownloadFileInfo> Parts { get; set; }
}

public class DownloadFileInfo
{
    [JsonProperty("part_number")] public int PartNumber { get; set; }

    [JsonProperty("file_name")] public string FileName { get; set; }

    [JsonProperty("url")] public string Url { get; set; }

    [JsonProperty("size")] public long Size { get; set; }
}