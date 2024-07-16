using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Windows;

namespace AnWallpaper
{
    internal class Utilities
    {
        private static string path_route = Environment.ExpandEnvironmentVariables(@"%LocalAppData%\AnWallpaper");
        private static string wallpaperCacheFile = Path.Combine(path_route, @"temp\cache\wallpaperCache.json");

        public class wallpaperUtils
        {
            private systemUtils systemUtils = new Utilities.systemUtils();

            public void ClearWallpaperCache()
            {
                try
                {
                    if (File.Exists(wallpaperCacheFile))
                    {
                        File.Delete(wallpaperCacheFile);
                    }
                    else
                    {
                        systemUtils.EnsureDirectoryExists(wallpaperCacheFile);
                    }
                }
                catch (Exception ex)
                {
                    // Console.WriteLine($"Error to delete data from wallpaperCache.json: {ex.Message}");
                }
            }
        }
        public class systemUtils
        {
            private const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            private static readonly Random random = new Random();

            public string CopyFile(string sourceFilePath, string destinationFolder, string newFileName)
            {
                string destinationFilePath = "";
                try
                {
                    string srcPath = new Uri(sourceFilePath).ToString().Replace("file:///", "");
                    string destPath = new Uri(destinationFolder).ToString().Replace("file:///", "");

                    destinationFilePath = Path.Combine(destinationFolder, newFileName);

                    File.Copy(sourceFilePath, destinationFilePath, true);

                    Console.WriteLine($"Archivo copiado exitosamente de {sourceFilePath} a {destinationFilePath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al copiar el archivo: {ex.Message}");
                }
                return destinationFilePath;
            }

            public void EnsureDirectoryExists(string path)
            {
                if (Path.HasExtension(path))
                {
                    string directoryPath = Path.GetDirectoryName(path);
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    if (!File.Exists(path))
                    {
                        File.Create(path).Dispose();
                    }
                }
                else
                {
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                }
            }

            public string GenerateRandomName(int length)
            {
                StringBuilder sb = new StringBuilder(length);
                for (int i = 0; i < length; i++)
                {
                    sb.Append(chars[random.Next(chars.Length)]);
                }
                return sb.ToString();
            }
            
            public string GetCurrentWallpaper()
            {
                string currentWallpaperPath = "";
                
                try
                {
                    if (File.Exists(wallpaperCacheFile))
                    {
                        string json = File.ReadAllText(wallpaperCacheFile);
                        var wallpaperData = JsonConvert.DeserializeObject<dynamic>(json);
                        if (wallpaperData != null && json.Length > 2)
                        {
                            currentWallpaperPath = wallpaperData.CurrentWallpaper;
                        }
                    }
                    else
                    {
                        EnsureDirectoryExists(wallpaperCacheFile);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Utils 1 "+ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                return currentWallpaperPath;
            }
        }
    }
}
