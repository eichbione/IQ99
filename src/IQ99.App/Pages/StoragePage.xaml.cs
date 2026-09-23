using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using IQ99.Core;
using IQ99.Core.Services;

namespace IQ99.App.Pages;

public partial class StoragePage : Page
{
    public StoragePage()
    {
        InitializeComponent();
        LoadDrives();
    }

    private void LoadDrives()
    {
        foreach (var drive in DriveInfo.GetDrives())
        {
            if (!drive.IsReady || (drive.DriveType != DriveType.Fixed && drive.DriveType != DriveType.Removable))
            {
                continue;
            }

            var label = string.IsNullOrWhiteSpace(drive.VolumeLabel) ? drive.Name : $"{drive.VolumeLabel} ({drive.Name})";
            var info = $"{label}  ·  {Formatter.FormatBytes(drive.TotalSize - drive.AvailableFreeSpace)} / {Formatter.FormatBytes(drive.TotalSize)} usados";
            DriveCombo.Items.Add(new ComboBoxItem { Content = info, Tag = drive.Name });
        }

        if (DriveCombo.Items.Count > 0)
        {
            DriveCombo.SelectedIndex = 0;
        }
    }

    private async void BtnScan_Click(object sender, RoutedEventArgs e)
    {
        var combo = DriveCombo.SelectedItem as ComboBoxItem;
        var root = combo?.Tag as string;
        if (string.IsNullOrEmpty(root))
        {
            return;
        }

        SetFolderButtonsEnabled(false);
        FolderProgress.IsIndeterminate = true;
        UiBus.SetStatus($"Analizando {root}... esto puede tardar unos minutos.");

        var results = await Task.Run(() => StorageAnalyzer.Analyze(root));

        FolderListView.ItemsSource = results.Take(80).ToList();
        FolderProgress.IsIndeterminate = false;
        SetFolderButtonsEnabled(true);

        var total = results.Sum(r => r.SizeBytes);
        UiBus.SetStatus($"Análisis completado: {Formatter.FormatBytes(total)} en carpetas principales.");
        UiBus.ShowSnackbar("Análisis de disco", $"{Formatter.FormatBytes(total)} en las carpetas principales de {root}.");
    }

    private void BtnEmptyBin_Click(object sender, RoutedEventArgs e)
    {
        var confirm = System.Windows.MessageBox.Show(
            "¿Vaciar la papelera de reciclaje? Esta acción no se puede deshacer.",
            "IQ99",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm == MessageBoxResult.Yes)
        {
            RecycleBin.EmptyAll();
            UiBus.SetStatus("Papelera de reciclaje vaciada.");
            UiBus.ShowSnackbar("Papelera vaciada", "Se liberó el espacio de la papelera de reciclaje.");
        }
    }

    private void SetFolderButtonsEnabled(bool enabled)
    {
        BtnScan.IsEnabled = enabled;
        BtnEmptyBin.IsEnabled = enabled;
        DriveCombo.IsEnabled = enabled;
    }
}