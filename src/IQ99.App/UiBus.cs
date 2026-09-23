namespace IQ99.App;

public static class UiBus
{
    public static event Action<string>? StatusChanged;
    public static event Action<string, string>? SnackbarChanged;
    public static event Action<Type>? NavigateRequested;

    public static void SetStatus(string message)
    {
        StatusChanged?.Invoke(message);
    }

    public static void ShowSnackbar(string title, string message)
    {
        SnackbarChanged?.Invoke(title, message);
    }

    public static void RequestNavigate(Type pageType)
    {
        NavigateRequested?.Invoke(pageType);
    }
}