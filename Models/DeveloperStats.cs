namespace DevSpace.Models
{
    public class DeveloperStats
    {
        public int TotalProjects { get; set; }
        public int TotalSnippets { get; set; }
        public int TotalNotes { get; set; }
        public string MostUsedLanguage { get; set; }
        public int ProjectsLaunchedToday { get; set; }
        public string MostActiveProject { get; set; }
    }
}
