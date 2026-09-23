using IQ99.Core.Models;

namespace IQ99.Core.Services;

public static class CleanerService
{
    public static IReadOnlyList<CleanItem> GetDefaultCategories()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var systemRoot = Environment.GetFolderPath(Environment.SpecialFolder.Windows);

        return new List<CleanItem>
        {
            new()
            {
                Id = "temp",
                Name = "Archivos temporales",
                Description = "Carpetas TEMP del usuario y del sistema",
                Paths = [Path.GetTempPath(), Path.Combine(systemRoot, "Temp")],
                IconKey = "Clock24",
                TintHex = "#5E9EFF",
            },
            new()
            {
                Id = "prefetch",
                Name = "Prefetch",
                Description = "Caché de precarga de aplicaciones",
                Paths = [Path.Combine(systemRoot, "Prefetch")],
                RequiresAdmin = true,
                IconKey = "Play24",
                TintHex = "#B36BFF",
            },
            new()
            {
                Id = "winupdate",
                Name = "Caché de Windows Update",
                Description = "Descargas pendientes de actualizaciones",
                Paths = [Path.Combine(systemRoot, "SoftwareDistribution", "Download")],
                RequiresAdmin = true,
                IconKey = "ArrowDownload24",
                TintHex = "#FFC24D",
                CloseNotice = "Reinicia el equipo para terminar de aplicar las actualizaciones.",
            },
            new()
            {
                Id = "thumbnails",
                Name = "Miniaturas del Explorador",
                Description = "Caché de miniaturas de archivos e imágenes",
                Paths = [Path.Combine(localAppData, "Microsoft", "Windows", "Explorer")],
                IconKey = "Image24",
                TintHex = "#6BCB77",
            },
            new()
            {
                Id = "chrome",
                Name = "Caché de Chrome",
                Description = "Caché de navegación de Google Chrome",
                Paths = [Path.Combine(localAppData, "Google", "Chrome", "User Data", "Default", "Cache")],
                IconKey = "Globe24",
                TintHex = "#EA5940",
                CloseNotice = "Cierra Chrome para liberarlo por completo.",
            },
            new()
            {
                Id = "edge",
                Name = "Caché de Edge",
                Description = "Caché de navegación de Microsoft Edge",
                Paths = [Path.Combine(localAppData, "Microsoft", "Edge", "User Data", "Default", "Cache")],
                IconKey = "Globe24",
                TintHex = "#35C1C1",
                CloseNotice = "Cierra Edge para liberarlo por completo.",
            },
            new()
            {
                Id = "firefox",
                Name = "Caché de Firefox",
                Description = "Caché de navegación de Mozilla Firefox",
                Paths = GetFirefoxCachePaths(localAppData),
                IconKey = "Globe24",
                TintHex = "#FF9143",
                CloseNotice = "Cierra Firefox para liberarlo por completo.",
            },
            new()
            {
                Id = "recyclebin",
                Name = "Papelera de reciclaje",
                Description = "Elimina permanentemente el contenido de la papelera",
                Paths = [],
                IconKey = "Delete24",
                TintHex = "#9FA3A8",
            },
        };
    }

    public static void Scan(IEnumerable<CleanItem> items)
    {
        foreach (var item in items)
        {
            item.Scan();
        }
    }

    private static string[] GetFirefoxCachePaths(string localAppData)
    {
        var profiles = Path.Combine(localAppData, "Mozilla", "Firefox", "Profiles");
        if (!Directory.Exists(profiles))
        {
            return [];
        }

        return Directory.EnumerateDirectories(profiles)
            .Select(profile => Path.Combine(profile, "cache2"))
            .Where(Directory.Exists)
            .ToArray();
    }
}