using HC.TechnicalCalculators.Src.Interfaces;
using HC.TechnicalCalculators.Src.Models;
using HC.TechnicalCalculators.Src.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Net.Http.Json;

namespace HC.TechnicalCalculators.Src.Services
{
    /// <summary>
    /// NewsFeed service implementation with enhanced security, validation, and rate limiting
    /// CURRENTLY NOT IN USE - Placeholder for news feed integration
    /// </summary>
    public class NewsFeedService : INewsFeedService
    {
        private readonly SecureHttpClientFactory _httpClientFactory;
        private readonly ILogger<NewsFeedService> _logger;
        private readonly ISecureDataService _secureDataService;
        private readonly IInputValidationService _validationService;
        private readonly SecureNewsFeedOptions _options;
        private readonly ConcurrentDictionary<string, List<NewsItem>> _newsCache = new();
        private readonly ConcurrentDictionary<string, DateTime> _rateLimitTracker = new();
        private DateTime _lastRefresh = DateTime.MinValue;
        private readonly object _refreshLock = new object();

        public NewsFeedService(
            SecureHttpClientFactory httpClientFactory,
            IOptions<SecureNewsFeedOptions> options,
            ILogger<NewsFeedService> logger,
            ISecureDataService secureDataService,
            IInputValidationService validationService)
        {
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
            _logger = logger;
            _secureDataService = secureDataService;
            _validationService = validationService;
        }

        public Task<List<NewsItem>> GetNewsForSymbol(string symbol, DateTime from, DateTime to)
        {
            // Validate inputs
            if (!_validationService.IsValidSymbol(symbol))
            {
                throw new ArgumentException($"Invalid symbol format: {symbol}");
            }

            if (from > to)
            {
                throw new ArgumentException("From date cannot be later than to date");
            }

            if (to > DateTime.UtcNow.AddDays(1))
            {
                throw new ArgumentException("To date cannot be in the future");
            }

            // Rate limiting check
            if (!CheckRateLimit(symbol))
            {
                _logger.LogWarning("Rate limit exceeded for symbol: {Symbol}", symbol);
                throw new InvalidOperationException("Rate limit exceeded. Please try again later.");
            }

            // Check if cache refresh is needed
            lock (_refreshLock)
            {
                if (DateTime.UtcNow - _lastRefresh > TimeSpan.FromMinutes(_options.CacheTimeMinutes))
                {
                    try
                    {
                        RefreshNewsData().Wait();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to refresh news data for symbol: {Symbol}", symbol);
                        // Continue with cached data if available
                    }
                }
            }

            // Return cached news for the symbol and timeframe
            if (_newsCache.TryGetValue(symbol, out var news))
            {
                return Task.FromResult(news.Where(n => n.PublishedAt >= from && n.PublishedAt <= to).ToList());
            }

            return Task.FromResult(new List<NewsItem>());
        }

        public double GetSentimentScore(string symbol, DateTime from, DateTime to)
        {
            // Validate inputs
            if (!_validationService.IsValidSymbol(symbol))
            {
                _logger.LogWarning("Invalid symbol format for sentiment: {Symbol}", symbol);
                return 0;
            }

            if (from > to)
            {
                _logger.LogWarning("Invalid date range for sentiment: from {From} to {To}", from, to);
                return 0;
            }

            // Get news for the specified symbol and timeframe
            var news = _newsCache.TryGetValue(symbol, out var newsItems)
                ? newsItems.Where(n => n.PublishedAt >= from && n.PublishedAt <= to).ToList()
                : new List<NewsItem>();

            if (news.Count == 0)
                return 0; // Neutral if no news

            // Calculate weighted average sentiment (more recent news has higher weight)
            double totalWeight = 0;
            double weightedSentiment = 0;

            foreach (var item in news)
            {
                // Validate sentiment score range
                if (item.SentimentScore < -1 || item.SentimentScore > 1)
                {
                    _logger.LogWarning("Invalid sentiment score {Score} for news item {Id}", item.SentimentScore, item.Id);
                    continue;
                }

                // Calculate weight based on recency (exponential decay)
                double hoursAgo = Math.Max(0, (to - item.PublishedAt).TotalHours);
                double weight = Math.Exp(-0.1 * hoursAgo); // Higher weight for more recent news

                weightedSentiment += item.SentimentScore * weight;
                totalWeight += weight;
            }

            return totalWeight > 0 ? weightedSentiment / totalWeight : 0;
        }

