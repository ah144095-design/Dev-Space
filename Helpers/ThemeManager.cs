using DevSpace.Repositories;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace DevSpace.Helpers
{
    public static class ThemeManager
    {
        private const string LightUri = "/DevSpace;component/Themes/LightTheme.xaml";
        private const string DarkUri = "/DevSpace;component/Themes/DarkTheme.xaml";

        public static bool IsDark { get; private set; }

        public static event Action ThemeChanged;

        public static void Initialize()
        {
            var profile = new ProfileRepository().GetProfile();
            var useDark = string.Equals(profile?.Theme, "Dark", StringComparison.OrdinalIgnoreCase);
            ApplyTheme(useDark, animate: false);
        }

        public static void Toggle(Window host, Border overlay = null)
        {
            if (overlay != null && host != null)
                AnimateToggle(host, overlay);
            else
            {
                ApplyTheme(!IsDark, animate: false);
                PersistTheme();
            }
        }

        private static void AnimateToggle(Window host, Border overlay)
        {
            overlay.IsHitTestVisible = true;
            overlay.Background = (Brush)Application.Current.FindResource("ThemeOverlayBrush");

            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(220))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            fadeIn.Completed += (_, _) =>
            {
                ApplyTheme(!IsDark, animate: false);
                PersistTheme();
                ThemeChanged?.Invoke();

                var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(280))
                {
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
                };
                fadeOut.Completed += (_, _) => overlay.IsHitTestVisible = false;
                overlay.BeginAnimation(UIElement.OpacityProperty, fadeOut);
            };

            overlay.BeginAnimation(UIElement.OpacityProperty, fadeIn);
        }

        public static void ApplyTheme(bool dark, bool animate)
        {
            IsDark = dark;
            SwapThemeDictionary(dark ? DarkUri : LightUri);
            if (!animate)
                ThemeChanged?.Invoke();
        }

        private static void PersistTheme()
        {
            new ProfileRepository().UpdateTheme(IsDark ? "Dark" : "Light");
        }

        private static void SwapThemeDictionary(string uri)
        {
            var app = Application.Current;
            if (app?.Resources == null) return;

            var merged = app.Resources.MergedDictionaries;
            ResourceDictionary existing = null;

            foreach (var dict in merged)
            {
                if (dict.Source?.OriginalString?.Contains("Themes/") == true)
                {
                    existing = dict;
                    break;
                }
            }

            var newDict = new ResourceDictionary
            {
                Source = new Uri(uri, UriKind.Relative)
            };

            if (existing != null)
            {
                var index = merged.IndexOf(existing);
                merged.Remove(existing);
                merged.Insert(index, newDict);
            }
            else
            {
                merged.Insert(0, newDict);
            }
        }
    }
}
