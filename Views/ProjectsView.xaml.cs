using DevSpace.Dialogs;
using DevSpace.Helpers;
using DevSpace.Models;
using DevSpace.Repositories;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DevSpace.Views
{
    public partial class ProjectsView : UserControl
    {
        private readonly ProjectRepository _repo = new ProjectRepository();
        private List<ProjectFolder> _allProjects = new List<ProjectFolder>();
        private string _currentFilter = "All";
        private string _searchText = "";

        public ProjectsView()
        {
            InitializeComponent();
            Refresh();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) =>
            ViewAnimations.FadeIn(this);

        public void Refresh()
        {
            _allProjects = _repo.GetAll();
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            var filtered = _allProjects.AsEnumerable();

            if (_currentFilter != "All")
            {
                if (System.Enum.TryParse<ProjectStatus>(_currentFilter, out var status))
                    filtered = filtered.Where(p => p.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(_searchText))
            {
                var q = _searchText.ToLower();
                filtered = filtered.Where(p =>
                    p.Title.ToLower().Contains(q) ||
                    (p.TechStack?.ToLower().Contains(q) == true) ||
                    p.LaunchAppDisplay.ToLower().Contains(q));
            }

            var result = filtered.ToList();
            ProjectsList.ItemsSource = result;
            EmptyState.Visibility = result.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            UpdateFilterButtons();
        }

        private void UpdateFilterButtons()
        {
            SetFilterStyle(BtnFilterAll, _currentFilter == "All");
            SetFilterStyle(BtnFilterActive, _currentFilter == "Active");
            SetFilterStyle(BtnFilterPaused, _currentFilter == "Paused");
            SetFilterStyle(BtnFilterCompleted, _currentFilter == "Completed");
        }

        private void SetFilterStyle(Button button, bool active)
        {
            button.Style = (Style)FindResource(active ? "PrimaryButtonStyle" : "OutlineButtonStyle");
        }

        private void Filter_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string tag)
            {
                _currentFilter = tag;
                ApplyFilter();
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchText = SearchBox.Text;
            ApplyFilter();
        }

        private void AddProject_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddProjectDialog { Owner = Window.GetWindow(this) };
            if (dialog.ShowDialog() == true)
                Refresh();
        }

        private void Launch_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var proj = _repo.GetById(id);
                if (proj != null)
                    IdeLauncher.TryLaunch(proj, Window.GetWindow(this));
            }
        }

        private void GitHub_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var proj = _repo.GetById(id);
                if (proj != null && !string.IsNullOrEmpty(proj.GitHubRepoUrl))
                {
                    try { Process.Start(new ProcessStartInfo(proj.GitHubRepoUrl) { UseShellExecute = true }); }
                    catch { MessageBox.Show("Could not open URL.", "Error"); }
                }
                else
                {
                    MessageBox.Show("No GitHub URL set for this project.", "Info");
                }
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var proj = _repo.GetById(id);
                if (proj != null)
                {
                    var dialog = new AddProjectDialog(proj) { Owner = Window.GetWindow(this) };
                    if (dialog.ShowDialog() == true)
                        Refresh();
                }
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var proj = _repo.GetById(id);
                if (proj == null) return;

                var result = MessageBox.Show(
                    $"Delete project \"{proj.Title}\"? This cannot be undone.",
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    _repo.Delete(id);
                    Refresh();
                }
            }
        }
    }
}
