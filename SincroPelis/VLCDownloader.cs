using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;

namespace SincroPelis
{
    public class VLCDownloader
    {
        private static readonly string VLCDownloadUrl = "https://get.videolan.org/vlc/3.0.20/win64/vlc-3.0.20-win64.zip";
        private static readonly string VLCVersion = "3.0.20";

        public static string GetVLCLibPath()
        {
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(appDir, "libvlc", "win-x64");
        }

        public static bool IsVLCAvailable()
        {
            string libPath = GetVLCLibPath();
            return Directory.Exists(libPath) && Directory.GetFiles(libPath, "*.dll").Length > 0;
        }

        public static async Task<bool> DownloadAndExtractAsync(IProgress<string>? progress = null)
        {
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            string vlcLibPath = GetVLCLibPath();
            string tempZip = Path.Combine(Path.GetTempPath(), "vlc.zip");

            try
            {
                progress?.Report("Descargando VLC...");

                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromMinutes(10);

                var response = await httpClient.GetAsync(VLCDownloadUrl);
                response.EnsureSuccessStatusCode();

                var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                var downloadedBytes = 0L;

                using var contentStream = await response.Content.ReadAsStreamAsync();
                using var fileStream = new FileStream(tempZip, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

                var buffer = new byte[8192];
                int bytesRead;
                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                    downloadedBytes += bytesRead;

                    if (totalBytes > 0)
                    {
                        var percent = (double)downloadedBytes / totalBytes * 100;
                        progress?.Report($"Descargando VLC... {percent:F0}%");
                    }
                }

                progress?.Report("Extrayendo VLC...");

                if (Directory.Exists(vlcLibPath))
                    Directory.Delete(vlcLibPath, true);

                using var zip = ZipFile.OpenRead(tempZip);
                foreach (var entry in zip.Entries)
                {
                    if (entry.FullName.StartsWith("vlc-3.0.20-win64/") && !entry.FullName.EndsWith("/"))
                    {
                        string relativePath = entry.FullName.Substring("vlc-3.0.20-win64/".Length);
                        string destPath = Path.Combine(appDir, relativePath);

                        string? destDir = Path.GetDirectoryName(destPath);
                        if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
                            Directory.CreateDirectory(destDir);

                        entry.ExtractToFile(destPath, true);
                    }
                }

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
                if (File.Exists(tempZip))
                    File.Delete(tempZip);
            }
        }
    }
}