using System.Windows;

namespace DevSpace.Models
{
    public class CodeSnippet : WorkspaceItem
    {
        public string Language { get; set; }
        public string CodeBlock { get; set; }
        public string Description { get; set; }
        public bool IsFavorite { get; set; }
        public int UsageCount { get; set; }

        public override void LaunchItem()
        {
            if (!string.IsNullOrEmpty(CodeBlock))
            {
                Helpers.ClipboardHelper.SetText(CodeBlock);
                UsageCount++;
                // Note: The usage count should be saved to DB via repository, which the ViewModel will handle.
            }
        }

        public override string GetItemType() => "Snippet";
    }
}
