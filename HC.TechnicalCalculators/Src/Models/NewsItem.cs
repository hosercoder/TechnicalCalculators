using System.ComponentModel.DataAnnotations;

namespace HC.TechnicalCalculators.Src.Models
{
    /// <summary>
    /// Represents a news item with sentiment analysis for financial markets
    /// </summary>
    public class NewsItem
    {
        /// <summary>
        /// Unique identifier for the news item
        /// </summary>
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Title of the news article
        /// </summary>
        [Required]
        [StringLength(500, MinimumLength = 1)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Summary or excerpt of the news article
        /// </summary>
        [StringLength(2000)]
        public string? Summary { get; set; }

        /// <summary>
        /// Full URL to the original article
        /// </summary>
        [Url]
        public string? Url { get; set; }

        /// <summary>
        /// Source of the news (e.g., "Reuters", "Bloomberg")
        /// </summary>
        [StringLength(100)]
        public string? Source { get; set; }

        /// <summary>
        /// Author of the article
        /// </summary>
        [StringLength(200)]
        public string? Author { get; set; }

        /// <summary>
        /// Publication date and time in UTC
        /// </summary>
        [Required]
        public DateTime PublishedAt { get; set; }

        /// <summary>
        /// Sentiment score ranging from -1 (very negative) to +1 (very positive)
        /// </summary>
        [Range(-1.0, 1.0)]
        public double SentimentScore { get; set; }

        /// <summary>
        /// Confidence level of the sentiment analysis (0 to 1)
        /// </summary>
        [Range(0.0, 1.0)]
        public double SentimentConfidence { get; set; }

        /// <summary>
        /// List of financial symbols mentioned in the article
        /// </summary>
        public List<string>? RelatedSymbols { get; set; }

        /// <summary>
        /// Keywords or tags associated with the article
        /// </summary>
        public List<string>? Keywords { get; set; }

        /// <summary>
        /// Category of the news (e.g., "earnings", "mergers", "regulatory")
        /// </summary>
        [StringLength(50)]
        public string? Category { get; set; }

        /// <summary>
        /// Impact level on the market (Low, Medium, High)
        /// </summary>
        [StringLength(20)]
        public string? ImpactLevel { get; set; }

        /// <summary>
        /// Language of the article (ISO 639-1 code)
        /// </summary>
        [StringLength(5)]
        public string Language { get; set; } = "en";

        /// <summary>
        /// Validates the news item data integrity
        /// </summary>
        /// <returns>True if the news item is valid</returns>
        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(Id) || Id.Length > 100) return false;
            if (string.IsNullOrWhiteSpace(Title) || Title.Length > 500) return false;
            if (Summary?.Length > 2000) return false;
            if (SentimentScore < -1 || SentimentScore > 1) return false;
            if (SentimentConfidence < 0 || SentimentConfidence > 1) return false;
            if (PublishedAt > DateTime.UtcNow.AddHours(1)) return false; // Allow slight future tolerance
            if (PublishedAt < DateTime.UtcNow.AddYears(-10)) return false; // Reasonable historical limit

            // Validate URL format if provided
            if (!string.IsNullOrWhiteSpace(Url) && !Uri.TryCreate(Url, UriKind.Absolute, out _))
                return false;

            // Validate symbols format if provided
            if (RelatedSymbols?.Any(s => string.IsNullOrWhiteSpace(s) || s.Length > 10) == true)
                return false;

            return true;
        }

        /// <summary>
        /// Creates a sanitized copy of the news item for safe processing
        /// </summary>
        /// <returns>Sanitized copy of the news item</returns>
        public NewsItem Sanitize()
        {
            return new NewsItem
            {
                Id = SanitizeString(Id, 100),
                Title = SanitizeString(Title, 500),
                Summary = SanitizeString(Summary, 2000),
                Url = SanitizeUrl(Url),
                Source = SanitizeString(Source, 100),
                Author = SanitizeString(Author, 200),
                PublishedAt = PublishedAt,
                SentimentScore = Math.Max(-1, Math.Min(1, SentimentScore)),
                SentimentConfidence = Math.Max(0, Math.Min(1, SentimentConfidence)),
                RelatedSymbols = RelatedSymbols?.Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => SanitizeString(s, 10))
                    .Where(s => !string.IsNullOrEmpty(s))
                    .Take(20) // Limit number of symbols
                    .ToList(),
                Keywords = Keywords?.Where(k => !string.IsNullOrWhiteSpace(k))
                    .Select(k => SanitizeString(k, 50))
                    .Where(k => !string.IsNullOrEmpty(k))
                    .Take(50) // Limit number of keywords
                    .ToList(),
                Category = SanitizeString(Category, 50),
                ImpactLevel = SanitizeString(ImpactLevel, 20),
                Language = SanitizeString(Language ?? "en", 5)
            };
        }

        private static string SanitizeString(string? input, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Remove control characters and normalize whitespace
            var sanitized = new string(input.Where(c => !char.IsControl(c) || char.IsWhiteSpace(c)).ToArray());
            sanitized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"\s+", " ").Trim();

            return sanitized.Length > maxLength ? sanitized.Substring(0, maxLength).TrimEnd() : sanitized;
        }

        private static string? SanitizeUrl(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return null;

            // Basic URL validation and sanitization
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri) && 
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                return uri.ToString();
            }

            return null;
        }
    }
}
