using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace HC.TechnicalCalculators.Src.Security
{
    public class SecureHttpClientFactory
    {
        private readonly ILogger<SecureHttpClientFactory> _logger;
        private readonly SecureNewsFeedOptions _options;

        public SecureHttpClientFactory(
            ILogger<SecureHttpClientFactory> logger,
            IOptions<SecureNewsFeedOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException("Logger Can not be null");
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options), "Options Can not be null");
            }
            _options = options.Value ?? throw new ArgumentNullException("Options Can not be null");
        }

        public HttpClient CreateSecureClient()
        {
            var handler = new HttpClientHandler
            {
                SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
                ServerCertificateCustomValidationCallback = ValidateServerCertificate,
                CheckCertificateRevocationList = _options.ValidateCertificates,
                UseCookies = false // Prevent cookie-based attacks
            };

            var client = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds)
            };

            // Set secure headers
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("User-Agent", "HC-TechnicalCalculators/1.0.0");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");

            // Validate endpoint URL
            if (_options.RequireHttps && !_options.NewsApiEndpoint.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("HTTPS is required but endpoint URL is not secure.");
            }

            return client;
        }

        private bool ValidateServerCertificate(
            HttpRequestMessage request,
            X509Certificate2? certificate,
            X509Chain? chain,
            SslPolicyErrors sslPolicyErrors)
        {
            if (!_options.ValidateCertificates)
            {
                _logger.LogWarning("Certificate validation is disabled. This should only be used in development.");
                return true;
            }

            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }

            _logger.LogError("SSL certificate validation failed: {Errors}", sslPolicyErrors);
            return false;
        }
    }
}
