﻿using Android.App;
using Android.Runtime;
using ApkDownload.Platforms.Android.Impl;
using ApkDownload.Shared.Interface;

namespace ApkDownload;

[Application]
public class MainApplication : MauiApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
        DependencyService.Register<AndroidToast>();
        DependencyService.Register<ApkInstaller>();
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}