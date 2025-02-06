using Editor;
using Sandbox;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;

[Title("Cursor")]
public class CursorEditor : ICodeEditor
{
    private const string WorkspaceFileName = "s&box.code-workspace";
    private static readonly Lazy<string> CursorPath = new(FindCursorPath);

    /// <summary>
    /// Checks if Cursor editor is installed on the system
    /// </summary>
    public bool IsInstalled() => File.Exists(CursorPath.Value);

    /// <summary>
    /// Opens a specific file in Cursor with optional line and column numbers
    /// </summary>
    /// <param name="path">Path to the file to open</param>
    /// <param name="line">Optional line number to focus</param>
    /// <param name="column">Optional column number to focus</param>
    public void OpenFile(string path, int? line = null, int? column = null)
    {
        EnsureWorkspaceExists();
        var workspacePath = Path.Combine(Environment.CurrentDirectory, WorkspaceFileName);
        
        var location = $"{path}";
        if (line.HasValue)
        {
            location += $":{line}";
            if (column.HasValue)
            {
                location += $":{column}";
            }
        }

        LaunchCursor($"\"{workspacePath}\" -g \"{location}\"");
    }

    /// <summary>
    /// Opens the current project's root directory
    /// </summary>
    public void OpenSolution()
    {
        EnsureWorkspaceExists();
        var workspacePath = Path.Combine(Environment.CurrentDirectory, WorkspaceFileName);
        LaunchCursor($"\"{workspacePath}\"");
    }

    /// <summary>
    /// Opens a specific addon's directory
    /// </summary>
    /// <param name="addon">The addon project to open</param>
    public void OpenAddon(Project addon)
    {
        if (addon == null) return;
        LaunchCursor($"\"{addon.GetRootPath()}\"");
    }

    private static void EnsureWorkspaceExists()
    {
        var workspacePath = Path.Combine(Environment.CurrentDirectory, WorkspaceFileName);
        var workspace = new
        {
            folders = EditorUtility.Projects.GetAll()
                .Where(a => a.Active)
                .Select(a => new
                {
                    name = a.Config.Ident,
                    path = a.GetRootPath().Replace(@"\", @"\\")
                }),
            extensions = new
            {
                recommendations = new[] { "ms-dotnettools.csharp" }
            },
            settings = new
            {
                omnisharp = new
                {
                    useModernNet = true,
                    enableRoslynAnalyzers = true
                }
            }
        };

        var options = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(workspacePath, JsonSerializer.Serialize(workspace, options));
    }

    private static string FindCursorPath()
    {
        var searchPaths = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "cursor", "Cursor.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Cursor", "Cursor.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Cursor", "Cursor.exe")
        };

        var cursorPath = searchPaths.FirstOrDefault(File.Exists);
        if (string.IsNullOrEmpty(cursorPath))
        {
            throw new FileNotFoundException("Cursor editor is not installed on this system.");
        }

        return cursorPath;
    }

    /// <summary>
    /// Launches Cursor with the specified arguments
    /// </summary>
    /// <param name="arguments">Command line arguments to pass to Cursor</param>
    private static void LaunchCursor(string arguments)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = CursorPath.Value,
                Arguments = arguments,
                CreateNoWindow = true,
                UseShellExecute = true
            };

            using var process = Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to launch Cursor: {ex.Message}");
            throw;
        }
    }
}