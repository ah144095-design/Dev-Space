using System;
using System.Collections.Generic;

namespace DevSpace.Models
{
    public abstract class WorkspaceItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime DateCreated { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public abstract void LaunchItem();
        public abstract string GetItemType();
    }
}
