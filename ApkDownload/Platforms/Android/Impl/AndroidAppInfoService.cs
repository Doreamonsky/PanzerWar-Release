using Android.Content;
using Android.Content.PM;
using ApkDownload.Shared.Interface;
using Application = Android.App.Application;

namespace ApkDownload.Platforms.Android.Impl;

public class AndroidAppInfoService : IAppInfoService
{
    public bool IsAppInstalled(string packageName)
    {
        PackageManager packageManager = Application.Context.PackageManager;
        bool installed;
        try
        {
            packageManager.GetPackageInfo(packageName, PackageInfoFlags.Activities);
            installed = true;
        }
        catch (PackageManager.NameNotFoundException)
        {
            installed = false;
        }

        return installed;
    }

    public string GetAppVersion(string packageName)
    {
        PackageManager packageManager = Application.Context.PackageManager;
        string version = "";
        try
        {
            PackageInfo packageInfo = packageManager.GetPackageInfo(packageName, 0);
            version = packageInfo.VersionName;
        }
        catch (PackageManager.NameNotFoundException)
        {
            // Handle error: package not found
        }

        return version;
    }

    public void LaunchApp(string packageName)
    {
        Context context = Application.Context;
        Intent launchIntent = context.PackageManager.GetLaunchIntentForPackage(packageName);
        if (launchIntent != null)
        {
            context.StartActivity(launchIntent);
        }
    }
}