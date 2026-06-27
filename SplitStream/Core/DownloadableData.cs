using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Security;
using System.IO;

namespace SplitStream
{
    struct DownloadableData
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly string _sourceUrl;
        private readonly long _minData;
        private readonly long _maxData;

        public DownloadableData(string source, long minData, long maxData)
        {
            _sourceUrl = source;
            _minData = minData;
            _maxData = maxData;
        }

        public async Task<HttpResponseMessage> RequestAsync()
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, _sourceUrl);
                request.Headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.110 Safari/537.36");
                request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(_minData, _maxData);

                return await _httpClient.SendAsync(request);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new AccessDeniedException("Unable to access the destination path.", ex);
            }
            catch (SecurityException ex)
            {
                throw new AccessDeniedException("Security settings prevent accessing the destination path.", ex);
            }
        }
    }
}
