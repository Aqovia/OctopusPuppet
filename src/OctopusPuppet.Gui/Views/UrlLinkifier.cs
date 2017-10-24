using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace OctopusPuppet.Gui.Views
{
    //Class copied from: https://stackoverflow.com/questions/861409/wpf-making-hyperlinks-clickable
    public static class UrlLinkifier
    {
        //Simplified the regex, but could possible use http://flanders.co.nz/2009/11/08/a-good-url-regular-expression-repost/
        private static readonly Regex UrlRegex = new Regex(@"(http[s]?|ftp):[^\s]*");

        public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached(
            "Text",
            typeof(string),
            typeof(UrlLinkifier),
            new PropertyMetadata(null, OnTextChanged)
        );

        public static string GetText(DependencyObject d)
        { return d.GetValue(TextProperty) as string; }

        public static void SetText(DependencyObject d, string value)
        { d.SetValue(TextProperty, value); }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textBlock = d as TextBlock;
            if (textBlock == null)
                return;

            textBlock.Inlines.Clear();

            var newText = (string)e.NewValue;
            if (string.IsNullOrEmpty(newText))
                return;

            // Find all URLs using a regular expression
            var lastPosition = 0;
            foreach (Match match in UrlRegex.Matches(newText))
            {
                // Copy raw string from the last position up to the match
                if (match.Index != lastPosition)
                {
                    var rawText = newText.Substring(lastPosition, match.Index - lastPosition);
                    textBlock.Inlines.Add(new Run(rawText));
                }

                // Create a hyperlink for the match
                var link = new Hyperlink(new Run(match.Value))
                {
                    NavigateUri = new Uri(match.Value)
                };

                link.Click += OnUrlClick;

                textBlock.Inlines.Add(link);

                // Update the last matched position
                lastPosition = match.Index + match.Length;
            }

            // Finally, copy the remainder of the string
            if (lastPosition < newText.Length)
            {
                textBlock.Inlines.Add(new Run(newText.Substring(lastPosition)));
            }
        }

        private static void OnUrlClick(object sender, RoutedEventArgs e)
        {
            var link = (Hyperlink)sender;
            Process.Start(link.NavigateUri.ToString());
        }
    }
}
