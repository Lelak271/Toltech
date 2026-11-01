using System.Diagnostics;
using Microsoft.Win32;

public static class SystemThemeHelper
{
    private const string KeyPath =
      @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

    /// <summary>Vrai si Windows est en mode sombre pour les applications.</summary>
    public static bool IsDark()
    {
        using var hive = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
        using var key = hive.OpenSubKey(KeyPath);

        var raw = key?.GetValue("AppsUseLightTheme");
        Debug.WriteLine($"[SystemTheme] AppsUseLightTheme = {raw ?? "null"} ({raw?.GetType().Name})");

        // 0 = sombre, 1 = clair. Absent ou illisible : on considère « clair » par défaut.
        return raw is int v && v == 0;
    }
}