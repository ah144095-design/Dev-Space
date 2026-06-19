using System;
using System.Collections.Generic;
using System.Linq;

namespace DevSpace.Models
{
    public class DeveloperProfile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Theme { get; set; }
        public int TotalLaunches { get; set; }

        public DeveloperStats GetStats(List<WorkspaceItem> items)
        {
            var projects = items.OfType<ProjectFolder>().ToList();
            var snippets = items.OfType<CodeSnippet>().ToList();
            var notes = items.OfType<DevNote>().ToList();

            var mostUsedLanguage = snippets
                .GroupBy(s => s.Language)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?.Key ?? "None";

            var mostActiveProject = projects
                .OrderByDescending(p => p.LastModified)
                .FirstOrDefault()?.Title ?? "None";

            int projectsLaunchedToday = projects
                .Count(p => p.LastModified.Date == DateTime.Today); // Approximation for launched today

            return new DeveloperStats
            {
                TotalProjects = projects.Count,
                TotalSnippets = snippets.Count,
                TotalNotes = notes.Count,
                MostUsedLanguage = mostUsedLanguage,
                ProjectsLaunchedToday = projectsLaunchedToday,
                MostActiveProject = mostActiveProject
            };
        }
    }
}
