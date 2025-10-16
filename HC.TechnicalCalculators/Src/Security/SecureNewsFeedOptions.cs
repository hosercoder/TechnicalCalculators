using System.ComponentModel.DataAnnotations;

namespace HC.TechnicalCalculators.Src.Security
{
    /// <summary>
    /// Configuration options for secure news feed service
    /// </summary>
    public class SecureNewsFeedOptions
    {
        /// <summary>
        /// Encrypted API key for news service authentication
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// The news API endpoint URL
        /// </summary>
        [Required]
        [Url]
        public string NewsApiEndpoint { get; set; } = string.Empty;

        /// <summary>
        /// Cache expiration time in minutes
        /// </summary>
        [Range(1, 1440)] // 1 minute to 24 hours
        public int CacheTimeMinutes { get; set; } = 30;

        /// <summary>
        /// HTTP request timeout in seconds
        /// </summary>
        [Range(1, 300)] // 1 to 300 seconds
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Maximum requests per second per symbol (rate limiting)
        /// </summary>
        [Range(1, 10)] // 1 to 10 requests per second
        public int RateLimitPerSecond { get; set; } = 5;

        /// <summary>
        /// Maximum number of retry attempts for failed requests
        /// </summary>
        [Range(0, 10)]
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// Maximum response size in bytes
        /// </summary>
        [Range(1, 1000000)] // Max 1MB
        public int MaxResponseSizeBytes { get; set; } = 1000000;

        /// <summary>
        /// Require HTTPS for all API calls
        /// </summary>
        public bool RequireHttps { get; set; } = true;

        /// <summary>
        /// Validate SSL certificates
        /// </summary>
        public bool ValidateCertificates { get; set; } = true;

        /// <summary>
        /// Custom user agent string for API requests
        /// </summary>
        [StringLength(200)]
        public string UserAgent { get; set; } = "HC.TechnicalCalculators/1.0";

        /// <summary>
        /// Validates the configuration options
        /// </summary>
        /// <returns>True if configuration is valid</returns>
        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(NewsApiEndpoint) || !Uri.TryCreate(NewsApiEndpoint, UriKind.Absolute, out _))
                return false;

            if (string.IsNullOrWhiteSpace(ApiKey))
                return false;

            if (CacheTimeMinutes < 1 || CacheTimeMinutes > 1440)
                return false;

            if (RateLimitPerSecond < 1 || RateLimitPerSecond > 10)
                return false;

            if (TimeoutSeconds < 1 || TimeoutSeconds > 300)
                return false;

            if (MaxRetries < 0 || MaxRetries > 10)
                return false;

            return true;
        }
    }
}
