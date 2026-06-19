using DevSpace.Helpers;
using DevSpace.Models;
using DevSpace.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;

namespace DevSpace.Dialogs
{
    public partial class AddProjectDialog : Window
    {
        private readonly ProjectRepository _repo = new ProjectRepository();
        public ProjectFolder Project { get; private set; }
        private string _selectedColor = ProjectColorHelper.DefaultColor;

        public sealed class OptionItem
        {
            public string Display { get; set; }
            public string Description { get; set; }
            public object Value { get; set; }
        }

        public AddProjectDialog(ProjectFolder existingProject = null)
        {
            InitializeComponent();

            CmbLaunchWith.ItemsSource = BuildLaunchOptions();
            CmbStatus.ItemsSource = BuildStatusOptions();

            if (existingProject != null)
            {
                Project = existingProject;
                TxtHeading.Text = "Edit Project";
                Title = "Edit Project";
                TxtTitle.Text = Project.Title;
                TxtLocalPath.Text = Project.LocalPath;
                SelectLaunchOption(Project.IDEType);
                TxtGitHubUrl.Text = Project.GitHubRepoUrl;
                TxtTechStack.Text = Project.TechStack;
                SelectStatusOption(Project.Status);
                TxtTags.Text = string.Join(", ", Project.Tags);
                TxtCustomLaunch.Text = Project.CustomLaunchCommand;
                _selectedColor = ProjectColorHelper.Normalize(Project.AccentColor);
            }
            else
            {
                TxtHeading.Text = "New Project";
                CmbLaunchWith.SelectedIndex = 1;
                CmbStatus.SelectedIndex = 0;
            }

            BuildColorSwatches();
        }

        private void BuildColorSwatches()
        {
            ColorSwatches.Children.Clear();
            foreach (var hex in ProjectColorHelper.PresetColors)
            {
                var btn = new Button
                {
                    Width = 24, Height = 24,
                    Margin = new Thickness(0, 0, 8, 8),
                    Background = ProjectColorHelper.ToBrush(hex),
                    Tag = hex,
                    Cursor = System.Windows.Input.Cursors.Hand,
                    BorderThickness = new Thickness(2),
                    BorderBrush = string.Equals(hex, _selectedColor, StringComparison.OrdinalIgnoreCase)
                        ? Brushes.White : Brushes.Transparent
                };

                btn.Click += (s, e) =>
                {
                    _selectedColor = hex;
                    TxtSelectedColor.Text = "Color: " + hex;
                    
                    foreach (Button b in ColorSwatches.Children)
                    {
                        var bHex = b.Tag.ToString();
                        b.BorderBrush = string.Equals(bHex, _selectedColor, StringComparison.OrdinalIgnoreCase)
                            ? Brushes.White : Brushes.Transparent;
                    }
                };

                ColorSwatches.Children.Add(btn);
            }
            TxtSelectedColor.Text = "Color: " + _selectedColor;
        }

        private static List<OptionItem> BuildLaunchOptions()
        {
            return Enum.GetValues(typeof(IDEType))
                .Cast<IDEType>()
                .Select(t => new OptionItem
                {
                    Value = t,
                    Display = IdeLauncher.GetDisplayName(t),
                    Description = IdeLauncher.GetDescription(t)
                })
                .ToList();
        }

        private static List<OptionItem> BuildStatusOptions() =>
            Enum.GetValues(typeof(ProjectStatus))
                .Cast<ProjectStatus>()
                .Select(s => new OptionItem
                {
                    Value = s,
                    Display = s.ToString(),
                    Description = ""
                })
                .ToList();

        private void SelectLaunchOption(IDEType type)
        {
            CmbLaunchWith.SelectedItem = ((List<OptionItem>)CmbLaunchWith.ItemsSource)
                .FirstOrDefault(o => (IDEType)o.Value == type);
        }

        private void SelectStatusOption(ProjectStatus status)
        {
            CmbStatus.SelectedItem = ((List<OptionItem>)CmbStatus.ItemsSource)
                .FirstOrDefault(o => (ProjectStatus)o.Value == status);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) => ViewAnimations.FadeIn(this, 180);

        private void CmbLaunchWith_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (CmbLaunchWith.SelectedItem is OptionItem item)
            {
                TxtLaunchHint.Text = item.Description;
                if (PanelCustomLaunch != null)
                {
                    PanelCustomLaunch.Visibility = ((IDEType)item.Value == IDEType.CustomApp) 
                        ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        private void BrowseCustomLaunch_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select executable",
                Filter = "Executables (*.exe;*.cmd;*.bat)|*.exe;*.cmd;*.bat|All files (*.*)|*.*"
            };

            if (dialog.ShowDialog() == true)
                TxtCustomLaunch.Text = dialog.FileName;
        }

        private void BrowsePath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Select project folder",
                Multiselect = false
            };

            if (!string.IsNullOrWhiteSpace(TxtLocalPath.Text) && System.IO.Directory.Exists(TxtLocalPath.Text))
                dialog.InitialDirectory = TxtLocalPath.Text;

            if (dialog.ShowDialog() == true)
                TxtLocalPath.Text = dialog.FolderName;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtTitle.Text))
            {
                System.Windows.MessageBox.Show(this, "Title is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtTitle.Focus();
                return;
            }

            if (CmbLaunchWith.SelectedItem is not OptionItem launchItem ||
                CmbStatus.SelectedItem is not OptionItem statusItem)
            {
                System.Windows.MessageBox.Show(this, "Please select launch app and status.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var isNew = Project == null;
            if (isNew)
            {
                Project = new ProjectFolder { DateCreated = DateTime.Now };
            }

            Project.Title = TxtTitle.Text.Trim();
            Project.LocalPath = TxtLocalPath.Text.Trim();
            Project.IDEType = (IDEType)launchItem.Value;
            Project.CustomLaunchCommand = TxtCustomLaunch.Text.Trim();
            Project.GitHubRepoUrl = TxtGitHubUrl.Text.Trim();
            Project.TechStack = TxtTechStack.Text.Trim();
            Project.Status = (ProjectStatus)statusItem.Value;
            Project.Tags = TxtTags.Text
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .Where(t => t.Length > 0)
                .ToList();
            Project.LastModified = DateTime.Now;

            if (isNew)
                _repo.Add(Project);
            else
                _repo.Update(Project);

            DialogResult = true;
        }
    }
}
