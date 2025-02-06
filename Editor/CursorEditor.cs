using Editor;
using Sandbox;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

[Title("Cursor")]
public class CursorEditor : ICodeEditor
{
    // Cache the cursor executable location
    private static string cursorLocation;

    /// <summary>
    /// Checks if Cursor editor is installed on the system
    /// </summary>
    public bool IsInstalled() => !string.IsNullOrEmpty(GetCursorLocation());

    /// <summary>
    /// Opens a specific file in Cursor with optional line and column numbers
    /// </summary>
    /// <param name="path">Path to the file to open</param>
    /// <param name="line">Optional line number to focus</param>
    /// <param name="column">Optional column number to focus</param>
    public void OpenFile(string path, int? line = null, int? column = null)
    {
        var cursorPath = GetCursorLocation();
        if (string.IsNullOrEmpty(cursorPath)) 
        {
            Log.Warning("Cursor editor not found");
            return;
        }

        var arguments = $"\"{path}\"";
        if (line.HasValue)
        {
            arguments += $":{line}";
            if (column.HasValue)
            {
                arguments += $":{column}";
            }
        }

        Launch(arguments);
    }

    /// <summary>
    /// Opens the current project's root directory
    /// </summary>
    public void OpenSolution()
    {
        Launch($"\"{Environment.CurrentDirectory}\"");
    }

    /// <summary>
    /// Opens a specific addon's directory
    /// </summary>
    /// <param name="addon">The addon project to open</param>
    public void OpenAddon(Project addon)
    {
        if (addon == null)
        {
            Log.Warning("Cannot open null addon");
            return;
        }
        
        var projectPath = addon.GetRootPath();
        Launch($"\"{projectPath}\"");
    }

    /// <summary>
    /// Gets the installation path of Cursor editor
    /// </summary>
    private static string GetCursorLocation()
    {
        // Return cached location if available
        if (!string.IsNullOrEmpty(cursorLocation))
            return cursorLocation;

        // Check common installation paths
        var commonPaths = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "cursor", "Cursor.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Cursor", "Cursor.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Cursor", "Cursor.exe")
        };

        cursorLocation = commonPaths.FirstOrDefault(File.Exists);
        
        if (string.IsNullOrEmpty(cursorLocation))
        {
            Log.Error("Could not find Cursor installation path");
        }
        return cursorLocation;
    }

    /// <summary>
    /// Launches Cursor with the specified arguments
    /// </summary>
    /// <param name="arguments">Command line arguments to pass to Cursor</param>
    private static void Launch(string arguments)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = GetCursorLocation(),
                Arguments = arguments,
                CreateNoWindow = true,
            };

            Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to launch Cursor: {ex.Message}");
        }
    }
}