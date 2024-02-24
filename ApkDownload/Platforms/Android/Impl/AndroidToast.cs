using Android.Widget;
using ApkDownload.Shared.Interface;

namespace ApkDownload.Platforms.Android.Impl;

public class AndroidToast : IToast
{
    public void Show(string message)
    {
        var context = Application.Current?.Handler?.MauiContext?.Context;
        if (context != null)
        {
            Toast.MakeText(context, message, ToastLength.Short)?.Show();
        }
    }
}