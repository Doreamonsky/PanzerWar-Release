using ApkDownload.Resources.Localization;

namespace ApkDownload;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();

        AppDomain.CurrentDomain.UnhandledException += HandleException;
        TaskScheduler.UnobservedTaskException += HandleUnobservedException;
    }

    private void HandleException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception exception)
        {
            MainPage.Dispatcher.Dispatch(() =>
            {
                MainPage.DisplayAlert(AppRes.Exception,
                    string.Format(AppRes.ExceptionContent, exception.Message, exception.StackTrace), AppRes.Yes);
            });
        }
    }

    private void HandleUnobservedException(object sender, UnobservedTaskExceptionEventArgs e)
    {
        e.SetObserved();

        MainPage.Dispatcher.Dispatch(() =>
        {
            MainPage.DisplayAlert(AppRes.Exception,
                string.Format(AppRes.ExceptionContent, e.Exception.Message, e.Exception.StackTrace), AppRes.Yes);
        });
    }
}