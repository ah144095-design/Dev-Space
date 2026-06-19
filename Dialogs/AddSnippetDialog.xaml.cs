using DevSpace.Models;
using DevSpace.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace DevSpace.Dialogs
{
    public partial class AddSnippetDialog : Window
    {
        private SnippetRepository _repo = new SnippetRepository();
        public CodeSnippet Snippet { get; private set; }

        private static readonly List<string> Languages = new List<string>
        {
            "C#", "JavaScript", "Python", "SQL", "HTML", "CSS", "TypeScript",
            "Java", "Go", "Rust", "PowerShell", "Bash", "Other"
        };

        public AddSnippetDialog(CodeSnippet existingSnippet = null)
        {
            InitializeComponent();

            CmbLanguage.ItemsSource = Languages;

            if (existingSnippet != null)
            {
                Snippet = existingSnippet;
                TxtTitle.Text = Snippet.Title;
                CmbLanguage.SelectedItem = Snippet.Language;
                TxtDescription.Text = Snippet.Description;
                TxtCodeBlock.Text = Snippet.CodeBlock;
                TxtTags.Text = string.Join(",", Snippet.Tags);
                ChkFavorite.IsChecked = Snippet.IsFavorite;
                Title = "Edit Snippet";
            }
            else
            {
                CmbLanguage.SelectedIndex = 0;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtTitle.Text))
            {
                MessageBox.Show("Title is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(TxtCodeBlock.Text))
            {
                MessageBox.Show("Code block cannot be empty.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool isNew = Snippet == null;
            if (isNew)
            {
                Snippet = new CodeSnippet { DateCreated = DateTime.Now };
            }

            Snippet.Title = TxtTitle.Text.Trim();
            Snippet.Language = CmbLanguage.SelectedItem?.ToString() ?? "Other";
            Snippet.Description = TxtDescription.Text.Trim();
            Snippet.CodeBlock = TxtCodeBlock.Text;
            Snippet.IsFavorite = ChkFavorite.IsChecked == true;
            Snippet.Tags = TxtTags.Text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(t => t.Trim()).ToList();

            if (isNew)
                _repo.Add(Snippet);
            else
                _repo.Update(Snippet);

            DialogResult = true;
        }
    }
}
