using DevSpace.Dialogs;
using DevSpace.Helpers;
using DevSpace.Repositories;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DevSpace.Views
{
    public partial class MainWindow : Window
    {
        private DashboardView _dashboardView;
        private ProjectsView _projectsView;
        private SnippetsView _snippetsView;
        private NotesView _notesView;
        private StatsView _statsView;
        private SearchView _searchView;
        private string _activeNav = "Dashboard";

        public MainWindow()
        {
            InitializeComponent();

            _dashboardView = new DashboardView();
            _projectsView = new ProjectsView();
            _snippetsView = new SnippetsView();
            _notesView = new NotesView();
            _statsView = new StatsView();
            _searchView = new SearchView();

            NavigateTo("Dashboard", _dashboardView);
            ThemeManager.ThemeChanged += () => UpdateThemeToggleLabel();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var profile = new ProfileRepository().GetProfile();
            if (!string.IsNullOrWhiteSpace(profile?.Name))
            {
                ProfileName.Text = profile.Name;
                ProfileInitials.Text = profile.Name.Length >= 2
                    ? profile.Name.Substring(0, 2).ToUpper()
                    : profile.Name.Substring(0, 1).ToUpper();
            }
            UpdateThemeToggleLabel();
        }

        private void UpdateThemeToggleLabel() =>
            BtnThemeToggle.Content = ThemeManager.IsDark ? "Light mode" : "Dark mode";

        private void ThemeToggle_Click(object sender, RoutedEventArgs e) =>
            ThemeManager.Toggle(this, ThemeOverlay);

        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn)
                return;

            var tag = btn.Name switch
            {
                "NavDashboard" => "Dashboard",
                "NavProjects" => "Projects",
                "NavSnippets" => "Snippets",
                "NavNotes" => "Notes",
                "NavStats" => "Stats",
                _ => btn.Tag as string
            };

            if (string.IsNullOrEmpty(tag))
                return;

            switch (tag)
            {
                case "Dashboard":
                    _dashboardView.Refresh();
                    NavigateTo(tag, _dashboardView);
                    break;
                case "Projects":
                    _projectsView.Refresh();
                    NavigateTo(tag, _projectsView);
                    break;
                case "Snippets":
                    _snippetsView.Refresh();
                    NavigateTo(tag, _snippetsView);
                    break;
                case "Notes":
                    _notesView.Refresh();
                    NavigateTo(tag, _notesView);
                    break;
                case "Stats":
                    _statsView.Refresh();
                    NavigateTo(tag, _statsView);
                    break;
            }
        }

        private void NavigateTo(string tag, UIElement view)
        {
            _activeNav = tag;
            PageTitle.Text = tag == "Stats" ? "Analytics" : tag;
            UpdateNavHighlight();
            ViewAnimations.AnimateContentSwap(MainContent, view);
        }

        private void UpdateNavHighlight()
        {
            SetNavActive(NavDashboard, _activeNav == "Dashboard");
            SetNavActive(NavProjects, _activeNav == "Projects");
            SetNavActive(NavSnippets, _activeNav == "Snippets");
            SetNavActive(NavNotes, _activeNav == "Notes");
            SetNavActive(NavStats, _activeNav == "Stats");
        }

        private static void SetNavActive(Button button, bool active) =>
            button.Tag = active ? "Active" : button.Name switch
            {
                "NavDashboard" => "Dashboard",
                "NavProjects" => "Projects",
                "NavSnippets" => "Snippets",
                "NavNotes" => "Notes",
                "NavStats" => "Stats",
                _ => button.Tag
            };

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                _searchView.PerformSearch(SearchBox.Text);
                if (MainContent.Content != _searchView)
                    ViewAnimations.AnimateContentSwap(MainContent, _searchView);
            }
            else if (MainContent.Content == _searchView)
            {
                _dashboardView.Refresh();
                NavigateTo("Dashboard", _dashboardView);
            }
        }

        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                _searchView.PerformSearch(SearchBox.Text);
        }

        private void NewProject_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddProjectDialog { Owner = this };
            if (dialog.ShowDialog() != true)
                return;

            _dashboardView.Refresh();
            _projectsView.Refresh();

            if (MainContent.Content != _dashboardView && MainContent.Content != _projectsView)
            {
                _projectsView.Refresh();
                NavigateTo("Projects", _projectsView);
            }
        }
    }
}
