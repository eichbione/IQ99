using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using IQ99.Core;
using IQ99.Core.Models;
using IQ99.Core.Services;

namespace IQ99.App.Pages;

public partial class CleanerPage : Page
{
    private readonly ObservableCollection<CleanItem> _cleanItems = [];

    public CleanerPage()
    {
        InitializeComponent();
    }

    private async void BtnAnalyze_Click(object sender, RoutedEventArgs e)
    {
        _cleanItems.Clear();
        var items = CleanerService.GetDefaultCategories();
        foreach (var item in items)
        {
            _cleanItems.Add(item);
        }

        CleanListView.ItemsSource = _cleanItems;
        SetCleanButtonsEnabled(false);
        CleanProgress.IsIndeterminate = true;
        UiBus.SetStatus("Analizando carpetas de sistema y navegadores...");
        SetCleanStatus("");

        var total = items.Count;
        var completed = 0;
        try
        {
            await Task.WhenAll(items.Select(item => Task.Run(() =>
            {
                item.Scan();
                var done = Interlocked.Increment(ref completed);
                Dispatcher.Invoke(() =>
                {
                    CleanListView.Items.Refresh();
                    SetCleanStatus($"{done}/{total} categorías analizadas: {item.Name}");
                });
            })));
        }
        finally
        {
            CleanProgress.IsIndeterminate = false;
            SetCleanButtonsEnabled(true);
        }

        CleanListView.Items.Refresh();
        CleanProgress.Value = 100;
        var totalBytes = _cleanItems.Sum(i => i.SizeBytes);
        SetCleanStatus("Análisis completado");
        UiBus.SetStatus($"Análisis completado. Espacio recuperable: {Formatter.FormatBytes(totalBytes)}");
    }

    private async void BtnClean_Click(object sender, RoutedEventArgs e)
    {
        var selected = _cleanItems.Where(i => i.IsSelected).ToList();
        if (selected.Count == 0)
        {
            UiBus.SetStatus("No hay categorías seleccionadas.");
            return;
        }

        var confirm = System.Windows.MessageBox.Show(
            $"Vas a eliminar archivos de {selected.Count} categorías. ¿Quieres continuar?",
            "IQ99",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes)
        {
            return;
        }

        SetCleanButtonsEnabled(false);
        CleanProgress.IsIndeterminate = true;

        var removedTotal = 0;
        var total = selected.Count;
        var done = 0;
        try
        {
            foreach (var item in selected)
            {
                SetCleanStatus($"Limpiando ({done + 1}/{total}): {item.Name}...");
                if (item.Id == "recyclebin")
                {
                    await Task.Run(RecycleBin.EmptyAll);
                }
                else
                {
                    removedTotal += await Task.Run(item.Clean);
                }

                done++;
            }
        }
        finally
        {
            CleanProgress.IsIndeterminate = false;
            SetCleanButtonsEnabled(true);
        }

        CleanListView.Items.Refresh();
        CleanProgress.Value = 100;
        SetCleanStatus("Limpieza completada");
        UiBus.SetStatus($"Limpieza completada. Se eliminaron {removedTotal} archivos/carpetas.");
    }

    private void BtnSelectAll_Click(object sender, RoutedEventArgs e)
    {
        foreach (var item in _cleanItems)
        {
            item.IsSelected = true;
        }

        CleanListView.Items.Refresh();
    }

    private void BtnSelectNone_Click(object sender, RoutedEventArgs e)
    {
        foreach (var item in _cleanItems)
        {
            item.IsSelected = false;
        }

        CleanListView.Items.Refresh();
    }

    private void SetCleanButtonsEnabled(bool enabled)
    {
        BtnAnalyze.IsEnabled = enabled;
        BtnClean.IsEnabled = enabled;
    }

    private void SetCleanStatus(string message)
    {
        TxtCleanStatus.Text = message;
    }
}