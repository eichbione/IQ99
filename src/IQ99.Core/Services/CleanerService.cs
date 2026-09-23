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
            },
            new()
            {
                Id = "prefetch",
                Name = "Prefetch",
                Description = "CachÃ© de precarga de aplicaciones",
                Paths = [Path.Combine(systemRoot, "Prefetch")],
                RequiresAdmin = true,
            },
            new()
            {
                Id = "winupdate",
                Name = "CachÃ© de Windows Update",
                Description = "Descargas pendientes de actualizaciones",
                Paths = [Path.Combine(systemRoot, "SoftwareDistribution", "Download")],
                RequiresAdmin = true,
            },
            new()
            {
                Id = "thumbnails",
                Name = "Miniaturas del Explorador",
                Description = "CachÃ© de miniaturas de archivos e imÃ¡genes",
                Paths = [Path.Combine(localAppData, "Microsoft", "Windows", "Explorer")],
            },
            new()
            {
                Id = "chrome",
                Name = "CachÃ© de Chrome",
                Description = "CachÃ© de navegaciÃ³n de Google Chrome",
                Paths = [Path.Combine(localAppData, "Google", "Chrome", "User Data", "Default", "Cache")],
            },
            new()
            {
                Id = "edge",
                Name = "CachÃ© de Edge",
                Description = "CachÃ© de navegaciÃ³n de Microsoft Edge",
                Paths = [Path.Combine(localAppData, "Microsoft", "Edge", "User Data", "Default", "Cache")],
            },
            new()
            {
                Id = "firefox",
                Name = "CachÃ© de Firefox",
                Description = "CachÃ© de navegaciÃ³n de Mozilla Firefox",
                Paths = GetFirefoxCachePaths(localAppData),
            },
            new()
            {
                Id = "recyclebin",
                Name = "Papelera de reciclaje",
                Description = "Elimina permanentemente el contenido de la papelera",
                Paths = [],
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