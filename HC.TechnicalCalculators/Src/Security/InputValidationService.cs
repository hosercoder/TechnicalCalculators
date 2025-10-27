using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace HC.TechnicalCalculators.Src.Security
{
    /// <summary>
    /// Interface for validating string and symbol formats
    /// </summary>
    public interface IFormatValidator
    {
        /// <summary>
        /// Validates if a symbol follows the correct format
        /// </summary>
        /// <param name="symbol">Symbol to validate</param>
        /// <returns>True if symbol is valid</returns>
        bool IsValidSymbol(string symbol);

        /// <summary>
        /// Validates if a URL follows the correct format
        /// </summary>
        /// <param name="url">URL to validate</param>
        /// <returns>True if URL is valid</returns>
        bool IsValidUrl(string url);
    }

    /// <summary>
    /// Interface for validating financial price data
    /// </summary>
    public interface IPriceDataValidator
    {
        /// <summary>
        /// Validates price data array structure and values
        /// </summary>
        /// <param name="prices">Price data to validate</param>
        /// <returns>True if price data is valid</returns>
        bool IsValidPriceData(double[,] prices);
    }

    /// <summary>
    /// Interface for validating data size constraints
    /// </summary>
    public interface ISizeValidator
    {
        /// <summary>
        /// Validates that an array doesn't exceed maximum size
        /// </summary>
        /// <param name="array">Array to validate</param>
        /// <param name="maxSize">Maximum allowed size</param>
        /// <exception cref="ArgumentException">Thrown when array exceeds max size</exception>
        void ValidateArraySize(Array array, int maxSize);

        /// <summary>
        /// Validates that a string doesn't exceed maximum length
        /// </summary>
        /// <param name="input">String to validate</param>
        /// <param name="maxLength">Maximum allowed length</param>
        /// <param name="parameterName">Name of the parameter for error messages</param>
        /// <exception cref="ArgumentException">Thrown when string exceeds max length</exception>
        void ValidateStringLength(string input, int maxLength, string parameterName);
    }

    /// <summary>
    /// Composite interface for complete input validation functionality
    /// Inherits from segregated interfaces to follow ISP while maintaining backward compatibility
    /// </summary>
    public interface IInputValidationService : IFormatValidator, IPriceDataValidator, ISizeValidator
    {
        // Interface now inherits from segregated interfaces
        // All methods are defined in the base interfaces
    }

    public class InputValidationService : IInputValidationService
    {
        private readonly ILogger<InputValidationService> _logger;
        private static readonly Regex SymbolRegex = new Regex(@"^[A-Z]{1,10}$", RegexOptions.Compiled);
        private static readonly Regex UrlRegex = new Regex(@"^https?://[^\s/$.?#].[^\s]*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private const int MAX_ARRAY_SIZE = 1000000; // 1M elements
        private const int MAX_STRING_LENGTH = 1000;


        public InputValidationService(ILogger<InputValidationService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException("Logger Must be provided");
        }

        public bool IsValidSymbol(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                _logger.LogWarning("Invalid symbol: null or empty");
                return false;
            }

            if (!SymbolRegex.IsMatch(symbol))
            {
                _logger.LogWarning("Invalid symbol format: {Symbol}", symbol);
                return false;
            }

            return true;
        }

        public bool IsValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                _logger.LogWarning("Invalid URL: null or empty");
                return false;
            }

            if (!UrlRegex.IsMatch(url))
            {
                _logger.LogWarning("Invalid URL format: {Url}", url);
                return false;
            }

            return true;
        }

        public bool IsValidPriceData(double[,] prices)
        {
            if (prices == null)
            {
                _logger.LogWarning("Price data is null");
                return false;
            }

            if (prices.GetLength(1) != 6)
            {
                _logger.LogWarning("Price data must have 6 columns, found {Columns}", prices.GetLength(1));
                return false;
            }

            var rows = prices.GetLength(0);
            if (rows == 0)
            {
                _logger.LogWarning("Price data is empty");
                return false;
            }

            if (rows > MAX_ARRAY_SIZE)
            {
                _logger.LogWarning("Price data too large: {Rows} rows", rows);
                return false;
            }

            // Validate price values
            for (int i = 0; i < rows; i++)
            {
                for (int j = 1; j < 6; j++) // Skip timestamp column (0)
                {
                    var value = prices[i, j];
                    if (double.IsNaN(value) || double.IsInfinity(value))
                    {
                        _logger.LogWarning("Invalid price value at [{Row},{Column}]: {Value}", i, j, value);
                        return false;
                    }

                    if (j == 5 && value < 0) // Volume column
                    {
                        _logger.LogWarning("Negative volume at [{Row}]: {Value}", i, value);
                        return false;
                    }
                }
            }

            return true;
        }

        public void ValidateArraySize(Array array, int maxSize)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            if (array.Length > maxSize)
            {
                _logger.LogError("Array size {Size} exceeds maximum allowed size {MaxSize}", array.Length, maxSize);
                throw new ArgumentException($"Array size {array.Length} exceeds maximum allowed size {maxSize}");
            }
        }

        public void ValidateStringLength(string input, int maxLength, string parameterName)
        {
            if (input != null && input.Length > maxLength)
            {
                _logger.LogError("String parameter {Parameter} length {Length} exceeds maximum {MaxLength}",
                    parameterName, input.Length, maxLength);
                throw new ArgumentException($"Parameter '{parameterName}' exceeds maximum length of {maxLength} characters");
            }
        }
    }
}
