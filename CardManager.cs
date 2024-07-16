using System.IO;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Microsoft.Win32;
using Newtonsoft.Json;
using MediaToolkit;
using MediaToolkit.Model;
using MediaToolkit.Options;
using MaterialDesignThemes.Wpf;

namespace AnWallpaper
{
    internal class CardManager
    {
        public static string path_route = Environment.ExpandEnvironmentVariables(@"%LocalAppData%\AnWallpaper");
        private static string CacheFileName = Path.Combine(path_route, @"temp\cache\cardCache.json");
        private Utilities.wallpaperUtils wallpaperUtils = new Utilities.wallpaperUtils();
        private Utilities.systemUtils systemUtils = new Utilities.systemUtils();
        private ResourceDictionary _Resources;
        Utilities Utilities = new Utilities();
        private string current_video_path = "";
        private List<CardInfo> cardsInfo;
        private UniformGrid _CardPanel;
        private string videoPath = "";
        private int cardCount = 0;
        public LiveWallpaper liveWallpaper;
        public UniformGrid CardPanel
        {
            set => _CardPanel = value;
        }
        public ResourceDictionary Resources
        {
            set => _Resources = value;
        }

        public string video_path;

        private void StartWallpaper(string video_path)
        {
            if (!File.Exists(video_path))
            {
                MessageBox.Show($"Video file doesn't exist: {video_path}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            liveWallpaper.Show();
            current_video_path = video_path;
            SetCurrentWallpaper();
            OpenWallpaper(current_video_path, true);
        }

        private void OpenWallpaper(string video_path, bool status = false)
        {
            if (!status)
            {
                liveWallpaper = new LiveWallpaper();
                liveWallpaper.VideoPath = video_path;
                liveWallpaper.setBg();
                liveWallpaper.Show();
            }
            else
            {
                liveWallpaper.VideoPath = video_path;
                liveWallpaper.setBg();
                liveWallpaper.Show();
            }
            
        }


        public void AddCard()
        {
            cardCount++;

            Brush cardImage;

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Video Files |*.mp4";
            if (openFileDialog.ShowDialog() == true)
            {
                videoPath = openFileDialog.FileName;

                var videoInfo = ExtractFirstFrame(videoPath);

                systemUtils.EnsureDirectoryExists(Path.Combine(path_route, @"Resources\wallpapers\"));

                string newFileCopied = systemUtils.CopyFile(videoPath, Path.Combine(path_route, @"Resources\wallpapers\"), videoInfo.Item2 + ".mp4");

                Border card = new Border
                {
                    Width = 200,
                    Height = 150,
                    Margin = new Thickness(10),
                    Background = string.IsNullOrEmpty(videoInfo.Item1) ? Brushes.LightGray : new ImageBrush(new BitmapImage(new Uri(videoInfo.Item1))),
                    CornerRadius = new CornerRadius(8),
                    BorderBrush = Brushes.DarkGray,
                    BorderThickness = new Thickness(1),
                    Style = (Style)_Resources["CardStyle"]
                };

                cardImage = card.Background;

                Grid cardContent = new Grid();

                cardContent.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                TextBlock content = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    FontSize = 16
                };

                cardContent.Children.Add(content);

                Button menuButton = new Button
                {
                    Width = 30,
                    Height = 30,
                    Background = Brushes.Transparent,
                    BorderBrush = Brushes.Transparent,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(0, 0, 5, 5),
                    Content = new PackIcon
                    {
                        Kind = PackIconKind.DotsHorizontal,
                        Foreground = Brushes.White,
                    }
                };

                menuButton.Click += (s, args) =>
                {
                    menuButton.ContextMenu.IsOpen = true;
                };

                ContextMenu contextMenu = new ContextMenu();

                MenuItem aboutItem = new MenuItem { Header = "About", Icon = new PackIcon { Kind = PackIconKind.InformationOutline } };
                MenuItem setAsWallpaperItem = new MenuItem { Header = "Set as wallpaper", Icon = new PackIcon { Kind = PackIconKind.Wallpaper } };
                MenuItem editItem = new MenuItem { Header = "Edit", Icon = new PackIcon { Kind = PackIconKind.Edit } };
                MenuItem deleteItem = new MenuItem { Header = "Delete", Icon = new PackIcon { Kind = PackIconKind.Delete } };

                deleteItem.Click += (s, args) => { RemoveCard(card, videoInfo.Item1); };

                setAsWallpaperItem.Click += (s, args) => { StartWallpaper(newFileCopied); };

                contextMenu.Items.Add(aboutItem);
                contextMenu.Items.Add(setAsWallpaperItem);
                contextMenu.Items.Add(editItem);
                contextMenu.Items.Add(deleteItem);

                menuButton.ContextMenu = contextMenu;

                cardContent.Children.Add(menuButton);

                card.Child = cardContent;

                _CardPanel.Children.Add(card);

                UpdateGridColumns();

                SaveCardsToCache(cardImage);
            }
        }

        private (string, string) ExtractFirstFrame(string videoPath)
        {
            string randomName = systemUtils.GenerateRandomName(5);
            string imagePath = "";

            try
            {
                string tempDir = Path.Combine(path_route, @"temp\wallpapers");
                if (!Directory.Exists(tempDir))
                    Directory.CreateDirectory(tempDir);

                string tempImageFile = Path.Combine(tempDir, randomName+".png");

                var inputFile = new MediaFile { Filename = videoPath };
                var outputFile = new MediaFile { Filename = tempImageFile };

                using (var engine = new Engine())
                {
                    engine.GetMetadata(inputFile);
                    var options = new ConversionOptions { Seek = TimeSpan.FromSeconds(1) };
                    engine.GetThumbnail(inputFile, outputFile, options);
                }

                imagePath = tempImageFile;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al extraer el primer fotograma del video: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return (imagePath, randomName);
        }

        private void UpdateGridColumns()
        {
            int columns = (int)(_CardPanel.ActualWidth / (200 + 10));
            _CardPanel.Columns = columns;
        }

        private Border CreateCardFromInfo(CardInfo cardInfo)
        {
            ImageBrush backgroundBrush = new ImageBrush(new BitmapImage(new Uri(cardInfo.BackgroundImagePath)));

            Border card = new Border
            {
                Width = 200,
                Height = 150,
                Margin = new Thickness(10),
                Background = backgroundBrush,
                CornerRadius = new CornerRadius(8),
                BorderBrush = Brushes.DarkGray,
                BorderThickness = new Thickness(1),
                Style = (Style)_Resources["CardStyle"]
            };

            Grid cardContent = new Grid();

            cardContent.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            TextBlock contentTextBlock = new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 16
            };
            cardContent.Children.Add(contentTextBlock);

            Button menuButton = new Button
            {
                Width = 30,
                Height = 30,
                Background = Brushes.Transparent,
                BorderBrush = Brushes.Transparent,
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 0, 5, 5),
                Content = new PackIcon
                {
                    Kind = PackIconKind.DotsHorizontal,
                    Foreground = Brushes.White,
                }
            };

            menuButton.Click += (s, args) =>
            {
                menuButton.ContextMenu.IsOpen = true;
            };

            string videoInfo = cardInfo.BackgroundImagePath.Replace("temp", "Resources").Replace("file:///", "").Replace("png", "mp4");

            ContextMenu contextMenu = new ContextMenu();

            MenuItem aboutItem = new MenuItem { Header = "About", Icon = new PackIcon { Kind = PackIconKind.InformationOutline } };
            MenuItem setAsWallpaperItem = new MenuItem { Header = "Set as wallpaper", Icon = new PackIcon { Kind = PackIconKind.Wallpaper } };
            MenuItem editItem = new MenuItem { Header = "Edit", Icon = new PackIcon { Kind = PackIconKind.Edit } };
            MenuItem deleteItem = new MenuItem { Header = "Delete", Icon = new PackIcon { Kind = PackIconKind.Delete } };

            deleteItem.Click += (s, args) => { RemoveCard(card, videoInfo); };

            setAsWallpaperItem.Click += (s, args) =>
            {
                string videoFileInCache = videoInfo;
                StartWallpaper(videoFileInCache);
            };

            contextMenu.Items.Add(aboutItem);
            contextMenu.Items.Add(setAsWallpaperItem);
            contextMenu.Items.Add(editItem);
            contextMenu.Items.Add(deleteItem);

            menuButton.ContextMenu = contextMenu;
            cardContent.Children.Add(menuButton);
            card.Child = cardContent;

            return card;
        }


        private void SaveCardsToCache(Brush cardImage = null)
        {
            try
            {
                cardsInfo.Clear();

                foreach (Border card in _CardPanel.Children)
                {
                    Grid cardContent = card.Child as Grid;

                    if (cardContent != null && cardContent.Children.Count > 1)
                    {
                        TextBlock contentTextBlock = cardContent.Children[0] as TextBlock;

                        Button menuButton = cardContent.Children[cardContent.Children.Count - 1] as Button;

                        ImageBrush currentBackground = card.Background as ImageBrush;
                        string backgroundImagePath = currentBackground?.ImageSource.ToString();

                        CardInfo cardInfo = new CardInfo
                        {
                            BackgroundImagePath = backgroundImagePath,
                            MenuItems = new List<string> { "About", "Set as wallpaper", "Edit", "Delete" }
                        };

                        cardsInfo.Add(cardInfo);
                    }
                }

                string json = JsonConvert.SerializeObject(cardsInfo);
                File.WriteAllText(CacheFileName, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving wallpapers in cache: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveCard(Border card, string cardImg)
        {
            string cardImgPath = cardImg.Replace("temp", "Resources");
            cardImgPath = cardImgPath.Replace(".png", ".mp4");
            try
            {
                cardImgPath = cardImgPath.Replace("/", "\u005c");
            }
            finally { }

            if(cardImgPath == current_video_path)
            {
                wallpaperUtils.ClearWallpaperCache();
                liveWallpaper.Hide();
                _CardPanel.Children.Remove(card);
                UpdateGridColumns();
                SaveCardsToCache();
            }
            else
            {
                _CardPanel.Children.Remove(card);
                UpdateGridColumns();
                SaveCardsToCache();
            }
            
        }

        public void LoadCards()
        {
            try
            {
                current_video_path = systemUtils.GetCurrentWallpaper();

                if (File.Exists(CacheFileName))
                {
                    string json = File.ReadAllText(CacheFileName);
                    cardsInfo = JsonConvert.DeserializeObject<List<CardInfo>>(json);

                    foreach (var cardInfo in cardsInfo)
                    {
                        Border card = CreateCardFromInfo(cardInfo);
                        _CardPanel.Children.Add(card);
                    }

                    UpdateGridColumns();
                }
                else
                {
                    cardsInfo = new List<CardInfo>();
                    systemUtils.EnsureDirectoryExists(CacheFileName);
                }

                OpenWallpaper(current_video_path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void SetCurrentWallpaper()
        {
            try
            {
                var currentWallpaper = new
                {
                    CurrentWallpaper = current_video_path
                };

                string json = JsonConvert.SerializeObject(currentWallpaper);

                string wallpaperCacheFile = Path.Combine(path_route, @"temp\cache\wallpaperCache.json");

                File.WriteAllText(wallpaperCacheFile, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private class CardInfo
        {
            public string Content { get; set; }
            public string BackgroundImagePath { get; set; }
            public List<string> MenuItems { get; set; }
        }
    }
}
