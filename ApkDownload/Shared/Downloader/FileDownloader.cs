using System.Security.Cryptography;
using ApkDownload.Shared.Interface;

namespace ApkDownload.Shared.Downloader
{
    public partial class FileDownloader
    {
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(3);


        public enum DownloadStage
        {
            Downloading,
            Merge,
            Validate
        }

        public delegate void ProgressChangedHandler(FileDownloaderProgressReport report);

        public delegate void DownloadStageHandler(DownloadStage stage);

        public async Task<bool> DownloadAndMergeFilesAsync(ApkFileDetails apkFileDetails, string outputPath,
            ProgressChangedHandler onProgressChanged, DownloadStageHandler onStageChanged)
        {
            var parts = apkFileDetails.Parts;
            var outputFile = new FileInfo(outputPath);
            var downloadFile = true;
            if (outputFile.Exists)
            {
                onStageChanged?.Invoke(DownloadStage.Validate);
                var actualMD5 = await CalculateMD5Async(outputPath);
                if (apkFileDetails.FileMd5 == actualMD5)
                {
                    downloadFile = false;
                }
            }

            if (downloadFile)
            {
                onStageChanged?.Invoke(DownloadStage.Downloading);

                var totalSize = parts.Sum(file => file.Size);
                var progressReport = new FileDownloaderProgressReport { TotalBytes = totalSize };

                var downloadTasks = parts.Select(file => DownloadFileAsync(file, progressReport, onProgressChanged))
                    .ToList();

                var cacheFiles = await Task.WhenAll(downloadTasks);
                return await MergeFilesAsync(cacheFiles, outputPath, apkFileDetails.FileMd5, onStageChanged);
            }

            return true;
        }

        private async Task<string> DownloadFileAsync(DownloadFileInfo fileInfo, FileDownloaderProgressReport fileDownloaderProgressReport,
            ProgressChangedHandler onProgressChanged)
        {
            await semaphore.WaitAsync();

            DependencyService.Get<IToast>()
                .Show($"Start download chunk: {fileInfo.FileName}");

            var localFilePath = Path.Combine(FileSystem.CacheDirectory, fileInfo.FileName);
            var localFileInfo = new FileInfo(localFilePath);
            long downloadedBytes = 0;

            try
            {
                long existingLength = 0;
                if (File.Exists(localFilePath))
                {
                    existingLength = new FileInfo(localFilePath).Length;
                    downloadedBytes += existingLength;
                    fileDownloaderProgressReport.BytesDownloaded += existingLength;
                }

                using var httpClient = new HttpClient();
                using var request = new HttpRequestMessage(HttpMethod.Get, fileInfo.Url);
                if (existingLength > 0)
                {
                    request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(existingLength, null);
                }

                using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                await using var inputStream = await response.Content.ReadAsStreamAsync();

                await using var outputStream =
                    existingLength > 0 ? File.OpenWrite(localFilePath) : File.Create(localFilePath);
                if (existingLength > 0)
                {
                    outputStream.Seek(0, SeekOrigin.End); // 移动到文件末尾
                }

                var buffer = new byte[8192];
                int bytesRead;
                while ((bytesRead = await inputStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await outputStream.WriteAsync(buffer, 0, bytesRead);
                    downloadedBytes += bytesRead;
                    fileDownloaderProgressReport.BytesDownloaded += bytesRead;
                    onProgressChanged?.Invoke(fileDownloaderProgressReport);
                }

                await outputStream.FlushAsync();
                await outputStream.DisposeAsync();

                var localMd5 = await CalculateMD5Async(localFilePath);
                if (localMd5 == fileInfo.FileMd5)
                {
                    DependencyService.Get<IToast>()
                        .Show($"Chunk: {fileInfo.FileName} verified!");
                    semaphore.Release();
                    return localFilePath;
                }

                DependencyService.Get<IToast>()
                    .Show(
                        $"Md5 unmatch local: {localMd5.Substring(0, 5)} remote: {fileInfo.FileMd5.Substring(0, 5)} at file {fileInfo.FileName}");
            }
            catch (Exception e)
            {
                // Some error
                DependencyService.Get<IToast>().Show($"Encounter exception: {e.Message} at {e.StackTrace}");
            }

            // Re-download part file ...
            if (localFileInfo.Exists)
            {
                localFileInfo.Delete();
            }

            fileDownloaderProgressReport.BytesDownloaded -= downloadedBytes;
            semaphore.Release();

            await Task.Delay(1000 * 5);
            return await DownloadFileAsync(fileInfo, fileDownloaderProgressReport, onProgressChanged);
        }

        public async Task<string> CalculateMD5Async(string filePath)
        {
            await using var stream = File.OpenRead(filePath);
            var md5 = MD5.Create();
            var hash = await md5.ComputeHashAsync(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        private async Task<bool> MergeFilesAsync(string[] filePaths, string outputPath, string md5,
            DownloadStageHandler onStageChanged)
        {
            // merge apks
            onStageChanged?.Invoke(DownloadStage.Merge);
            await using var outputStream = File.Create(outputPath);

            foreach (var filePath in filePaths)
            {
                await using var inputStream = File.OpenRead(filePath);
                await inputStream.CopyToAsync(outputStream);
                File.Delete(filePath);
            }

            await outputStream.FlushAsync();
            outputStream.Close();

            // validate md5
            onStageChanged?.Invoke(DownloadStage.Validate);
            var actualMD5 = await CalculateMD5Async(outputPath);
            if (actualMD5 != md5)
            {
                var outputFile = new FileInfo(outputPath);
                if (outputFile.Exists)
                {
                    outputFile.Delete();
                }

                return false;
            }

            return true;
        }

        public void CleanCacheFiles()
        {
            var folder = new DirectoryInfo(FileSystem.CacheDirectory);
            foreach (var file in folder.GetFiles())
            {
                file.Delete();
            }
        }
    }
}