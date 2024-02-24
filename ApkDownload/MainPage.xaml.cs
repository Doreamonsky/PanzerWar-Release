using ApkDownload.Resources.Localization;
using ApkDownload.Shared.Downloader;
using ApkDownload.Shared.Interface;

namespace ApkDownload;

public partial class MainPage : ContentPage
{
    private bool _isDownloading = false;
    private readonly FileDownloader _downloader;
    private ApkFileDetails _apkFileDetail;

    public MainPage()
    {
        InitializeComponent();
        _downloader = new FileDownloader();

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
        if (_apkFileDetail == null)
        {
            _apkFileDetail = await _downloader.FetchFileListAsync(GetDownloadLink());
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
    }

    private async Task RunDownloadAndInstall(ApkFileDetails apkFileDetail, string apkFile)
    {
        DownloadBtn.Text = AppRes.Downloading;
        DownloadProgress.IsVisible = true;
        DownloadInfo.IsVisible = true;

        DependencyService.Get<IToast>().Show($"Local cache file: {apkFile}");

        var isDownloaded = await _downloader.DownloadAndMergeFilesAsync(apkFileDetail, apkFile,
            (report =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    var bytesDownloaded = $"{report.BytesDownloaded / 1024 / 1024:0.00} MB";
                    var totalBytes = $"{report.TotalBytes / 1024 / 1024:0.00} MB";
                    DownloadProgress.Progress = report.ProgressPercentage;
                    var percent = (int)Math.Round(report.ProgressPercentage * 100);
                    DownloadInfo.Text = $"{bytesDownloaded} | {totalBytes} | {percent}%";
                });
            }));

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