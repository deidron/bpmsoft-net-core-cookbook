namespace BPMSoft.Configuration.EP
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using BPMSoft.Common.Threading;
    using BPMSoft.Core.Factories;
    using global::Common.Logging;

    [DefaultBinding(typeof(IEpHttpClientExample), Name = "Default")]
    public class EpHttpClientExample(IHttpClientFactory httpClientFactory) : IEpHttpClientExample
    {
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

        private readonly ILog _logger = LogManager.GetLogger(nameof(EpHttpClientExample));

        public string Execute(string endpoint) =>
            AsyncPump.Run(() => ExecuteAsync(endpoint));

        public Task<string> ExecuteAsync(string endpoint) =>
            ExecuteAsync(endpoint, CancellationToken.None);

        public async Task<string> ExecuteAsync(string endpoint, CancellationToken cancellationToken)
        {
            try
            {
                HttpClient client = _httpClientFactory.CreateClient();
                using (HttpResponseMessage response = await client.GetAsync(endpoint, cancellationToken))
                {
                    if (response.IsSuccessStatusCode)
#if NET5_0_OR_GREATER
                        return await response.Content.ReadAsStringAsync(cancellationToken);
#else
                        return await response.Content.ReadAsStringAsync();
#endif
                    _logger.Error($"Server returned an error {response.StatusCode} for {endpoint}");
                    return null;
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.Error($"Request to {endpoint} failed", ex);
                return null;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.Error($"Request to {endpoint} timed out");
                return null;
            }
        }
    }
}
