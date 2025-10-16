using HC.TechnicalCalculators.Src.Models;

namespace HC.TechnicalCalculators.Src.Interfaces
{
    /// <summary>
    /// Composite interface for news feed functionality
    /// Inherits from segregated interfaces to follow ISP while maintaining backward compatibility
    /// </summary>
    public interface INewsFeedService : INewsRetriever, ISentimentAnalyzer, INewsDataCache
    {
        // Interface now inherits from segregated interfaces
        // All methods are defined in the base interfaces
    }

    /// <summary>
    /// Interface for retrieving news data for financial symbols
    /// </summary>
    public interface INewsRetriever
    {
        /// <summary>
        /// Gets news items for a specific symbol within a date range
        /// </summary>
        /// <param name="symbol">Financial symbol (e.g., "AAPL")</param>
        /// <param name="from">Start date for news retrieval</param>
        /// <param name="to">End date for news retrieval</param>
        /// <returns>List of news items for the symbol</returns>
        Task<List<NewsItem>> GetNewsForSymbol(string symbol, DateTime from, DateTime to);
    }

    /// <summary>
    /// Interface for analyzing sentiment from news data
    /// </summary>
    public interface ISentimentAnalyzer
    {
        /// <summary>
        /// Calculates sentiment score for a symbol within a date range
        /// </summary>
        /// <param name="symbol">Financial symbol (e.g., "AAPL")</param>
        /// <param name="from">Start date for sentiment analysis</param>
        /// <param name="to">End date for sentiment analysis</param>
        /// <returns>Sentiment score between -1 (negative) and +1 (positive)</returns>
        double GetSentimentScore(string symbol, DateTime from, DateTime to);
    }

    /// <summary>
    /// Interface for managing news data cache
    /// </summary>
    public interface INewsDataCache
    {
        /// <summary>
        /// Refreshes the news data cache from external sources
        /// </summary>
        /// <returns>Task representing the refresh operation</returns>
        Task RefreshNewsData();
    }
}
