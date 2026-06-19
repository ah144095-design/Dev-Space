using DevSpace.Dialogs;
using DevSpace.Models;
using DevSpace.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DevSpace.Views
{
    public partial class NotesView : UserControl
    {
        private NoteRepository _noteRepo = new NoteRepository();
        private ProjectRepository _projectRepo = new ProjectRepository();
        private List<ProjectFolder> _allProjects = new List<ProjectFolder>();
        private List<DevNote> _allNotes = new List<DevNote>();
        private int _selectedProjectId = 0;
        private DevNote _editingNote = null;

        public NotesView()
        {
            InitializeComponent();
            Refresh();
        }

        public void Refresh()
        {
            _allProjects = _projectRepo.GetAll();
            _allNotes = _noteRepo.GetAll();
            ProjectList.ItemsSource = _allProjects;
            ShowNotesForProject(_selectedProjectId);
        }

        private void ShowNotesForProject(int projectId)
        {
            _selectedProjectId = projectId;

            IEnumerable<DevNote> notes;
            if (projectId == 0)
            {
                notes = _allNotes;
                SelectedProjectLabel.Text = "All Notes";
            }
            else
            {
                var proj = _allProjects.FirstOrDefault(p => p.Id == projectId);
                SelectedProjectLabel.Text = proj != null ? $"Notes for: {proj.Title}" : "Notes";
                notes = _allNotes.Where(n => n.LinkedProjectId == projectId);
            }

            var noteList = notes.OrderByDescending(n => n.LastEdited).ToList();
            NotesList.ItemsSource = noteList;
            EmptyState.Visibility = noteList.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Project_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
                ShowNotesForProject(id);
        }

        private void AddNote_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddNoteDialog(null, _selectedProjectId);
            dialog.Owner = Window.GetWindow(this);
            if (dialog.ShowDialog() == true)
                Refresh();
        }

        private void OpenNote_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                _editingNote = _allNotes.FirstOrDefault(n => n.Id == id);
                if (_editingNote != null)
                {
                    NoteEditor.Text = _editingNote.Content;
                    NoteEditor.IsEnabled = true;
                    SaveNoteBtn.IsEnabled = true;
                    EditingNoteLabel.Text = $"Editing: {_editingNote.Title}";
                }
            }
        }

        private void NoteEditor_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Mark unsaved
            if (_editingNote != null && !EditingNoteLabel.Text.EndsWith("*"))
                EditingNoteLabel.Text += " *";
        }

        private void SaveNoteContent_Click(object sender, RoutedEventArgs e)
        {
            if (_editingNote != null)
            {
                _editingNote.Content = NoteEditor.Text;
                _editingNote.LastEdited = DateTime.Now;
                _noteRepo.Update(_editingNote);
                EditingNoteLabel.Text = $"Editing: {_editingNote.Title}";
                Refresh();
                // Re-open the same note so editor stays populated
                NoteEditor.Text = _editingNote.Content;
                NoteEditor.IsEnabled = true;
                SaveNoteBtn.IsEnabled = true;
            }
        }

        private void EditNote_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var note = _allNotes.FirstOrDefault(n => n.Id == id);
                if (note != null)
                {
                    var dialog = new AddNoteDialog(note);
                    dialog.Owner = Window.GetWindow(this);
                    if (dialog.ShowDialog() == true)
                        Refresh();
                }
            }
        }

        private void DeleteNote_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var note = _allNotes.FirstOrDefault(n => n.Id == id);
                if (note == null) return;

                var result = MessageBox.Show(
                    $"Delete note \"{note.Title}\"? This cannot be undone.",
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    if (_editingNote?.Id == id)
                    {
                        _editingNote = null;
                        NoteEditor.Text = "";
                        NoteEditor.IsEnabled = false;
                        SaveNoteBtn.IsEnabled = false;
                        EditingNoteLabel.Text = "No note selected";
                    }
                    _noteRepo.Delete(id);
                    Refresh();
                }
            }
        }
    }
}
