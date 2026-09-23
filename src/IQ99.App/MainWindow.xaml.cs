using System.Windows;
using IQ99.App.Pages;
using Wpf.Ui.Controls;

namespace IQ99.App;

public partial class MainWindow : FluentWindow
{
    public MainWindow()
    {
        InitializeComponent();
        UiBus.StatusChanged += message => Dispatcher.Invoke(() => TxtStatus.Text = message);
        UiBus.SetStatus("Listo.");
        Loaded += (_, _) => NavView.Navigate(typeof(CleanerPage));
    }
}