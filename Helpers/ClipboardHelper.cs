using System.Windows;

namespace DevSpace.Helpers
{
    public static class ClipboardHelper
    {
        public static void SetText(string text)
        {
            try
            {
                Clipboard.SetText(text);
            }
            catch
            {
                // Ignored - Clipboard operations can sometimes fail if locked by another process
            }
        }
    }
}
