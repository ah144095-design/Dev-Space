using DevSpace.Models;
using DevSpace.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DevSpace.Views
{
    public partial class StatsView : UserControl
    {
        private ProjectRepository _projectRepo = new ProjectRepository();
        private SnippetRepository _snippetRepo = new SnippetRepository();
        private NoteRepository _noteRepo = new NoteRepository();

        public StatsView()
        {
            InitializeComponent();
            Refresh();
        }

        public void Refresh()
        {
            var projects = _projectRepo.GetAll();
            var snippets = _snippetRepo.GetAll();
            var notes = _noteRepo.GetAll();

            // Basic counts
            CardTotalProjects.Text = projects.Count.ToString();
            CardActiveProjects.Text = projects.Count(p => p.Status == ProjectStatus.Active).ToString();
            CardTotalSnippets.Text = snippets.Count.ToString();
            CardFavSnippets.Text = snippets.Count(s => s.IsFavorite).ToString();
            CardTotalNotes.Text = notes.Count.ToString();

            // Total launches: sum of UsageCount across all snippets as a proxy metric
            CardTotalLaunches.Text = snippets.Sum(s => s.UsageCount).ToString();

            // Project status breakdown
            int active = projects.Count(p => p.Status == ProjectStatus.Active);
            int paused = projects.Count(p => p.Status == ProjectStatus.Paused);
            int completed = projects.Count(p => p.Status == ProjectStatus.Completed);

            StatusActive.Text = active.ToString();
            StatusPaused.Text = paused.ToString();
            StatusCompleted.Text = completed.ToString();

            int total = active + paused + completed;
            if (total > 0)
            {
                ColActive.Width = new GridLength(active, GridUnitType.Star);
                ColPaused.Width = new GridLength(paused == 0 ? 0.001 : paused, GridUnitType.Star);
                ColCompleted.Width = new GridLength(completed == 0 ? 0.001 : completed, GridUnitType.Star);
            }

            // Language bar chart
            var langGroups = snippets
                .GroupBy(s => string.IsNullOrEmpty(s.Language) ? "Other" : s.Language)
                .OrderByDescending(g => g.Count())
                .ToList();

            if (langGroups.Count == 0)
            {
                NoLangData.Visibility = Visibility.Visible;
                LangChart.Visibility = Visibility.Collapsed;
            }
            else
            {
                NoLangData.Visibility = Visibility.Collapsed;
                LangChart.Visibility = Visibility.Visible;

                int maxCount = langGroups.Max(g => g.Count());
                const double maxBarWidth = 160;

                var chartItems = langGroups.Select(g => new LangChartItem
                {
                    Language = g.Key,
                    Count = g.Count(),
                    BarWidth = maxCount > 0 ? (g.Count() / (double)maxCount) * maxBarWidth : 0,
                    BarColor = GetLangBrush(g.Key)
                }).ToList();

                LangChart.ItemsSource = chartItems;
            }

            // Most used snippet
            var topSnippet = snippets.OrderByDescending(s => s.UsageCount).FirstOrDefault();
            if (topSnippet != null)
            {
                TopSnippetTitle.Text = topSnippet.Title;
                TopSnippetDesc.Text = topSnippet.Description;
                TopSnippetCode.Text = topSnippet.CodeBlock;
                TopSnippetUsage.Text = $"Used {topSnippet.UsageCount} times  •  Language: {topSnippet.Language}";
            }
            else
            {
                TopSnippetTitle.Text = "No snippets yet";
                TopSnippetCode.Text = "";
                TopSnippetUsage.Text = "";
            }
        }

        private SolidColorBrush GetLangBrush(string lang)
        {
            return lang switch
            {
                "C#" => new SolidColorBrush(Color.FromRgb(0x9B, 0x59, 0xB6)),
                "JavaScript" => new SolidColorBrush(Color.FromRgb(0xF1, 0xC4, 0x0F)),
                "Python" => new SolidColorBrush(Color.FromRgb(0x34, 0x98, 0xDB)),
                "SQL" => new SolidColorBrush(Color.FromRgb(0xE6, 0x7E, 0x22)),
                "HTML" => new SolidColorBrush(Color.FromRgb(0xE7, 0x4C, 0x3C)),
                "CSS" => new SolidColorBrush(Color.FromRgb(0x1A, 0xBC, 0x9C)),
                "TypeScript" => new SolidColorBrush(Color.FromRgb(0x3B, 0x82, 0xF6)),
                _ => new SolidColorBrush(Color.FromRgb(0x64, 0x74, 0x8B))
            };
        }
    }

    public class LangChartItem
    {
        public string Language { get; set; }
        public int Count { get; set; }
        public double BarWidth { get; set; }
        public SolidColorBrush BarColor { get; set; }
    }
}
