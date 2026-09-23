using System.Threading.Tasks;
using System.Windows;

namespace IQ99.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        AppLog.Clear();
        AppLog.Write("IQ99 arrancando");

        DispatcherUnhandledException += (_, args) =>
        {
            AppLog.Write("EXCEPCIÓN NO CONTROLADA", args.Exception);
            try
            {
                MessageBox.Show(
                    $"Ocurrió un error inesperado:\n\n{args.Exception.Message}\n\nSe guardó el detalle en el registro.",
                    "IQ99 - Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch
            {
            }
        };

        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            AppLog.Write("TAREA CON ERROR NO VISTO", args.Exception);
            args.SetObserved();
        };
    }
}