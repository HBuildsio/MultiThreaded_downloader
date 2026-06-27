using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;

class NewDownloader
{
    private readonly string url;
    private readonly string outputPath;
    private readonly int threadCount;
    private readonly List<string> tempFiles = new();
    private static readonly HttpClient httpClient = new();
    private readonly string fileName;
    private readonly string fileExtension;

    public NewDownloader(string url, string outputPath, int threadCount)
    {
        this.url = url;
        this.outputPath = outputPath;
        this.threadCount = threadCount;
        (this.fileName, this.fileExtension) = ExtractFileInfo(url);
    }

    private (string fileName, string extension) ExtractFileInfo(string url)
    {
        var uri = new Uri(url);
        var path = Uri.UnescapeDataString(uri.AbsolutePath);
        return (
            Path.GetFileNameWithoutExtension(path),
            Path.GetExtension(path)
        );
    }

    public string GetFileName() => fileName;
    public string GetFileExtension() => fileExtension;
    public string GetFullFileName() => SanitizeFileName($"{fileName}{fileExtension}");

    private string SanitizeFileName(string fileName)
    {
        return string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
    }

    public async Task<TimeSpan> DownloadFileAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        using var response = await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, url), cancellationToken);
        var fileSize = response.Content.Headers.ContentLength ??
            throw new InvalidOperationException("Content length not available");

        var tasks = new List<Task>();
        var chunkSize = fileSize / threadCount;
        for (int i = 0; i < threadCount; i++)
        {
            var start = i * chunkSize;
            var end = (i == threadCount - 1) ? fileSize - 1 : start + chunkSize - 1;
            var tempFile = $"{outputPath}.part{i}";
            tempFiles.Add(tempFile);
            tasks.Add(DownloadChunkAsync(start, end, tempFile, cancellationToken));
        }

        await Task.WhenAll(tasks);
        await MergeFilesAsync(cancellationToken);

        stopwatch.Stop();
        return stopwatch.Elapsed;
    }

    private async Task DownloadChunkAsync(long start, long end, string tempFile, CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(start, end);

            using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);

            await stream.CopyToAsync(fileStream, cancellationToken);
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine($"Chunk {start}-{end} download was canceled.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading chunk {start}-{end}: {ex.Message}");
        }
    }


    private async Task MergeFilesAsync(CancellationToken cancellationToken)
    {
        var finalPath = Path.Combine(outputPath, GetFullFileName());
        await using var output = new FileStream(finalPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
        foreach (var tempFile in tempFiles)
        {
            await using (var input = new FileStream(tempFile, FileMode.Open))
            {
                await input.CopyToAsync(output, cancellationToken);
            }
            File.Delete(tempFile);
        }
    }
}
