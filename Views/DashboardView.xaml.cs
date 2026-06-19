using DevSpace.Helpers;
using DevSpace.Repositories;
using DevSpace.Models;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;

namespace DevSpace.Views
{
    public partial class DashboardView : UserControl
    {
        private ProjectRepository _projectRepo = new ProjectRepository();
        private SnippetRepository _snippetRepo = new SnippetRepository();
        private NoteRepository _noteRepo = new NoteRepository();

        public DashboardView()
        {
            InitializeComponent();
            Refresh();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) =>
            ViewAnimations.FadeIn(this);

        public void Refresh()
        {
            var projects = _projectRepo.GetAll();
            var snippets = _snippetRepo.GetAll();
            var notes = _noteRepo.GetAll();

            var items = new List<WorkspaceItem>();
            items.AddRange(projects);
            items.AddRange(snippets);
            items.AddRange(notes);

            var profile = new DeveloperProfile();
            var stats = profile.GetStats(items);

            StatTotalProjects.Text = stats.TotalProjects.ToString();
            StatTotalSnippets.Text = stats.TotalSnippets.ToString();
            StatTopLanguage.Text = stats.MostUsedLanguage;
            StatLaunchesToday.Text = stats.ProjectsLaunchedToday.ToString();

            QuickLaunchItems.ItemsSource = projects.Take(5).ToList();
            RecentProjectsList.ItemsSource = projects.OrderByDescending(p => p.LastModified).Take(6).ToList();
        }

        private void QuickLaunch_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var project = _projectRepo.GetById(id);
                if (project != null)
                    IdeLauncher.TryLaunch(project, Window.GetWindow(this));
            }
        }

        private void LaunchProject_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var project = _projectRepo.GetById(id);
                if (project != null)
                    IdeLauncher.TryLaunch(project, Window.GetWindow(this));
            }
        }

        private void GitHubProject_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var project = _projectRepo.GetById(id);
                if (project != null && !string.IsNullOrEmpty(project.GitHubRepoUrl))
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo(project.GitHubRepoUrl) { UseShellExecute = true });
                    }
                    catch { }
                }
            }
        }
    }
}
