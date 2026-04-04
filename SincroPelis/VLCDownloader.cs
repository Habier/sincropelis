using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SincroPelis
{
    public class VLCDownloader
    {
        private static string? _cachedVersion;

        private static readonly string[] SystemVLCLocations = new[]
        {
            @"C:\Program Files\VideoLAN\VLC",
            @"C:\Program Files (x86)\VideoLAN\VLC",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VLC")
        };

        public static string GetVLCLibPath()
        {
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(appDir, "libvlc", "win-x64");
        }

        public static bool IsVLCAvailable()
        {
            string libPath = GetVLCLibPath();
            if (Directory.Exists(libPath))
            {
                string libvlcDll = Path.Combine(libPath, "libvlc.dll");
                if (File.Exists(libvlcDll))
                    return true;
            }

            foreach (var vlcPath in SystemVLCLocations)
            {
                if (Directory.Exists(vlcPath))
                {
                    string dllPath = Path.Combine(vlcPath, "libvlc.dll");
                    if (File.Exists(dllPath))
                        return true;
                }
            }

            return false;
        }

        public static string? GetSystemVLCLibPath()
        {
            foreach (var vlcPath in SystemVLCLocations)
            {
                if (Directory.Exists(vlcPath))
                {
                    string dllPath = Path.Combine(vlcPath, "libvlc.dll");
                    if (File.Exists(dllPath))
                        return vlcPath;
                }
            }
            return null;
        }

        private static async Task<string> GetLatestVLCVersionAsync()
        {
            if (_cachedVersion != null)
                return _cachedVersion;

            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(30);
                
                var html = await client.GetStringAsync("https://get.videolan.org/vlc/");
                var match = Regex.Match(html, @"vlc/(\d+\.\d+\.\d+)/win64/", RegexOptions.IgnoreCase);
                
                if (match.Success)
                {
                    _cachedVersion = match.Groups[1].Value;
                    return _cachedVersion;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error obteniendo versión de VLC", ex);
            }

            _cachedVersion = "3.0.21";
            return _cachedVersion;
        }

        public static async Task<bool> DownloadAndExtractAsync(IProgress<string>? progress = null)
        {
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            string vlcLibPath = GetVLCLibPath();
            string vlcVersion = await GetLatestVLCVersionAsync();

            string tempDir = Path.Combine(Path.GetTempPath(), $"vlc_install_{Guid.NewGuid():N}");
            string tempZip = Path.Combine(tempDir, "vlc.zip");

            try
            {
                Directory.CreateDirectory(tempDir);

                progress?.Report("Descargando VLC...");

                string downloadUrl = $"https://get.videolan.org/vlc/{vlcVersion}/win64/vlc-{vlcVersion}-win64.zip";
                
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromMinutes(10);

                var bytes = await client.GetByteArrayAsync(downloadUrl);
                await File.WriteAllBytesAsync(tempZip, bytes);

                progress?.Report("Extrayendo VLC...");

                if (Directory.Exists(vlcLibPath))
                    Directory.Delete(vlcLibPath, true);

                Directory.CreateDirectory(vlcLibPath);

                ZipFile.ExtractToDirectory(tempZip, tempDir, true);

                var dirs = Directory.GetDirectories(tempDir);
                Logger.Debug($"Directories in temp: {dirs.Length}");
                foreach (var d in dirs)
                {
                    Logger.Debug($"  Dir: {Path.GetFileName(d)}");
                }

                string? vlcFolder = null;
                foreach (var d in dirs)
                {
                    var name = Path.GetFileName(d);
                    if (name.StartsWith("vlc-"))
                    {
                        vlcFolder = d;
                        break;
                    }
                }

                if (vlcFolder == null)
                {
                    var files = Directory.GetFiles(tempDir);
                    Logger.Debug($"Files in temp: {files.Length}");
                    foreach (var f in files.Take(10))
                    {
                        Logger.Debug($"  File: {Path.GetFileName(f)}");
                    }
                }

                if (vlcFolder != null)
                {
                    foreach (var file in Directory.GetFiles(vlcFolder))
                    {
                        var destFile = Path.Combine(vlcLibPath, Path.GetFileName(file));
                        File.Move(file, destFile);
                    }

                    foreach (var dir in Directory.GetDirectories(vlcFolder))
                    {
                        var destDir = Path.Combine(vlcLibPath, Path.GetFileName(dir));
                        Directory.Move(dir, destDir);
                    }
                }

                var libvlcDll = Path.Combine(vlcLibPath, "libvlc.dll");
                Logger.Debug($"libvlc.dll exists: {File.Exists(libvlcDll)}");

                progress?.Report("VLC instalado correctamente.");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Error descargando VLC", ex);
                progress?.Report($"Error: {ex.Message}");
                return false;
            }
            finally
            {
                try 
                { 
                    if (Directory.Exists(tempDir)) 
                        Directory.Delete(tempDir, true); 
                } 
                catch { }
            }
        }
    }
}