using DevSpace.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace DevSpace.Helpers
{
    public static class IdeLauncher
    {
        public static string GetDisplayName(IDEType type) => type switch
        {
            IDEType.VisualStudio => "Visual Studio",
            IDEType.VSCode => "Visual Studio Code",
            IDEType.Cursor => "Cursor",
            IDEType.Rider => "JetBrains Rider",
            IDEType.IntelliJ => "IntelliJ IDEA",
            IDEType.WebStorm => "WebStorm",
            IDEType.SublimeText => "Sublime Text",
            IDEType.NotepadPlusPlus => "Notepad++",
            IDEType.WindowsTerminal => "Windows Terminal",
            IDEType.FileExplorer => "File Explorer",
            IDEType.AndroidStudio => "Android Studio",
            IDEType.Eclipse => "Eclipse",
            IDEType.Fleet => "JetBrains Fleet",
            IDEType.Other => "Default (File Explorer)",
            IDEType.CustomApp => "Custom App (.exe)",
            _ => type.ToString()
        };

        public static string GetDescription(IDEType type) => type switch
        {
            IDEType.VisualStudio => "Opens with devenv.exe",
            IDEType.VSCode => "Opens with code command",
            IDEType.Cursor => "Opens with cursor command",
            IDEType.Rider => "Opens with rider command",
            IDEType.IntelliJ => "Opens with idea64 command",
            IDEType.WebStorm => "Opens with webstorm64 command",
            IDEType.SublimeText => "Opens with subl command",
            IDEType.NotepadPlusPlus => "Opens with Notepad++",
            IDEType.WindowsTerminal => "Opens folder in Windows Terminal",
            IDEType.FileExplorer => "Opens folder in Explorer",
            IDEType.AndroidStudio => "Opens with studio64 command",
            IDEType.Eclipse => "Opens with eclipse command",
            IDEType.Fleet => "Opens with fleet command",
            IDEType.Other => "Opens folder in Explorer",
            IDEType.CustomApp => "Browse or enter a custom path to the launcher",
            _ => ""
        };

        public static bool TryLaunch(ProjectFolder project, Window owner = null)
        {
            if (string.IsNullOrWhiteSpace(project?.LocalPath))
            {
                ShowMessage(owner, "No local path set for this project.", "Cannot Launch");
                return false;
            }

            if (!Directory.Exists(project.LocalPath))
            {
                ShowMessage(owner, $"Folder not found:\n{project.LocalPath}", "Cannot Launch");
                return false;
            }

            try
            {
                if (project.IDEType == IDEType.CustomApp && !string.IsNullOrWhiteSpace(project.CustomLaunchCommand))
                {
                    Process.Start(new ProcessStartInfo(project.CustomLaunchCommand, $"\"{project.LocalPath}\"") { UseShellExecute = true });
                    project.LastModified = DateTime.Now;
                    return true;
                }

                var startInfo = BuildStartInfo(project.IDEType, project.LocalPath);
                Process.Start(startInfo);
                project.LastModified = DateTime.Now;
                return true;
            }
            catch (Exception ex)
            {
                try
                {
                    Process.Start(new ProcessStartInfo(project.LocalPath) { UseShellExecute = true });
                    return true;
                }
                catch
                {
                    ShowMessage(owner,
                        $"Could not launch with {GetDisplayName(project.IDEType)}.\n\n" +
                        $"Make sure the app is installed and on your PATH.\n\n{ex.Message}",
                        "Launch Failed");
                    return false;
                }
            }
        }

        private static ProcessStartInfo BuildStartInfo(IDEType type, string path)
        {
            var quoted = $"\"{path}\"";

            return type switch
            {
                IDEType.VisualStudio => Shell("devenv", quoted),
                IDEType.VSCode => Shell("code", quoted),
                IDEType.Cursor => Shell("cursor", quoted),
                IDEType.Rider => Shell("rider", quoted),
                IDEType.IntelliJ => Shell("idea64", quoted),
                IDEType.WebStorm => Shell("webstorm64", quoted),
                IDEType.SublimeText => Shell("subl", quoted),
                IDEType.NotepadPlusPlus => Shell("notepad++", quoted),
                IDEType.WindowsTerminal => Shell("wt", $"-d {quoted}"),
                IDEType.AndroidStudio => Shell("studio64", quoted),
                IDEType.Eclipse => Shell("eclipse", quoted),
                IDEType.Fleet => Shell("fleet", quoted),
                IDEType.FileExplorer or IDEType.Other => new ProcessStartInfo(path) { UseShellExecute = true },
                _ => new ProcessStartInfo(path) { UseShellExecute = true }
            };
        }

        private static ProcessStartInfo Shell(string fileName, string arguments) =>
            new ProcessStartInfo(fileName, arguments) { UseShellExecute = true };

        private static void ShowMessage(Window owner, string message, string title)
        {
            if (owner != null)
                MessageBox.Show(owner, message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
            else
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