        public async Task RefreshNewsData()
        {
            try
            {
                // Validate configuration
                if (!_validationService.IsValidUrl(_options.NewsApiEndpoint))
                {
                    throw new InvalidOperationException("Invalid news API endpoint configuration");
                }

                using var client = _httpClientFactory.CreateSecureClient();

                // Add protected API key
                var protectedApiKey = _secureDataService.UnprotectString(_options.ApiKey);
                client.DefaultRequestHeaders.Add("x-api-key", protectedApiKey);

                // Secure API call with size limit
                var response = await client.GetFromJsonAsync<NewsApiResponse>(
                    $"{_options.NewsApiEndpoint}/market-news");

                if (response?.Articles == null)
                {
                    _logger.LogWarning("No news data received from API");
                    return;
                }

                // Validate response size
                if (response.Articles.Count > 10000) // Reasonable limit
                {
                    _logger.LogWarning("Response contains too many articles: {Count}", response.Articles.Count);
                    response.Articles = response.Articles.Take(10000).ToList();
                }

                // Process and organize by symbol with validation
                _newsCache.Clear();

                foreach (var article in response.Articles)
                {
                    if (!ValidateNewsItem(article))
                    {
                        _logger.LogWarning("Invalid news item received: {Id}", article.Id);
                        continue;
                    }

                    foreach (var symbol in article.RelatedSymbols ?? new List<string>())
                    {
                        if (!_validationService.IsValidSymbol(symbol))
                        {
                            _logger.LogWarning("Invalid symbol in news item: {Symbol}", symbol);
                            continue;
                        }

                        if (!_newsCache.ContainsKey(symbol))
                        {
                            _newsCache[symbol] = new List<NewsItem>();
                        }

                        _newsCache[symbol].Add(article);
                    }
                }

                _lastRefresh = DateTime.UtcNow;
                _logger.LogInformation("News data refreshed successfully. {Count} articles loaded.", response.Articles.Count);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error refreshing news data");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing news data");
                throw;
            }
        }

        private bool CheckRateLimit(string symbol)
        {
            var now = DateTime.UtcNow;
            var key = $"{symbol}_{now:yyyy-MM-dd-HH-mm-ss}";

            // Clean old entries (older than 1 second)
            var expiredKeys = _rateLimitTracker
                .Where(kvp => now - kvp.Value > TimeSpan.FromSeconds(1))
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var expiredKey in expiredKeys)
            {
                _rateLimitTracker.TryRemove(expiredKey, out _);
            }

            // Count requests in the current second
            var recentRequests = _rateLimitTracker.Count(kvp =>
                kvp.Key.StartsWith($"{symbol}_") &&
                now - kvp.Value <= TimeSpan.FromSeconds(1));

            if (recentRequests >= _options.RateLimitPerSecond)
            {
                return false;
            }

            _rateLimitTracker[key] = now;
            return true;
        }

        private bool ValidateNewsItem(NewsItem item)
        {
            if (item == null) return false;
            if (string.IsNullOrWhiteSpace(item.Id)) return false;
            if (string.IsNullOrWhiteSpace(item.Title)) return false;
            if (item.Title.Length > 500) return false; // Reasonable title length
            if (item.Summary?.Length > 2000) return false; // Reasonable summary length
            if (item.SentimentScore < -1 || item.SentimentScore > 1) return false;
            if (item.PublishedAt > DateTime.UtcNow.AddHours(1)) return false; // Not too far in future
            if (item.PublishedAt < DateTime.UtcNow.AddYears(-5)) return false; // Not too old

            return true;
        }

        private class NewsApiResponse
        {
            public List<NewsItem> Articles { get; set; } = new();
            public int TotalResults { get; set; }
        }
    }
}
