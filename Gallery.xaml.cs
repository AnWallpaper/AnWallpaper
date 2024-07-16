using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AnWallpaper
{
    public partial class Gallery : Page
    {
        public static string path_route = Environment.ExpandEnvironmentVariables(@"%LocalAppData%\AnWallpaper");
        private static string path_bgw = "pack://application:,,,/resources/images/background.jpg";
        private CardManager cardManager;
        private MainWindow _mainWindow;

        public Gallery()
        {
            InitializeComponent();
            setBackground();

            CardManager NewcardManager = new CardManager();
            cardManager = NewcardManager;
            
            cardManager.CardPanel = CardPanel;
            cardManager.Resources = Resources;
            cardManager.LoadCards();
        }

        public void NewCard()
        {
            cardManager.AddCard();
        }

        private void setBackground()
        {
            ImageBrush mainBrush = new ImageBrush();
            mainBrush.ImageSource = new BitmapImage(new Uri(path_bgw, UriKind.Absolute));
            this.WindowPanel.Background = mainBrush;
        }
    
        public void closeWallpaperWindow()
        {
            cardManager.liveWallpaper.Close();
        }
    }
}
