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
    }

    private async void OnDownloadFileClicked(object sender, EventArgs e)
    {
        if (_isDownloading)
        {
            return;
        }

        _isDownloading = true;

        var apkFile = $"{FileSystem.CacheDirectory}/base.apk";
        if (_apkFileDetail == null)
        {
            _apkFileDetail = await _downloader.FetchFileListAsync("https://dl.windyverse.net/apk/apk_ship.json");
        }

        var isUserAgree =
            await DisplayAlert(AppRes.DownloadGame, string.Format(AppRes.DownloadConfirm, _apkFileDetail.FileName),
                AppRes.Yes,
                AppRes.No);

        if (isUserAgree)
        {
            await RunDownloadAndInstall(_apkFileDetail, apkFile);
        }

        _isDownloading = false;
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
                    var percent = Math.Round(report.ProgressPercentage * 100);
                    DownloadProgress.Progress = report.ProgressPercentage;
                    DownloadInfo.Text = $"{bytesDownloaded} | {totalBytes} | {percent}%";
                });
            }));

        if (!isDownloaded)
        {
            await DisplayAlert(AppRes.Error, AppRes.FileDamaged, AppRes.Yes);
            return;
        }

        DownloadBtn.Text = AppRes.Downloaded;

        var installer = DependencyService.Get<IApkInstaller>();
        if (installer != null)
        {
            installer.InstallApk(apkFile);
        }
    }
}