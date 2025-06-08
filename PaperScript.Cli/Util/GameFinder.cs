using Microsoft.Win32;

namespace PaperScript.Cli.Util;

public class GameFinder
{
#if WINDOWS
    public static string? GetSkyrimSEInstallDir() {
        var registryPath = @"Computer\HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Bethesda Softworks\Skyrim Special Edition";

        using var key = Registry.LocalMachine.OpenSubKey(registryPath);
        return key?.GetValue("Installed Path")?.ToString();
    }
#else
    public static string? GetSkyrimSeInstallDir()
    {
        return null;
    }
#endif
}