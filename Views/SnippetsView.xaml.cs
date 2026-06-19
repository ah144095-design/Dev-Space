using DevSpace.Dialogs;
using DevSpace.Models;
using DevSpace.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DevSpace.Views
{
    public partial class SnippetsView : UserControl
    {
        private SnippetRepository _repo = new SnippetRepository();
        private List<CodeSnippet> _allSnippets = new List<CodeSnippet>();
        private string _currentLang = "All";

        private static readonly List<string> Languages = new List<string>
        {
            "All", "C#", "JavaScript", "Python", "SQL", "HTML", "CSS",
            "TypeScript", "Java", "Go", "Rust", "PowerShell", "Bash", "Other"
        };

        public SnippetsView()
        {
            InitializeComponent();
            LangFilterList.ItemsSource = Languages;
            Refresh();
        }

        public void Refresh()
        {
            _allSnippets = _repo.GetAll();
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            IEnumerable<CodeSnippet> filtered = _allSnippets;
            if (_currentLang != "All")
                filtered = filtered.Where(s => s.Language == _currentLang);

            var result = filtered.ToList();
            SnippetsList.ItemsSource = result;
            EmptyState.Visibility = result.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void LangFilter_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string lang)
            {
                _currentLang = lang;
                ApplyFilter();
            }
        }

        private void AddSnippet_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddSnippetDialog();
            dialog.Owner = Window.GetWindow(this);
            if (dialog.ShowDialog() == true)
                Refresh();
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var snippet = _repo.GetById(id);
                if (snippet != null)
                {
                    snippet.LaunchItem(); // copies to clipboard and increments UsageCount
                    _repo.Update(snippet);
                    Refresh();
                    MessageBox.Show("Code copied to clipboard!", "Copied", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void ToggleFav_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var snippet = _repo.GetById(id);
                if (snippet != null)
                {
                    snippet.IsFavorite = !snippet.IsFavorite;
                    _repo.Update(snippet);
                    Refresh();
                }
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var snippet = _repo.GetById(id);
                if (snippet != null)
                {
                    var dialog = new AddSnippetDialog(snippet);
                    dialog.Owner = Window.GetWindow(this);
                    if (dialog.ShowDialog() == true)
                        Refresh();
                }
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var snippet = _repo.GetById(id);
                if (snippet == null) return;
                var result = MessageBox.Show(
                    $"Delete snippet \"{snippet.Title}\"? This cannot be undone.",
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
