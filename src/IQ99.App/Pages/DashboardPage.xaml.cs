using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using IQ99.Core;
using IQ99.Core.Models;
using IQ99.Core.Services;

namespace IQ99.App.Pages;

public partial class DashboardPage : Page
{
    private readonly ObservableCollection<CleanItem> _items = [];

    public DashboardPage()
    {
        InitializeComponent();
        Loaded += async (_, _) => await RunScanAsync();
    }

    private async Task RunScanAsync()
    {
        _items.Clear();
        var items = CleanerService.GetDefaultCategories();
        foreach (var item in items)
        {
            _items.Add(item);
        }

        CategoryList.ItemsSource = _items;

        HealthRing.IsIndeterminate = true;
        TxtScore.Text = "--";
        TxtHealthStatus.Text = "Analizando el equipo...";
        TxtRecommendation.Text = "Escaneando carpetas del sistema y navegadores...";
        UiBus.SetStatus("Analizando carpetas de sistema y navegadores...");

        var total = items.Count;
        var completed = 0;

        await Task.WhenAll(items.Select(item => Task.Run(() =>
        {
            item.Scan();
            var done = Interlocked.Increment(ref completed);
            Dispatcher.Invoke(() =>
            {
                CategoryList.Items.Refresh();
                UiBus.SetStatus($"Analizando... {done}/{total} categorías");
            });
        })));

        HealthRing.IsIndeterminate = false;
        UpdateDashboard();
    }

    private void UpdateDashboard()
    {
        var reclaimable = _items.Sum(i => i.SizeBytes);
        var withContent = _items.Count(i => i.SizeBytes > 0);
        var score = ComputeScore(reclaimable);

        HealthRing.Progress = score;
        TxtScore.Text = score.ToString();
        TxtWithContent.Text = withContent.ToString();
        TxtCategories.Text = _items.Count.ToString();
        TxtReclaimable.Text = reclaimable > 0
            ? $"Espacio recuperable: {Formatter.FormatBytes(reclaimable)}"
            : "No se encontró nada que limpiar. ¡Buen trabajo!";

        var (label, recommendation) = BuildSummary(reclaimable, _items);
        TxtHealthStatus.Text = label;
        TxtRecommendation.Text = recommendation;
        CategoryList.Items.Refresh();
        UiBus.SetStatus("Análisis completado");
    }

    private static int ComputeScore(long reclaimable)
    {
        if (reclaimable < 50 * 1024 * 1024) return 98;
        if (reclaimable < 250 * 1024 * 1024) return 92;
        if (reclaimable < 1024 * 1024 * 1024) return 84;
        if (reclaimable < 3L * 1024 * 1024 * 1024) return 72;
        if (reclaimable < 6L * 1024 * 1024 * 1024) return 55;
        return 35;
    }

    private static (string Label, string Recommendation) BuildSummary(long reclaimable, IReadOnlyList<CleanItem> items)
    {
        if (reclaimable < 250 * 1024 * 1024)
        {
            return ("Estado excelente", "Tu equipo está ordenado. Vuelve a pasar el limpiador en unos días.");
        }

        var suggestions = new List<string>();
        foreach (var item in items.OrderByDescending(i => i.SizeBytes).Take(3))
        {
            if (item.SizeBytes > 0)
            {
                suggestions.Add($"{item.Name} ({Formatter.FormatBytes(item.SizeBytes)})");
            }
        }

        var advice = suggestions.Count > 0
            ? $"Lo que más espacio ocupa: {string.Join(", ", suggestions)}."
            : "Hay trabajo pendiente. Ejecuta una limpieza para liberar espacio.";

        return (reclaimable > 5L * 1024 * 1024 * 1024 ? "Estado regular" : "Puedes mejorar", advice);
    }

    private void BtnCleanNow_Click(object sender, RoutedEventArgs e)
    {
        AppLog.Write("Click en Limpiar ahora (Inicio)");
        UiBus.RequestNavigate(typeof(CleanerPage));
    }

    private void BtnAnalyzeNow_Click(object sender, RoutedEventArgs e)
    {
        AppLog.Write("Click en Analizar (Inicio)");
        _ = RunScanAsync();
    }
}