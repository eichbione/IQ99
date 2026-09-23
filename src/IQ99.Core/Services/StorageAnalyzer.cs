using IQ99.Core.Models;

namespace IQ99.Core.Services;

public static class StorageAnalyzer
{
    public static List<FolderSize> Analyze(string root, int maxDepth = 3)
    {
        var nodes = new List<FolderSize>();
        Walk(root, 1, maxDepth, nodes);

        nodes.Sort((a, b) => b.SizeBytes.CompareTo(a.SizeBytes));

        var max = nodes.Count > 0 ? nodes[0].SizeBytes : 0;
        foreach (var node in nodes)
        {
            node.Percent = max > 0 ? node.SizeBytes * 100.0 / max : 0;
        }

        return nodes;
    }

    private static void Walk(string dir, int depth, int maxDepth, List<FolderSize> nodes)
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
                        if (depth < maxDepth)
                        {
                            Walk(entry, depth + 1, maxDepth, nodes);
                        }

                        size += DirSize(entry);
                    }
                    else
                    {
                        var info = new FileInfo(entry);
                        if (info.Length > 0)
                        {
                            size += info.Length;
                        }
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

        if (size > 1024)
        {
            nodes.Add(new FolderSize { Path = dir, SizeBytes = size });
        }
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
                        var info = new FileInfo(entry);
                        if (info.Length > 0)
                        {
                            size += info.Length;
                        }
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