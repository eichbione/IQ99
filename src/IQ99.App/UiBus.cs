namespace IQ99.App;

public static class UiBus
{
    public static event Action<string>? StatusChanged;

    public static void SetStatus(string message)
    {
        StatusChanged?.Invoke(message);
    }
}