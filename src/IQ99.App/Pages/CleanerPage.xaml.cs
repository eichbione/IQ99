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
    private bool _scanned;
    private bool _isBusy;
    private List<CleanRecord> _history = [];

    public CleanerPage()
    {
        InitializeComponent();
        RefreshHistory();
        Loaded += async (_, _) =>
        {
            if (!_scanned)
            {
                await RunAnalyzeAsync();
            }
        };
    }

    private async Task RunAnalyzeAsync()
    {
        if (_isBusy)
        {
            return;
        }

        _isBusy = true;
        AppLog.Write("Análisis iniciado");
        SetCleanButtonsEnabled(false);
        CleanProgress.IsIndeterminate = true;

        _cleanItems.Clear();
        var items = CleanerService.GetDefaultCategories();
        foreach (var item in items)
        {
            _cleanItems.Add(item);
        }

        CleanListView.ItemsSource = _cleanItems;
        SetCleanStatus("Analizando carpetas de sistema y navegadores...");
        UiBus.SetStatus("Analizando carpetas de sistema y navegadores...");

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
                    SetCleanStatus($"Analizado {done}/{total}: {item.Name}");
                });
            })));

            CleanListView.Items.Refresh();
            var reclaimable = _cleanItems.Sum(i => i.SizeBytes);
            SetCleanStatus("Análisis completado");
            UiBus.SetStatus($"Análisis completado. Espacio recuperable: {Formatter.FormatBytes(reclaimable)}");
            UiBus.ShowSnackbar("Análisis completado", $"Se encontró {Formatter.FormatBytes(reclaimable)} de espacio recuperable.");
            AppLog.Write($"Análisis completado: {reclaimable} bytes");
        }
        catch (Exception ex)
        {
            AppLog.Write("Error al analizar", ex);
            SetCleanStatus("No se pudo completar el análisis.");
        }
        finally
        {
            _scanned = true;
            _isBusy = false;
            CleanProgress.IsIndeterminate = false;
            CleanProgress.Value = 100;
            SetCleanButtonsEnabled(true);
        }
    }

    private async void BtnAnalyze_Click(object sender, RoutedEventArgs e)
    {
        AppLog.Write("Click en Analizar");
        await RunAnalyzeAsync();
    }

    private async void BtnClean_Click(object sender, RoutedEventArgs e)
    {
        AppLog.Write("Click en Limpiar");
        var selected = _cleanItems.Where(i => i.IsSelected).ToList();
        if (selected.Count == 0)
        {
            UiBus.ShowSnackbar("Nada que limpiar", "Marca al menos una categoría en la lista.");
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

        _isBusy = true;
        SetCleanButtonsEnabled(false);
        CleanProgress.Minimum = 0;
        CleanProgress.Maximum = selected.Count;
        CleanProgress.Value = 0;

        var freedBytes = 0L;
        var removedTotal = 0;
        var done = 0;
        try
        {
            foreach (var item in selected)
            {
                SetCleanStatus($"Limpiando ({done + 1}/{selected.Count}): {item.Name}...");
                AppLog.Write($"Limpiando: {item.Id}");
                freedBytes += item.SizeBytes;
                if (item.Id == "recyclebin")
                {
                    await Task.Run(RecycleBin.EmptyAll);
                }
                else
                {
                    removedTotal += await Task.Run(item.Clean);
                }

                done++;
                CleanProgress.Value = done;
            }

            CleanListView.Items.Refresh();
            SetCleanStatus("Limpieza completada");
            UiBus.SetStatus($"Limpieza completada. Se liberaron {Formatter.FormatBytes(freedBytes)}.");
            AppLog.Write($"Limpieza completada: {freedBytes} bytes, {removedTotal} elementos");

            CleanHistory.Add(new CleanRecord(DateTime.Now, freedBytes, removedTotal));
            RefreshHistory();
            await ShowCleanDoneAsync(freedBytes, removedTotal);
        }
        catch (Exception ex)
        {
            AppLog.Write("Error al limpiar", ex);
            SetCleanStatus("Ocurrió un error durante la limpieza.");
        }
        finally
        {
            CleanProgress.IsIndeterminate = false;
            CleanProgress.Value = 100;
            SetCleanButtonsEnabled(true);
            _isBusy = false;
        }
    }

    private async Task ShowCleanDoneAsync(long freedBytes, int removedTotal)
    {
        TxtDoneCount.Text = $"{removedTotal} archivos o carpetas eliminados";

        CleanDoneOverlay.Visibility = Visibility.Visible;
        TxtDoneBytes.Text = Formatter.FormatBytes(0);

        var steps = 45;
        for (var i = 1; i <= steps; i++)
        {
            var current = (long)(freedBytes * i / (double)steps);
            TxtDoneBytes.Text = Formatter.FormatBytes(current);
            await Task.Delay(18);
        }

        TxtDoneBytes.Text = Formatter.FormatBytes(freedBytes);
        UiBus.ShowSnackbar("Limpieza completada", $"Se liberaron {Formatter.FormatBytes(freedBytes)}.");
    }

    private void BtnDoneClose_Click(object sender, RoutedEventArgs e)
    {
        CleanDoneOverlay.Visibility = Visibility.Collapsed;
    }

    private void CleanDoneOverlay_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        CleanDoneOverlay.Visibility = Visibility.Collapsed;
    }

    private void BtnClearHistory_Click(object sender, RoutedEventArgs e)
    {
        CleanHistory.Clear();
        RefreshHistory();
        UiBus.ShowSnackbar("Historial borrado", "El registro de limpiezas quedó vacío.");
    }

    private void RefreshHistory()
    {
        _history = CleanHistory.Load();
        HistoryList.ItemsSource = _history;

        var hasHistory = _history.Count > 0;
        HistoryList.Visibility = hasHistory ? Visibility.Visible : Visibility.Collapsed;
        TxtHistoryEmpty.Visibility = hasHistory ? Visibility.Collapsed : Visibility.Visible;
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