using DevSpace.Helpers;
using System;

namespace DevSpace.Models
{
    public class ProjectFolder : WorkspaceItem
    {
        public string LocalPath { get; set; }
        public IDEType IDEType { get; set; }
        public string CustomLaunchCommand { get; set; }
        public string AccentColor { get; set; } = ProjectColorHelper.DefaultColor;
        public string GitHubRepoUrl { get; set; }
        public string TechStack { get; set; }
        public ProjectStatus Status { get; set; }
        public DateTime LastModified { get; set; }

        public string LaunchAppDisplay => IdeLauncher.GetDisplayName(IDEType);
        public string ProjectInitial => ProjectColorHelper.GetInitial(Title);

        public override void LaunchItem() => IdeLauncher.TryLaunch(this);

        public override string GetItemType() => "Project";
    }
}
