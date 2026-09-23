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

    public CleanerPage()
    {
        InitializeComponent();
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
        CleanProgress.IsIndeterminate = true;

        var removedTotal = 0;
        var total = selected.Count;
        var done = 0;
        try
        {
            foreach (var item in selected)
            {
                SetCleanStatus($"Limpiando ({done + 1}/{total}): {item.Name}...");
                AppLog.Write($"Limpiando: {item.Id}");
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

            CleanListView.Items.Refresh();
            SetCleanStatus("Limpieza completada");
            UiBus.SetStatus($"Limpieza completada. Se eliminaron {removedTotal} archivos/carpetas.");
            UiBus.ShowSnackbar("Limpieza completada", $"Se eliminaron {removedTotal} archivos o carpetas.");
            AppLog.Write($"Limpieza completada: {removedTotal} elementos");
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