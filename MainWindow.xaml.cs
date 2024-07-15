using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;


namespace AnWallpaper
{
    public partial class MainWindow : Window
    {
        private Gallery galleryInstance;
        private bool windState = false;

        public MainWindow()
        {
            InitializeComponent();
            this.MinHeight = 600;
            this.MinWidth = 800;

            taskbarIcon.TrayMouseDoubleClick += NotifyIcon_DoubleClick;
            NavigateToGalleryStart();
        }

        /* Window Actions */
        private void TitleBarActions(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                MoveWindow();
            }
            else if (e.ButtonState == MouseButtonState.Released)
            {
                MaximizeWindow(sender, e);
            }
        }

        private void MinimizeWindow(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeWindow(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                Resizer.Kind = MaterialDesignThemes.Wpf.PackIconKind.WindowMaximize;
                WindowState = WindowState.Normal;
                windState = true;
            }
            else
            {
                Resizer.Kind = MaterialDesignThemes.Wpf.PackIconKind.WindowRestore;
                WindowState = WindowState.Maximized;
                windState = false;
            }
        }
        
        private void CloseWindow(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void MoveWindow()
        {
            if (WindowState == WindowState.Maximized)
            {
                Point mousePosition = Mouse.GetPosition(this);

                Left = mousePosition.X - (Width / 2);
                Top = mousePosition.Y - (50 / 2);

                ReleaseCapture();
                SendMessage(new WindowInteropHelper(this).Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
            else
            {
                ReleaseCapture();
                SendMessage(new WindowInteropHelper(this).Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }


        /* Notify Icon Actions */
        private void NotifyIcon_Menu_Open_Click(object sender, RoutedEventArgs e)
        {
            RestoreWindow();
        }
        
        private void NotifyIcon_Menu_Close_Click(object sender, RoutedEventArgs e)
        {
            CloseAll();
        }
        
        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            if (WindowState == WindowState.Minimized)
            {
                Hide();
            }
        }
        
        private void NotifyIcon_DoubleClick(object sender, RoutedEventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        }
        
        private void RestoreWindow()
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
        }
        
        private void CloseAll()
        {
            galleryInstance.closeWallpaperWindow();
            taskbarIcon.Dispose();
            Close();
        }

        /* Gallery Actions */
        private void NavigateToGalleryStart()
        {
            galleryInstance = new Gallery();
            MainFrame.Navigate(galleryInstance);
        }
        
        private void NavigateToGallery(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(galleryInstance);
        }

        private void AddCard(object sender, RoutedEventArgs e)
        {
            galleryInstance.NewCard();
        }

        /* Updates Actions */
        private void NavigateToUpdates(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Updates());
        }


        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

    }
}