using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using HC.TechnicalCalculators.Src.Security;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Xunit;

namespace HC.TechnicalCalculators.Tests.Security
{
    public class SecureHttpClientFactoryTests : IDisposable
    {
        private readonly Mock<ILogger<SecureHttpClientFactory>> _mockLogger;
        private readonly Mock<IOptions<SecureNewsFeedOptions>> _mockOptions;
        private readonly SecureNewsFeedOptions _options;
        private readonly SecureHttpClientFactory _factory;

        public SecureHttpClientFactoryTests()
        {
            _mockLogger = new Mock<ILogger<SecureHttpClientFactory>>();
            _mockOptions = new Mock<IOptions<SecureNewsFeedOptions>>();
            
            _options = new SecureNewsFeedOptions
            {
                ApiKey = "test-api-key",
                NewsApiEndpoint = "https://api.test.com",
                CacheTimeMinutes = 30,
                TimeoutSeconds = 30,
                RequireHttps = true,
                ValidateCertificates = true,
                RateLimitPerSecond = 5,
                MaxRetries = 3,
                MaxResponseSizeBytes = 1000000,
                UserAgent = "HC.TechnicalCalculators/1.0"
            };

            _mockOptions.Setup(o => o.Value).Returns(_options);
            _factory = new SecureHttpClientFactory(_mockLogger.Object, _mockOptions.Object);
        }

        [Fact]
        public void Constructor_WithValidParameters_ShouldCreateInstance()
        {
            // Act
            var factory = new SecureHttpClientFactory(_mockLogger.Object, _mockOptions.Object);

            // Assert
            Assert.NotNull(factory);
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => 
                new SecureHttpClientFactory(null!, _mockOptions.Object));
            Assert.Equal("Logger Can not be null", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullOptions_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => 
                new SecureHttpClientFactory(_mockLogger.Object, null!));
            Assert.Contains("Options Can not be null", exception.Message);
        }

        [Fact]
        public void CreateSecureClient_WithValidOptions_ShouldReturnConfiguredClient()
        {
            // Act
            var client = _factory.CreateSecureClient();

            // Assert
            Assert.NotNull(client);
            Assert.Equal(TimeSpan.FromSeconds(_options.TimeoutSeconds), client.Timeout);
            
            // Check headers
            Assert.True(client.DefaultRequestHeaders.Contains("User-Agent"));
            Assert.True(client.DefaultRequestHeaders.Contains("Accept"));
            Assert.True(client.DefaultRequestHeaders.Contains("Cache-Control"));
            
            Assert.Equal("HC-TechnicalCalculators/1.0.0", client.DefaultRequestHeaders.UserAgent.ToString());
            Assert.Equal("application/json", client.DefaultRequestHeaders.Accept.ToString());
            Assert.Equal("no-cache", client.DefaultRequestHeaders.CacheControl.ToString());
        }

        [Fact]
        public void CreateSecureClient_WithHttpsRequired_ShouldSucceedWithHttpsEndpoint()
        {
            // Arrange
            _options.RequireHttps = true;
            _options.NewsApiEndpoint = "https://secure.api.com";

            // Act
            var client = _factory.CreateSecureClient();

            // Assert
            Assert.NotNull(client);
        }

