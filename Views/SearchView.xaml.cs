using DevSpace.Models;
using DevSpace.Repositories;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DevSpace.Views
{
    public partial class SearchView : UserControl
    {
        private ProjectRepository _projectRepo = new ProjectRepository();
        private SnippetRepository _snippetRepo = new SnippetRepository();
        private NoteRepository _noteRepo = new NoteRepository();

        public SearchView()
        {
            InitializeComponent();
        }

        public void PerformSearch(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                EmptyState.Visibility = Visibility.Visible;
                ResultsPanel.Visibility = Visibility.Collapsed;
                return;
            }

            var q = query.ToLower();
            SearchLabel.Text = $"Results for \"{query}\"";

            var projects = _projectRepo.GetAll()
                .Where(p => p.Title.ToLower().Contains(q)
                         || (p.TechStack?.ToLower().Contains(q) == true)
                         || string.Join(",", p.Tags).ToLower().Contains(q))
                .ToList();

            var snippets = _snippetRepo.GetAll()
                .Where(s => s.Title.ToLower().Contains(q)
                         || (s.Language?.ToLower().Contains(q) == true)
                         || (s.Description?.ToLower().Contains(q) == true)
                         || (s.CodeBlock?.ToLower().Contains(q) == true))
                .ToList();

            var notes = _noteRepo.GetAll()
                .Where(n => n.Title.ToLower().Contains(q)
                         || (n.Content?.ToLower().Contains(q) == true))
                .ToList();

            bool hasAnyResults = projects.Count > 0 || snippets.Count > 0 || notes.Count > 0;

            EmptyState.Visibility = Visibility.Collapsed;
            ResultsPanel.Visibility = Visibility.Visible;
            NoResults.Visibility = hasAnyResults ? Visibility.Collapsed : Visibility.Visible;

            // Projects
            if (projects.Count > 0)
            {
                ProjectsSection.Visibility = Visibility.Visible;
                ProjectResults.ItemsSource = projects;
                ProjectCount.Text = $"({projects.Count})";
            }
            else
            {
                ProjectsSection.Visibility = Visibility.Collapsed;
            }

            // Snippets
            if (snippets.Count > 0)
            {
                SnippetsSection.Visibility = Visibility.Visible;
                SnippetResults.ItemsSource = snippets;
                SnippetCount.Text = $"({snippets.Count})";
            }
            else
            {
                SnippetsSection.Visibility = Visibility.Collapsed;
            }

            // Notes
            if (notes.Count > 0)
            {
                NotesSection.Visibility = Visibility.Visible;
                NoteResults.ItemsSource = notes;
                NoteCount.Text = $"({notes.Count})";
            }
            else
            {
                NotesSection.Visibility = Visibility.Collapsed;
            }
        }
    }
}
