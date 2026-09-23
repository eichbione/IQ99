using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using IQ99.App.Pages;
using Wpf.Ui.Controls;

namespace IQ99.App;

public partial class MainWindow : FluentWindow
{
    private bool _toastVisible;
    private CancellationTokenSource? _toastCts;

    public MainWindow()
    {
        InitializeComponent();
        UiBus.StatusChanged += message =>
        {
            Dispatcher.Invoke(() => TxtStatus.Text = message);
        };
        UiBus.SnackbarChanged += (title, message) => Dispatcher.Invoke(() => ShowToast(title, message));
        UiBus.NavigateRequested += pageType => Dispatcher.Invoke(() =>
        {
            AppLog.Write($"Navegando a {pageType.Name}");
            NavView.Navigate(pageType);
        });
        UiBus.SetStatus("Listo.");
        Loaded += (_, _) => NavView.Navigate(typeof(DashboardPage));
    }

    private void ShowToast(string title, string message, bool showCheck = true)
    {
        ToastTitle.Text = title;
        ToastMessage.Text = message;
        ToastIcon.Symbol = showCheck ? SymbolRegular.CheckmarkCircle24 : SymbolRegular.Info24;

        _toastCts?.Cancel();
        _toastCts = new CancellationTokenSource();

        Toast.Visibility = Visibility.Visible;
        _toastVisible = true;
        var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(160));
        Toast.BeginAnimation(OpacityProperty, fadeIn);
        Toast.RenderTransform = new TranslateTransform();
        Toast.RenderTransform.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(-14, 0, TimeSpan.FromMilliseconds(200)));

        _ = HideToastAfterAsync(_toastCts.Token);
    }

    private async Task HideToastAfterAsync(CancellationToken token)
    {
        try
        {
            await Task.Delay(3500, token);
            Dispatcher.Invoke(HideToast);
        }
        catch (TaskCanceledException)
        {
        }
    }

    private void HideToast()
    {
        if (!_toastVisible)
        {
            return;
        }

        _toastVisible = false;
        var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(150));
        fadeOut.Completed += (_, _) => Toast.Visibility = Visibility.Collapsed;
        Toast.BeginAnimation(OpacityProperty, fadeOut);
    }
}