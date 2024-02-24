using ApkDownload.Resources.Localization;
using ApkDownload.Shared;
using ApkDownload.Shared.Downloader;
using ApkDownload.Shared.Interface;

namespace ApkDownload;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        CheckInstalledApp();
    }

    private void CheckInstalledApp()
    {
        var freeVerInstalled = DependencyService.Get<IAppInfoService>()
            .IsAppInstalled(AppConst.FREE_VER_PACKAGE_NAME);
        if (freeVerInstalled)
        {
            PlayFreeVerBtn.Text = string.Format(AppRes.Launch, AppRes.FreeVer, DependencyService.Get<IAppInfoService>()
                .GetAppVersion(AppConst.FREE_VER_PACKAGE_NAME));
        }

        PlayFreeVerBtn.IsVisible = freeVerInstalled;

        var paidVerInstalled = DependencyService.Get<IAppInfoService>()
            .IsAppInstalled(AppConst.DE_VER_PACKAGE_NAME);
        if (paidVerInstalled)
        {
            PlayDEVerBtn.Text = string.Format(AppRes.Launch, AppRes.DEVer, DependencyService.Get<IAppInfoService>()
                .GetAppVersion(AppConst.DE_VER_PACKAGE_NAME));
        }

        PlayDEVerBtn.IsVisible = paidVerInstalled;
    }

    private async void OnDownloadGameClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//DownloadGame");
    }

    private void OnPlayFreeVerClicked(object sender, EventArgs e)
    {
        DependencyService.Get<IAppInfoService>().LaunchApp(AppConst.FREE_VER_PACKAGE_NAME);
    }

    private void OnPlayPaidVerClicked(object sender, EventArgs e)
    {
        DependencyService.Get<IAppInfoService>().LaunchApp(AppConst.DE_VER_PACKAGE_NAME);
    }
}