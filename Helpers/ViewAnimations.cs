using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace DevSpace.Helpers
{
    public static class ViewAnimations
    {
        public static void FadeIn(FrameworkElement element, double durationMs = 220)
        {
            if (element == null) return;

            element.Opacity = 0;
            var animation = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(durationMs))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            element.BeginAnimation(UIElement.OpacityProperty, animation);
        }

        public static void SlideAndFadeIn(FrameworkElement element, double distance = 20, double durationMs = 300)
        {
            if (element == null) return;

            element.Opacity = 0;
            element.RenderTransform = new TranslateTransform(0, distance);

            var opacityAnim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(durationMs))
            {
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
            };

            var translateAnim = new DoubleAnimation(distance, 0, TimeSpan.FromMilliseconds(durationMs))
            {
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
            };

            element.BeginAnimation(UIElement.OpacityProperty, opacityAnim);
            element.RenderTransform.BeginAnimation(TranslateTransform.YProperty, translateAnim);
        }

        public static void AnimateContentSwap(ContentControl host, UIElement newContent)
        {
            if (host == null) return;

            host.Content = newContent;
            if (newContent is FrameworkElement fe)
                SlideAndFadeIn(fe);
        }
    }
}
