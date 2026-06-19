using System;

namespace DevSpace.Models
{
    public class DevNote : WorkspaceItem
    {
        public string Content { get; set; }
        public int LinkedProjectId { get; set; }
        public DateTime LastEdited { get; set; }

        public override void LaunchItem()
        {
            // The prompt says: "Opens the note in the editor view"
            // This behavior is usually handled at the UI layer by publishing an event
            // or setting a property. We will just leave it empty here or trigger an event.
        }

        public override string GetItemType() => "Note";
    }
}
