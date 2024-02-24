using ApkDownload.Platforms.Android.Impl;
using ApkDownload.Shared.Interface;
using Microsoft.Extensions.Logging;

namespace ApkDownload;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });


#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}