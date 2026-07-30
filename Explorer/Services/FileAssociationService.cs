using Explorer.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Explorer.Services;

public static class FileAssociationService
{
    private enum ASSOCSTR
    {
        EXECUTABLE = 2,
        FRIENDLYAPPNAME = 4,
    }

    [DllImport("shell32.dll")]
    private static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

    private const uint SHCNE_ASSOCCHANGED = 0x08000000;
    private const uint SHCNF_IDLIST = 0x0000;

    [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
    private static extern int AssocQueryString(
        int flags,
        ASSOCSTR str,
        string pszAssoc,
        string? pszExtra,
        [Out] StringBuilder pszOut,
        ref int pcchOut);

    public static string? GetDefaultAppFriendlyName(string extension) =>
        QueryAssocString(extension, ASSOCSTR.FRIENDLYAPPNAME);

    private static string? QueryAssocString(string extension, ASSOCSTR str)
    {
        var length = 260;
        var sb = new StringBuilder(length);

        var result = AssocQueryString(0, str, extension, null, sb, ref length);

        return result != 0 ? null : sb.ToString();
    }

    public static void OpenWithDefault(string filePath)
    {
        Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
    }

    public static void Launch(AppCandidate app, string filePath)
    {
        var arguments = app.CommandLineTemplate is { } template
            ? BuildArguments(template, filePath)
            : $"\"{filePath}\"";

        Process.Start(new ProcessStartInfo(app.ExecutablePath)
        {
            Arguments = arguments,
            UseShellExecute = false
        });
    }

    private static string BuildArguments(string commandLineTemplate, string filePath)
    {
        var withFile = commandLineTemplate.Replace("%1", filePath);

        if (TrySplitExecutableAndArgs(withFile, out _, out var args) && !string.IsNullOrWhiteSpace(args))
            return args;

        return $"\"{filePath}\"";
    }

    private static bool TrySplitExecutableAndArgs(string commandLine, out string executable, out string arguments)
    {
        commandLine = commandLine.Trim();

        if (commandLine.StartsWith('"'))
        {
            var closingQuote = commandLine.IndexOf('"', 1);

            if (closingQuote > 0)
            {
                executable = commandLine[1..closingQuote];
                arguments = commandLine[(closingQuote + 1)..].Trim();
                return true;
            }
        }

        var firstSpace = commandLine.IndexOf(' ');

        if (firstSpace > 0)
        {
            executable = commandLine[..firstSpace];
            arguments = commandLine[(firstSpace + 1)..].Trim();
            return true;
        }

        executable = commandLine;
        arguments = string.Empty;
        return false;
    }

    public static List<AppCandidate> GetCandidateApps(string extension)
    {
        var results = new Dictionary<string, AppCandidate>(StringComparer.OrdinalIgnoreCase);

        TrySafe(() => CollectFromProgIds(extension, results));
        TrySafe(() => CollectFromUserOpenWithList(extension, results));

        return results.Values
            .OrderBy(a => a.DisplayName, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    private static void TrySafe(Action action)
    {
        try { action(); } catch { /* реестр может быть недоступен — просто пропускаем источник */ }
    }

    private static void CollectFromProgIds(string extension, Dictionary<string, AppCandidate> results)
    {
        using var progIdsKey = Registry.ClassesRoot.OpenSubKey($@"{extension}\OpenWithProgids");

        if (progIdsKey is null)
            return;

        foreach (var progId in progIdsKey.GetValueNames())
        {
            if (!string.IsNullOrEmpty(progId))
                TrySafe(() => TryAddFromProgId(progId, results));
        }
    }

    private static void TryAddFromProgId(string progId, Dictionary<string, AppCandidate> results)
    {
        using var progIdKey = Registry.ClassesRoot.OpenSubKey(progId);

        var commandLine = progIdKey?.OpenSubKey(@"shell\open\command")?.GetValue(null) as string;

        if (string.IsNullOrWhiteSpace(commandLine))
            return;

        if (!TrySplitExecutableAndArgs(commandLine, out var executable, out _))
            return;

        executable = Environment.ExpandEnvironmentVariables(executable.Trim('"'));

        if (!File.Exists(executable))
            return;

        var friendlyName =
            ResolveIndirectString(progIdKey?.OpenSubKey("Application")?.GetValue("ApplicationName") as string) ??
            ResolveIndirectString(progIdKey?.GetValue("FriendlyTypeName") as string) ??
            Path.GetFileNameWithoutExtension(executable);

        results[executable.ToLowerInvariant()] = new AppCandidate
        {
            DisplayName = friendlyName,
            ExecutablePath = executable,
            CommandLineTemplate = commandLine,
            ProgId = progId,
            Icon = ShellIconProvider.GetIcon(executable)
        };
    }

    private static void CollectFromUserOpenWithList(string extension, Dictionary<string, AppCandidate> results)
    {
        using var listKey = Registry.CurrentUser.OpenSubKey(
            $@"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\{extension}\OpenWithList");

        if (listKey is null)
            return;

        foreach (var valueName in listKey.GetValueNames())
        {
            if (valueName.Equals("MRUList", StringComparison.OrdinalIgnoreCase))
                continue;

            if (listKey.GetValue(valueName) is string exeName && !string.IsNullOrWhiteSpace(exeName))
                TrySafe(() => TryAddFromApplicationName(exeName, results));
        }
    }

    private static void TryAddFromApplicationName(string exeName, Dictionary<string, AppCandidate> results)
    {
        using var appKey = Registry.ClassesRoot.OpenSubKey($@"Applications\{exeName}");

        var commandLine = appKey?.OpenSubKey(@"shell\open\command")?.GetValue(null) as string;

        string? executable = null;

        if (!string.IsNullOrWhiteSpace(commandLine) && TrySplitExecutableAndArgs(commandLine, out var exeFromCommand, out _))
            executable = Environment.ExpandEnvironmentVariables(exeFromCommand.Trim('"'));

        executable ??= ResolveFromAppPaths(exeName);

        if (executable is null || !File.Exists(executable))
            return;

        var key = executable.ToLowerInvariant();

        if (results.ContainsKey(key))
            return;

        var friendlyName =
            ResolveIndirectString(appKey?.GetValue("FriendlyAppName") as string) ??
            Path.GetFileNameWithoutExtension(executable);

        results[key] = new AppCandidate
        {
            DisplayName = friendlyName,
            ExecutablePath = executable,
            CommandLineTemplate = commandLine ?? $"\"{executable}\" \"%1\"",
            Icon = ShellIconProvider.GetIcon(executable)
        };
    }

    private static string? ResolveFromAppPaths(string exeName)
    {
        using var key = Registry.LocalMachine.OpenSubKey(
            $@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\{exeName}");

        return key?.GetValue(null) as string;
    }

    private static string? ResolveIndirectString(string? value)
    {
        // строки вида "@shell32.dll,-1234" — системная локализация, не резолвим, просто пропускаем
        if (string.IsNullOrEmpty(value) || value.StartsWith('@'))
            return null;

        return value;
    }

    /// <summary>
    /// Прямая запись в реестр текущего пользователя. Надёжно работает только там, где
    /// для расширения ещё нет защищённого UserChoice (Windows 8+ иногда его игнорирует).
    /// </summary>
    public static bool TrySetAsDefault(string extension, AppCandidate app)
    {
        try
        {
            var progId = app.ProgId ?? RegisterFallbackProgId(app);

            using (var classesKey = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{extension}"))
                classesKey?.SetValue(null, progId);

            using (var fileExtsKey = Registry.CurrentUser.OpenSubKey(
                $@"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\{extension}", writable: true))
            {
                fileExtsKey?.DeleteSubKey("UserChoice", throwOnMissingSubKey: false);
            }

            SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);

            return true;
        }
        catch
        {
            return false;
        }
    }
    private static string RegisterFallbackProgId(AppCandidate app)
    {
        var progId = $"DatorExplorer.OpenWith.{Path.GetFileNameWithoutExtension(app.ExecutablePath)}";

        using (var key = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{progId}\shell\open\command"))
            key?.SetValue(null, app.CommandLineTemplate ?? $"\"{app.ExecutablePath}\" \"%1\"");

        app.ProgId = progId;

        return progId;
    }
}