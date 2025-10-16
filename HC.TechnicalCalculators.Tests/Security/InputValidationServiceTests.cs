using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using HC.TechnicalCalculators.Src.Security;
using Xunit;

namespace HC.TechnicalCalculators.Tests.Security
{
    /// <summary>
    /// Unit tests for the InputValidationService class.
    /// Tests validation methods for string input, URLs, symbols, price data, and array size validation.
    /// </summary>
    public class InputValidationServiceTests
    {
        private readonly InputValidationService _validationService;
        private readonly ILogger<InputValidationService> _logger;

        /// <summary>
        /// Initializes a new instance of the InputValidationServiceTests class.
        /// Sets up the validation service with a null logger for testing.
        /// </summary>
        public InputValidationServiceTests()
        {
            _logger = NullLogger<InputValidationService>.Instance;
            _validationService = new InputValidationService(_logger);
        }

        #region ValidateStringLength Tests

        /// <summary>
        /// Tests that ValidateStringLength does not throw an exception when provided with a string within the maximum length.
        /// </summary>
        [Fact]
        public void ValidateStringLength_WithValidLength_ShouldNotThrow()
        {
            // Act & Assert
            _validationService.ValidateStringLength("valid", 10, "test");
        }

        /// <summary>
        /// Tests that ValidateStringLength throws ArgumentException when the string exceeds the maximum length.
        /// </summary>
        [Fact]
        public void ValidateStringLength_WithTooLongString_ShouldThrowArgumentException()
        {
            // Arrange
            var longString = new string('a', 15);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _validationService.ValidateStringLength(longString, 10, "test"));
            Assert.Contains("exceeds maximum length", exception.Message);
            Assert.Contains("10", exception.Message);
        }

