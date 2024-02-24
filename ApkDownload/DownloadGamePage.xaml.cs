using ApkDownload.Resources.Localization;
using ApkDownload.Shared.Downloader;
using ApkDownload.Shared.Interface;

namespace ApkDownload;

public partial class DownloadGamePage : ContentPage
{
    private static bool _isDownloading = false;
    private static readonly FileDownloader _downloader = new FileDownloader();
    private static readonly Dictionary<int, ApkFileDetails> _apkFileDetailMap = new Dictionary<int, ApkFileDetails>();

    public DownloadGamePage()
    {
        InitializeComponent();

        downloadTypePicker.Items.Clear();
        downloadTypePicker.Items.Add(AppRes.FreeVer);
        downloadTypePicker.Items.Add(AppRes.DEVer);
        downloadTypePicker.SelectedIndex = 0;
    }


    private string GetDownloadLink()
    {
        switch (downloadTypePicker.SelectedIndex)
        {
            case 1:
                return "https://dl.windyverse.net/apk-de/apk_ship.json";
            default:
                return "https://dl.windyverse.net/apk/apk_ship.json";
        }
    }

    private string GetLocalFileCacheName()
    {
        switch (downloadTypePicker.SelectedIndex)
        {
            case 1:
                return "de-base.apk";
            default:
                return "base.apk";
        }
    }

    private async void OnDownloadFileClicked(object sender, EventArgs e)
    {
        if (_isDownloading)
        {
            return;
        }

        SetDownloadState(true);

        var apkFile = $"{FileSystem.CacheDirectory}/{GetLocalFileCacheName()}";
        var index = downloadTypePicker.SelectedIndex;
        if (!_apkFileDetailMap.TryGetValue(index, out var _apkFileDetail))
        {
            _apkFileDetail = await _downloader.FetchFileListAsync(GetDownloadLink());
            _apkFileDetailMap[index] = _apkFileDetail;
        }

        var isUserAgree =
            await DisplayAlert(AppRes.DownloadGame, string.Format(AppRes.DownloadConfirm, _apkFileDetail.FileName),
                AppRes.Yes,
                AppRes.No);

        if (isUserAgree)
        {
            await RunDownloadAndInstall(_apkFileDetail, apkFile);
        }

        SetDownloadState(false);
    }

    private void SetDownloadState(bool isState)
    {
        _isDownloading = isState;
        CleanCacheBtn.IsVisible = !isState;
        DownloadProgress.IsVisible = isState;
        DownloadInfo.IsVisible = isState;
    }

    private async Task RunDownloadAndInstall(ApkFileDetails apkFileDetail, string apkFile)
    {
        DependencyService.Get<IToast>().Show($"Local cache file: {apkFile}");

        var isDownloaded = await _downloader.DownloadAndMergeFilesAsync(apkFileDetail, apkFile,
            report =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    var bytesDownloaded = $"{report.BytesDownloaded / 1024 / 1024:0.00} MB";
                    var totalBytes = $"{report.TotalBytes / 1024 / 1024:0.00} MB";
                    DownloadProgress.Progress = report.ProgressPercentage;
                    var percent = (int)Math.Round(report.ProgressPercentage * 100);
                    DownloadInfo.Text = $"{bytesDownloaded} | {totalBytes} | {percent}%";
                });
            }, stage =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    switch (stage)
                    {
                        case FileDownloader.DownloadStage.Downloading:
                            DownloadBtn.Text = AppRes.Downloading;
                            break;
                        case FileDownloader.DownloadStage.Merge:
                            DownloadBtn.Text = AppRes.Merge;
                            break;
                        case FileDownloader.DownloadStage.Validate:
                            DownloadBtn.Text = AppRes.Validate;
                            break;
                    }
                });
            });

        if (!isDownloaded)
        {
            await DisplayAlert(AppRes.Error, AppRes.FileDamaged, AppRes.Yes);
            DownloadBtn.Text = AppRes.DownloadGame;
            return;
        }

        DownloadBtn.Text = AppRes.Downloaded;

        var installer = DependencyService.Get<IApkInstaller>();
        if (installer != null)
        {
            installer.InstallApk(apkFile);
        }
    }

    private async void OnCleanCacheClicked(object sender, EventArgs e)
    {
        var isCleanCache =
            await DisplayAlert(AppRes.Confrim, AppRes.CleanCacheConfirm,
                AppRes.Yes,
                AppRes.No);

        if (isCleanCache)
        {
            _downloader.CleanCacheFiles();
        }
    }
}