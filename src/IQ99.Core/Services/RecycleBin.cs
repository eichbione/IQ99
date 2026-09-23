using System.Runtime.InteropServices;

namespace IQ99.Core.Services;

public static class RecycleBin
{
    private const int SherbNoConfirmation = 0x00000001;
    private const int SherbNoProgressUi = 0x00000002;
    private const int SherbNoSound = 0x00000004;

    public static void EmptyAll()
    {
        _ = SHEmptyRecycleBin(IntPtr.Zero, null, SherbNoConfirmation | SherbNoProgressUi | SherbNoSound);
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern int SHEmptyRecycleBin(IntPtr hwnd, string? pszRootPath, int dwFlags);
}