using System.IO;
using System.Linq;
using System.Text.Json;

namespace IQ99.App;

public sealed record CleanRecord(DateTime When, long BytesFreed, int FilesCount);

public static class CleanHistory
{
    private static readonly string FilePath =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "IQ99", "history.json");

    private const int MaxEntries = 50;

    public static List<CleanRecord> Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                return [];
            }

            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<List<CleanRecord>>(json) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public static void Add(CleanRecord record)
    {
        try
        {
            var records = Load();
            records.Insert(0, record);
            Save(records.Take(MaxEntries).ToList());
        }
        catch
        {
        }
    }

    public static void Clear()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
            }
        }
        catch
        {
        }
    }

    private static void Save(List<CleanRecord> records)
    {
        try
        {
            var dir = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllText(FilePath, JsonSerializer.Serialize(records, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch
        {
        }
    }
}