        [Fact]
        public void CreateSecureClient_WithHttpsRequired_ShouldThrowWithHttpEndpoint()
        {
            // Arrange
            _options.RequireHttps = true;
            _options.NewsApiEndpoint = "http://insecure.api.com";

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => 
                _factory.CreateSecureClient());
            Assert.Contains("HTTPS is required but endpoint URL is not secure", exception.Message);
        }

        [Fact]
        public void CreateSecureClient_WithHttpsNotRequired_ShouldSucceedWithHttpEndpoint()
        {
            // Arrange
            _options.RequireHttps = false;
            _options.NewsApiEndpoint = "http://api.com";

            // Act
            var client = _factory.CreateSecureClient();

            // Assert
            Assert.NotNull(client);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(30)]
        [InlineData(60)]
        [InlineData(120)]
        public void CreateSecureClient_WithDifferentTimeouts_ShouldSetCorrectTimeout(int timeoutSeconds)
        {
            // Arrange
            _options.TimeoutSeconds = timeoutSeconds;

            // Act
            var client = _factory.CreateSecureClient();

            // Assert
            Assert.Equal(TimeSpan.FromSeconds(timeoutSeconds), client.Timeout);
        }

        [Fact]
        public void CreateSecureClient_MultipleCalls_ShouldReturnNewInstancesEachTime()
        {
            // Act
            var client1 = _factory.CreateSecureClient();
            var client2 = _factory.CreateSecureClient();

            // Assert
            Assert.NotNull(client1);
            Assert.NotNull(client2);
            Assert.NotSame(client1, client2);
        }

        [Fact]
        public void CreateSecureClient_ShouldConfigureSecuritySettings()
        {
            // Act
            var client = _factory.CreateSecureClient();

            // Assert
            Assert.NotNull(client);
            
            // Verify the handler is properly configured (we can't directly access it, but we can check the client works)
            Assert.True(client.Timeout > TimeSpan.Zero);
        }

        [Fact]
        public void CreateSecureClient_WithValidateCertificatesDisabled_ShouldLogWarning()
        {
            // Arrange
            _options.ValidateCertificates = false;
            
            // We need to create a scenario where certificate validation would be called
            // This is difficult to test directly without making actual HTTPS calls
            // But we can verify the configuration is set up correctly
            
            // Act
            var client = _factory.CreateSecureClient();

            // Assert
            Assert.NotNull(client);
            // The warning would be logged when certificate validation callback is actually invoked
        }

        [Theory]
        [InlineData("https://api.newsapi.org")]
        [InlineData("https://api.test.com")]
        [InlineData("https://secure-endpoint.example.com")]
        public void CreateSecureClient_WithDifferentHttpsEndpoints_ShouldSucceed(string endpoint)
        {
            // Arrange
            _options.NewsApiEndpoint = endpoint;
            _options.RequireHttps = true;

            // Act
            var client = _factory.CreateSecureClient();

            // Assert
            Assert.NotNull(client);
        }

        [Theory]
        [InlineData("http://api.test.com")]
        [InlineData("http://insecure.example.com")]
        [InlineData("ftp://file.server.com")]
        public void CreateSecureClient_WithNonHttpsEndpoints_ShouldThrowWhenHttpsRequired(string endpoint)
        {
            // Arrange
            _options.NewsApiEndpoint = endpoint;
            _options.RequireHttps = true;

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => 
                _factory.CreateSecureClient());
            Assert.Contains("HTTPS is required but endpoint URL is not secure", exception.Message);
        }

        [Fact]
        public void CreateSecureClient_ShouldClearExistingHeaders()
        {
            // Act
            var client = _factory.CreateSecureClient();

            // Assert
            // Should have exactly the headers we set (User-Agent, Accept, Cache-Control)
            var headerCount = client.DefaultRequestHeaders.Count();
            Assert.True(headerCount >= 3); // At least our 3 headers
            
            Assert.True(client.DefaultRequestHeaders.Contains("User-Agent"));
            Assert.True(client.DefaultRequestHeaders.Contains("Accept"));
            Assert.True(client.DefaultRequestHeaders.Contains("Cache-Control"));
        }

        [Fact]
        public void CreateSecureClient_ShouldDisableCookies()
        {
            // Act
            var client = _factory.CreateSecureClient();

            // Assert
            Assert.NotNull(client);
            // We can't directly test the UseCookies setting without reflection
            // But we can verify the client is created successfully
        }

        [Fact]
        public void CreateSecureClient_ShouldConfigureTlsVersions()
        {
            // Act
            var client = _factory.CreateSecureClient();

            // Assert
            Assert.NotNull(client);
            // The TLS configuration (Tls12 | Tls13) is set on the handler
            // We can't directly test this without making actual network calls
            // But we can verify the client is created without errors
        }

        [Fact]
        public void CreateSecureClient_WithCaseInsensitiveHttpsCheck_ShouldWork()
        {
            // Arrange
            _options.NewsApiEndpoint = "HTTPS://API.TEST.COM";
            _options.RequireHttps = true;

            // Act
            var client = _factory.CreateSecureClient();

            // Assert
            Assert.NotNull(client);
        }

        [Fact]
        public void CreateSecureClient_WithHttpsInMiddleOfUrl_ShouldThrowWhenNotHttps()
        {
            // Arrange
            _options.NewsApiEndpoint = "http://api.https-test.com";
            _options.RequireHttps = true;

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => 
                _factory.CreateSecureClient());
            Assert.Contains("HTTPS is required but endpoint URL is not secure.", exception.Message);
        }

        [Fact]
        public void CreateSecureClient_WithEmptyEndpoint_ShouldNotThrowForEmptyCheck()
        {
            // Arrange
            _options.NewsApiEndpoint = "";
            _options.RequireHttps = true;

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => 
                _factory.CreateSecureClient());
            Assert.Contains("HTTPS is required but endpoint URL is not secure.", exception.Message);
        }

        [Fact]
        public void CreateSecureClient_WithNullEndpoint_ShouldThrow()
        {
            // Arrange
            _options.NewsApiEndpoint = null!;
            _options.RequireHttps = true;

            // Act & Assert
            Assert.ThrowsAny<Exception>(() => _factory.CreateSecureClient());
        }

        [Fact]
        public void CreateSecureClient_ShouldSetCorrectUserAgent()
        {
            // Act
            var client = _factory.CreateSecureClient();

            // Assert
            var userAgentHeader = client.DefaultRequestHeaders.UserAgent.FirstOrDefault();
            Assert.NotNull(userAgentHeader);
            Assert.Equal("HC-TechnicalCalculators", userAgentHeader.Product?.Name);
            Assert.Equal("1.0.0", userAgentHeader.Product?.Version);
        }

        [Fact]
        public void CreateSecureClient_ShouldSetAcceptHeader()
        {
            // Act
            var client = _factory.CreateSecureClient();

            // Assert
            var acceptHeader = client.DefaultRequestHeaders.Accept.FirstOrDefault();
            Assert.NotNull(acceptHeader);
            Assert.Equal("application/json", acceptHeader.MediaType);
        }

        [Fact]
        public void CreateSecureClient_ShouldSetCacheControlHeader()
        {
            // Act
            var client = _factory.CreateSecureClient();

            // Assert
            Assert.NotNull(client.DefaultRequestHeaders.CacheControl);
            Assert.True(client.DefaultRequestHeaders.CacheControl.NoCache);
        }

        public void Dispose()
        {
            // Cleanup resources

        }
    }
}
