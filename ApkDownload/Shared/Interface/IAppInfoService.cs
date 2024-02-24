namespace ApkDownload.Shared.Interface;

public interface IAppInfoService
{
    bool IsAppInstalled(string packageName);
    string GetAppVersion(string packageName);
    void LaunchApp(string packageName);
}