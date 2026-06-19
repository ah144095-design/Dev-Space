using DevSpace.Models;
using DevSpace.Repositories;
using System;
using System.Linq;
using System.Windows;

namespace DevSpace.Dialogs
{
    public partial class AddNoteDialog : Window
    {
        private NoteRepository _noteRepo = new NoteRepository();
        private ProjectRepository _projectRepo = new ProjectRepository();
        public DevNote Note { get; private set; }

        public AddNoteDialog(DevNote existingNote = null, int preselectedProjectId = 0)
        {
            InitializeComponent();

            var projects = _projectRepo.GetAll();
            CmbProject.ItemsSource = projects;

            if (existingNote != null)
            {
                Note = existingNote;
                TxtTitle.Text = Note.Title;
                TxtContent.Text = Note.Content;
                TxtTags.Text = string.Join(",", Note.Tags);
                CmbProject.SelectedValue = Note.LinkedProjectId;
                Title = "Edit Note";
            }
            else
            {
                if (preselectedProjectId > 0)
                    CmbProject.SelectedValue = preselectedProjectId;
                else if (projects.Count > 0)
                    CmbProject.SelectedIndex = 0;
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
            if (string.IsNullOrWhiteSpace(TxtContent.Text))
            {
                MessageBox.Show("Content cannot be empty.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool isNew = Note == null;
            if (isNew)
                Note = new DevNote { DateCreated = DateTime.Now };

            Note.Title = TxtTitle.Text.Trim();
            Note.Content = TxtContent.Text;
            Note.LinkedProjectId = CmbProject.SelectedValue is int id ? id : 0;
            Note.LastEdited = DateTime.Now;
            Note.Tags = TxtTags.Text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                     .Select(t => t.Trim()).ToList();

            if (isNew)
                _noteRepo.Add(Note);
            else
                _noteRepo.Update(Note);

            DialogResult = true;
        }
    }
}
