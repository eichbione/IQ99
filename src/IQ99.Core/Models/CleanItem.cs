namespace IQ99.Core.Models;

public sealed class CleanItem
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required string[] Paths { get; init; }
    public bool IsSelected { get; set; } = true;
    public bool RequiresAdmin { get; init; }
    public long SizeBytes { get; private set; }
    public string SizeText => Formatter.FormatBytes(SizeBytes);

    public void Scan()
    {
        SizeBytes = 0;
        foreach (var path in Paths)
        {
            if (Directory.Exists(path))
            {
                SizeBytes += DirSize(path);
            }
        }
    }

    public int Clean()
    {
        var removed = 0;
        foreach (var path in Paths)
        {
            if (!Directory.Exists(path))
            {
                continue;
            }

            foreach (var entry in Directory.EnumerateFileSystemEntries(path))
            {
                try
                {
                    if (Directory.Exists(entry))
                    {
                        Directory.Delete(entry, recursive: true);
                    }
                    else
                    {
                        File.Delete(entry);
                    }

                    removed++;
                }
                catch
                {
                }
            }
        }

        SizeBytes = 0;
        return removed;
    }

    private static long DirSize(string dir)
    {
        long size = 0;
        try
        {
            foreach (var entry in Directory.EnumerateFileSystemEntries(dir))
            {
                try
                {
                    if (Directory.Exists(entry))
                    {
                        size += DirSize(entry);
                    }
                    else
                    {
                        size += new FileInfo(entry).Length;
                    }
                }
                catch
                {
                }
            }
        }
        catch
        {
        }

        return size;
    }
}