﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YouTubeLib.Download;

namespace YouTubeLib
{
    public class YouTubeAPI : IDisposable
    {
        private const string YouTubeBaseUrl = "http://www.youtube.com";

        private string tempPath;
        private string executablePath;
        private string outputPath;

        public List<DownloadTask> Downloads { get; set; }

        public YouTubeAPI()
        {
            var stream = GetType().Assembly.GetManifestResourceStream("YouTubeLib.Resources.youtube-dl.exe");
            if (stream == null)
            {
                throw new FileLoadException("Could not find the youtube-dl resource");
            }

            tempPath = Path.Combine(Path.GetTempPath(), "B3");
            executablePath = Path.Combine(tempPath, "youtube-dl.exe");
            outputPath = Path.Combine(tempPath, "output");

            Directory.CreateDirectory(outputPath);

            if (!File.Exists(executablePath))
            {
                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
                File.WriteAllBytes(executablePath, bytes);
            }

            Downloads = new List<DownloadTask>();
        }

        public IEnumerable<Video> Search(string term, int pages)
        {
            var result = new List<Video>();

            Query.Search(term, pages, result);

            return result;
        }

        public DownloadTask CreateDownload(string relativeUrl)
        {
            var processStartInfo = CreateProcess(relativeUrl);
            var download = new DownloadTask(processStartInfo);
            Downloads.Add(download);
            return download;
        }

        internal ProcessStartInfo CreateProcess(string relativeUrl)
        {
            var guid = Guid.NewGuid();

            var arg = "\"" + YouTubeBaseUrl + relativeUrl + "\" -o \"" + outputPath + @"\" + guid + ".%(ext)s\" --newline";

            var oInfo = new ProcessStartInfo(executablePath, arg);
            oInfo.UseShellExecute = false;
            oInfo.CreateNoWindow = true;
            oInfo.RedirectStandardOutput = true;
            oInfo.RedirectStandardError = true;

            return oInfo;
        }

        public void Dispose()
        {
            try
            {
                File.Delete(executablePath);
            }
            catch (Exception)
            {
                // Could not delete file, maybe it is in use
            }
        }
    }
}