        /// <summary>
        /// Tests that ValidateStringLength throws ArgumentException when the string equals the maximum length exactly.
        /// </summary>
        [Fact]
        public void ValidateStringLength_WithZeroMaxLength_ShouldThrowArgumentException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _validationService.ValidateStringLength("test", 0, "test"));
            Assert.Contains("exceeds maximum length", exception.Message);
        }

        /// <summary>
        /// Tests that ValidateStringLength does not throw when input is null.
        /// </summary>
        [Fact]
        public void ValidateStringLength_WithNullString_ShouldNotThrow()
        {
            // Act & Assert - Should not throw for null input
            _validationService.ValidateStringLength(null!, 10, "test");
        }

        #endregion

        #region IsValidSymbol Tests

        /// <summary>
        /// Tests that IsValidSymbol returns true for valid stock symbols.
        /// </summary>
        /// <param name="symbol">The stock symbol to test</param>
        [Theory]
        [InlineData("AAPL")]
        [InlineData("MSFT")]
        [InlineData("GOOGL")]
        [InlineData("BERKSHIRE")] // Up to 10 characters
        public void IsValidSymbol_WithValidSymbols_ShouldReturnTrue(string symbol)
        {
            // Act
            var result = _validationService.IsValidSymbol(symbol);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that IsValidSymbol returns false for invalid stock symbols.
        /// </summary>
        /// <param name="symbol">The invalid stock symbol to test</param>
        [Theory]
        [InlineData("")]
        [InlineData("ABCDEFGHIJK")] // Too long (11 characters)
        [InlineData("12345")]
        [InlineData("a@pl")] // Lowercase and special characters
        [InlineData("AP#L")]
        [InlineData("AAPL.")]
        public void IsValidSymbol_WithInvalidSymbols_ShouldReturnFalse(string symbol)
        {
            // Act
            var result = _validationService.IsValidSymbol(symbol);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsValidSymbol returns false when the symbol is null.
        /// </summary>
        [Fact]
        public void IsValidSymbol_WithNullSymbol_ShouldReturnFalse()
        {
            // Act
            var result = _validationService.IsValidSymbol(null!);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region IsValidUrl Tests

        /// <summary>
        /// Tests that IsValidUrl returns true for valid HTTP and HTTPS URLs.
        /// </summary>
        /// <param name="url">The URL to test</param>
        [Theory]
        [InlineData("https://www.example.com")]
        [InlineData("http://example.com")]
        [InlineData("https://api.example.com/v1/data")]
        [InlineData("https://subdomain.example.co.uk")]
        public void IsValidUrl_WithValidUrls_ShouldReturnTrue(string url)
        {
            // Act
            var result = _validationService.IsValidUrl(url);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that IsValidUrl returns false for invalid or malformed URLs.
        /// </summary>
        /// <param name="url">The invalid URL to test</param>
        [Theory]
        [InlineData("")]
        [InlineData("not-a-url")]
        [InlineData("ftp://example.com")]
        [InlineData("javascript:alert('xss')")]
        [InlineData("file:///etc/passwd")]
        [InlineData("http://")]
        [InlineData("https://")]
        public void IsValidUrl_WithInvalidUrls_ShouldReturnFalse(string url)
        {
            // Act
            var result = _validationService.IsValidUrl(url);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsValidUrl returns false when the URL is null.
        /// </summary>
        [Fact]
        public void IsValidUrl_WithNullUrl_ShouldReturnFalse()
        {
            // Act
            var result = _validationService.IsValidUrl(null!);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region IsValidPriceData Tests

        /// <summary>
        /// Tests that IsValidPriceData returns true for well-formed price data with valid values.
        /// </summary>
        [Fact]
        public void IsValidPriceData_WithValidPriceData_ShouldReturnTrue()
        {
            // Arrange
            var validPrices = new double[3, 6]
            {
                { 1000, 100, 105, 99, 102, 50000 },
                { 2000, 102, 108, 101, 107, 60000 },
                { 3000, 107, 110, 106, 109, 55000 }
            };

            // Act
            var result = _validationService.IsValidPriceData(validPrices);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that IsValidPriceData returns false when the price data array is null.
        /// </summary>
        [Fact]
        public void IsValidPriceData_WithNullPrices_ShouldReturnFalse()
        {
            // Act
            var result = _validationService.IsValidPriceData(null!);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsValidPriceData returns false when the price data doesn't have exactly 6 columns.
        /// </summary>
        [Fact]
        public void IsValidPriceData_WithWrongColumns_ShouldReturnFalse()
        {
            // Arrange
            var invalidPrices = new double[3, 5];

            // Act
            var result = _validationService.IsValidPriceData(invalidPrices);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsValidPriceData returns false when price data contains infinite values.
        /// </summary>
        [Fact]
        public void IsValidPriceData_WithInfiniteValues_ShouldReturnFalse()
        {
            // Arrange
            var invalidPrices = new double[1, 6]
            {
                { 1000, double.PositiveInfinity, 105, 99, 102, 50000 }
            };

            // Act
            var result = _validationService.IsValidPriceData(invalidPrices);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsValidPriceData returns false when price data contains NaN values.
        /// </summary>
        [Fact]
        public void IsValidPriceData_WithNaNValues_ShouldReturnFalse()
        {
            // Arrange
            var invalidPrices = new double[1, 6]
            {
                { 1000, double.NaN, 105, 99, 102, 50000 }
            };

            // Act
            var result = _validationService.IsValidPriceData(invalidPrices);

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsValidPriceData returns false when the price data array is empty.
        /// </summary>
        [Fact]
        public void IsValidPriceData_WithEmptyArray_ShouldReturnFalse()
        {
            // Arrange
            var emptyPrices = new double[0, 6];

            // Act
            var result = _validationService.IsValidPriceData(emptyPrices);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region ValidateArraySize Tests

        /// <summary>
        /// Tests that ValidateArraySize does not throw when the array size is within the maximum limit.
        /// </summary>
        [Fact]
        public void ValidateArraySize_WithValidSize_ShouldNotThrow()
        {
            // Arrange
            var array = new double[10, 6];

            // Act & Assert
            _validationService.ValidateArraySize(array, 100);
        }

        /// <summary>
        /// Tests that ValidateArraySize throws ArgumentNullException when the array is null.
        /// </summary>
        [Fact]
        public void ValidateArraySize_WithNullArray_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => 
                _validationService.ValidateArraySize(null!, 100));
            Assert.Equal("array", exception.ParamName);
        }

        /// <summary>
        /// Tests that ValidateArraySize throws ArgumentException when the array size exceeds the maximum.
        /// </summary>
        [Fact]
        public void ValidateArraySize_WithTooLargeArray_ShouldThrowArgumentException()
        {
            // Arrange
            var largeArray = new double[15, 6];

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _validationService.ValidateArraySize(largeArray, 10));
            Assert.Contains("Array size", exception.Message);
            Assert.Contains("exceeds maximum", exception.Message);
        }

        /// <summary>
        /// Tests that ValidateArraySize does not throw when the array size equals exactly the maximum allowed size.
        /// </summary>
        [Fact]
        public void ValidateArraySize_WithExactMaxSize_ShouldNotThrow()
        {
            // Arrange
            var array = new double[10, 6];

            // Act & Assert
            _validationService.ValidateArraySize(array, 60); // 10 * 6 = 60
        }

        #endregion

        #region Constructor Tests

        /// <summary>
        /// Tests that the InputValidationService constructor throws ArgumentNullException when logger is null.
        /// </summary>
        [Fact]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => 
                new InputValidationService(null!));
            Assert.Equal("Logger Must be provided", exception.ParamName);
        }

        /// <summary>
        /// Tests that the InputValidationService constructor successfully creates an instance with a valid logger.
        /// </summary>
        [Fact]
        public void Constructor_WithValidLogger_ShouldCreateInstance()
        {
            // Arrange
            var logger = NullLogger<InputValidationService>.Instance;

            // Act
            var service = new InputValidationService(logger);

            // Assert
            Assert.NotNull(service);
        }

        #endregion
    }
}
