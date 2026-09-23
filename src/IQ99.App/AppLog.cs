using System.IO;
using System.Linq;

namespace IQ99.App;

public static class AppLog
{
    private static readonly object Lock = new();

    public static void Write(string message)
    {
        try
        {
            lock (Lock)
            {
                File.AppendAllText(
                    Path.Combine(Path.GetTempPath(), "iq99.log"),
                    $"[{DateTime.Now:HH:mm:ss.fff}] {message}{Environment.NewLine}");
            }
        }
        catch
        {
        }
    }

    public static void Write(string message, Exception exception)
    {
        Write($"{message} -> {exception}");
    }

    public static void Clear()
    {
        try
        {
            lock (Lock)
            {
                File.WriteAllText(Path.Combine(Path.GetTempPath(), "iq99.log"), string.Empty);
            }
        }
        catch
        {
        }
    }
}