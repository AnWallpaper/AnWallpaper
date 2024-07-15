using System.Collections.ObjectModel;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;


namespace AnWallpaper
{
    public partial class Updates : Page
    {
        public ObservableCollection<ReleaseInfo> Releases { get; set; } = new ObservableCollection<ReleaseInfo>();

        public Updates()
        {
            InitializeComponent();
            LoadReleasesAsync();
        }

        private async void LoadReleasesAsync()
        {
            ShowLoadingIndicator();

            string owner = "AnWallpaper";
            string repo = "AnWallpaper";
            string apiUrl = $"https://api.github.com/repos/{owner}/{repo}/releases";

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "C# HttpClient");
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var releases = JsonConvert.DeserializeObject<ReleaseInfo[]>(responseBody);

                    foreach (var release in releases)
                    {
                        Releases.Add(release);
                    }

                    RenderAllReleasesMarkdown();
                }
                else
                {
                    MessageBox.Show($"Error al consultar la API de GitHub: {response.StatusCode}");
                }
            }

            HideLoadingIndicator();
        }

        private void RenderAllReleasesMarkdown()
        {
            var markdownView = new Markdig.Wpf.MarkdownViewer();
            string combinedMarkdown = "";

            foreach (var release in Releases)
            {
                combinedMarkdown += $"# {release.Name}\n";
                combinedMarkdown += $"{release.TagName}";
                combinedMarkdown += $"Published at: {release.PublishedAt}\n\n";
                combinedMarkdown += $"{release.Body}\n\n";
                combinedMarkdown += "---\n\n";
            }

            markdownView.Markdown = combinedMarkdown;
            markdownContent.Content = markdownView;
        }

        private void ShowLoadingIndicator()
        {
            progressBar.Visibility = Visibility.Visible;
            markdownContent.Visibility = Visibility.Hidden;
        }

        private void HideLoadingIndicator()
        {
            progressBar.Visibility = Visibility.Hidden;
            markdownContent.Visibility = Visibility.Visible;
        }
    }

    public class ReleaseInfo
    {
        public string Name { get; set; }
        public string TagName { get; set; }
        public DateTime PublishedAt { get; set; }
        public string Body { get; set; }
    }
}
