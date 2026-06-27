using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SplitStream
{
    class Downloader
    {
        private volatile bool _allowedToRun;
        private readonly string _sourceUrl;
        private readonly string _destination;
        private readonly int _chunkSize;
        private readonly IProgress<double> _progress;
        private readonly Lazy<long> _contentLength;

        public long BytesWritten { get; private set; }
        public long ContentLength => _contentLength.Value;

        public bool Done => ContentLength == BytesWritten;

        public Downloader(string source, string destination, int chunkSizeInBytes = 10000 /*Default to 0.01 mb*/, IProgress<double> progress = null)
        {
            if (string.IsNullOrEmpty(source))
                throw new ArgumentNullException("source is empty");
            if (string.IsNullOrEmpty(destination))
                throw new ArgumentNullException("destination is empty");

            _allowedToRun = true;
            _sourceUrl = source;
            _destination = destination;
            _chunkSize = chunkSizeInBytes;
            _contentLength = new Lazy<long>(GetContentLength);
            _progress = progress;

            if (!File.Exists(destination))
                BytesWritten = 0;
            else
            {
                try
                {
                    BytesWritten = new FileInfo(destination).Length;
                }
                catch
                {
                    BytesWritten = 0;
                }
            }
        }

        private long GetContentLength()
        {
            var request = (HttpWebRequest)WebRequest.Create(_sourceUrl);
            request.Method = "HEAD";

            using (var response = request.GetResponse())
                return response.ContentLength;
        }

        private async Task Start(long range)
        {
            if (!_allowedToRun)
                throw new InvalidOperationException();

            if (Done)
            {
                Console.WriteLine("File is already downloaded");
                return;
            }
                //file has been found in folder destination and is already fully downloaded 
                

            var request = (HttpWebRequest)WebRequest.Create(_sourceUrl);
            request.Method = "GET";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/95.0.4638.69 Safari/537.36";
            request.AddRange(range);

            using (var response = await request.GetResponseAsync())
            {
                using (var responseStream = response.GetResponseStream())
                {
                    using (var fs = new FileStream(_destination, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    {
                        while (_allowedToRun)
                        {
                            var buffer = new byte[_chunkSize];
                            var bytesRead = await responseStream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                            Console.WriteLine($"Still better than you noob {bytesRead} ");

                            if (bytesRead == 0) break;

                            await fs.WriteAsync(buffer, 0, bytesRead);
                            BytesWritten += bytesRead;
                            _progress?.Report((double)BytesWritten / ContentLength);
                        }

                        await fs.FlushAsync();
                    }
                }
            }
        }

        public Task Start()
        {
            _allowedToRun = true;
            return Start(BytesWritten);
        }

        public void Pause()
        {
            _allowedToRun = false;
        }
    }
}
