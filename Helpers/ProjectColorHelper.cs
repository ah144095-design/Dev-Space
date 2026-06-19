using System;
using System.Windows.Media;

namespace DevSpace.Helpers
{
    public static class ProjectColorHelper
    {
        public static readonly string[] PresetColors =
        {
            "#D97706", "#2563EB", "#7C3AED", "#059669",
            "#E11D48", "#0891B2", "#DB2777", "#475569"
        };

        public const string DefaultColor = "#D97706";

        public static string Normalize(string hex) =>
            string.IsNullOrWhiteSpace(hex) ? DefaultColor : hex.Trim();

        public static SolidColorBrush ToBrush(string hex, double opacity = 1.0)
        {
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(Normalize(hex));
                if (opacity < 1.0)
                    color = Color.FromArgb((byte)(opacity * 255), color.R, color.G, color.B);
                return new SolidColorBrush(color);
            }
            catch
            {
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(DefaultColor));
            }
        }

        public static SolidColorBrush ToSoftBrush(string hex) => ToBrush(hex, 0.18);

        public static string GetInitial(string title)
        {
            if (string.IsNullOrWhiteSpace(title)) return "P";
            return char.ToUpper(title.Trim()[0]).ToString();
        }
    }
}
