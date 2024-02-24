using Android.Content;
using ApkDownload.Shared.Interface;
using Application = Android.App.Application;

namespace ApkDownload.Platforms.Android.Impl;

public class ApkInstaller : IApkInstaller
{
    public void InstallApk(string apkFilePath)
    {
        var context = Application.Context;
        var intent = new Intent(Intent.ActionView);
        var file = new Java.IO.File(apkFilePath);
        var apkURI = FileProvider.GetUriForFile(context, $"{context.PackageName}.provider", file);

        intent.SetDataAndType(apkURI, "application/vnd.android.package-archive");
        intent.AddFlags(ActivityFlags.NewTask);
        intent.AddFlags(ActivityFlags.GrantReadUriPermission);

        context.StartActivity(intent);
    }
}