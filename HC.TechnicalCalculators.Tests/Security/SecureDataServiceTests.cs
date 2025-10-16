using Microsoft.Extensions.Logging;
using Moq;
using HC.TechnicalCalculators.Src.Security;
using System.Text;
using Xunit;

namespace HC.TechnicalCalculators.Tests.Security
{
    public class SecureDataServiceTests : IDisposable
    {
        private readonly Mock<ILogger<SecureDataService>> _mockLogger;
        private readonly SecureDataService _secureDataService;

        public SecureDataServiceTests()
        {
            _mockLogger = new Mock<ILogger<SecureDataService>>();
            _secureDataService = new SecureDataService(_mockLogger.Object);
        }

        [Fact]
        public void Constructor_WithValidLogger_ShouldCreateInstance()
        {
            // Act
            var service = new SecureDataService(_mockLogger.Object);

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new SecureDataService(null!));
            Assert.Equal("Logger can not be null", exception.ParamName);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void ProtectString_WithNullOrEmptyData_ShouldReturnEmptyString(string input)
        {
            // Act
            var result = _secureDataService.ProtectString(input);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Theory]
        [InlineData("Hello World")]
        [InlineData("Test123!@#")]
        [InlineData("Special chars: åäö")]
        [InlineData("Numbers: 1234567890")]
        public void ProtectString_WithValidData_ShouldReturnBase64String(string input)
        {
            // Act
            var result = _secureDataService.ProtectString(input);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.NotEqual(input, result);
            
            // Should be valid base64
            var base64Bytes = Convert.FromBase64String(result);
            Assert.NotEmpty(base64Bytes);
        }

        [Theory]
        [InlineData("Hello World")]
        [InlineData("Test123!@#")]
        [InlineData("Special chars: åäö")]
        [InlineData("Numbers: 1234567890")]
        [InlineData("Long string: " + "This is a very long string that contains many characters and should test the encryption and decryption process thoroughly.")]
        public void ProtectString_ThenUnprotectString_ShouldReturnOriginalData(string original)
        {
            // Act
            var protected_ = _secureDataService.ProtectString(original);
            var unprotected = _secureDataService.UnprotectString(protected_);

            // Assert
            Assert.Equal(original, unprotected);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void UnprotectString_WithNullOrEmptyData_ShouldReturnEmptyString(string input)
        {
            // Act
            var result = _secureDataService.UnprotectString(input);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void UnprotectString_WithInvalidBase64_ShouldThrowException()
        {
            // Arrange
            var invalidBase64 = "This is not valid base64!";

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => 
                _secureDataService.UnprotectString(invalidBase64));
            Assert.Equal("Data unprotection failed", exception.Message);
            
            // Verify error was logged
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void ProtectBytes_WithNullData_ShouldReturnEmptyArray()
        {
            // Act
            var result = _secureDataService.ProtectBytes(null!);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void ProtectBytes_WithEmptyData_ShouldReturnEmptyArray()
        {
            // Act
            var result = _secureDataService.ProtectBytes(Array.Empty<byte>());

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Theory]
        [InlineData(new byte[] { 1, 2, 3, 4, 5 })]
        [InlineData(new byte[] { 255, 254, 253, 0, 1 })]
        public void ProtectBytes_WithValidData_ShouldReturnDifferentBytes(byte[] input)
        {
            // Act
            var result = _secureDataService.ProtectBytes(input);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
            Assert.NotEqual(input, result);
            Assert.True(result.Length > input.Length); // Should be larger due to IV
        }

        [Theory]
        [InlineData(new byte[] { 1, 2, 3, 4, 5 })]
        [InlineData(new byte[] { 255, 254, 253, 0, 1 })]
        [InlineData(new byte[] { 0 })]
        public void ProtectBytes_ThenUnprotectBytes_ShouldReturnOriginalData(byte[] original)
        {
            // Act
            var protected_ = _secureDataService.ProtectBytes(original);
            var unprotected = _secureDataService.UnprotectBytes(protected_);

            // Assert
            Assert.Equal(original, unprotected);
        }

        [Fact]
        public void UnprotectBytes_WithNullData_ShouldReturnEmptyArray()
        {
            // Act
            var result = _secureDataService.UnprotectBytes(null!);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void UnprotectBytes_WithEmptyData_ShouldReturnEmptyArray()
        {
            // Act
            var result = _secureDataService.UnprotectBytes(Array.Empty<byte>());

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void ProtectString_WithLargeData_ShouldHandleSuccessfully()
        {
            // Arrange
            var largeString = new string('A', 10000);

            // Act
            var protected_ = _secureDataService.ProtectString(largeString);
            var unprotected = _secureDataService.UnprotectString(protected_);

            // Assert
            Assert.Equal(largeString, unprotected);
        }

        [Fact]
        public void ProtectBytes_WithLargeData_ShouldHandleSuccessfully()
        {
            // Arrange
            var largeData = new byte[10000];
            for (int i = 0; i < largeData.Length; i++)
            {
                largeData[i] = (byte)(i % 256);
            }

            // Act
            var protected_ = _secureDataService.ProtectBytes(largeData);
            var unprotected = _secureDataService.UnprotectBytes(protected_);

            // Assert
            Assert.Equal(largeData, unprotected);
        }

        [Fact]
        public void ProtectString_MultipleCalls_ShouldProduceDifferentResults()
        {
            // Arrange
            var input = "Same input string";

            // Act
            var result1 = _secureDataService.ProtectString(input);
            var result2 = _secureDataService.ProtectString(input);

            // Assert
            Assert.NotEqual(result1, result2); // Should be different due to random IV
            
            // But both should decrypt to the same original
            Assert.Equal(input, _secureDataService.UnprotectString(result1));
            Assert.Equal(input, _secureDataService.UnprotectString(result2));
        }

        [Fact]
        public void ProtectBytes_MultipleCalls_ShouldProduceDifferentResults()
        {
            // Arrange
            var input = new byte[] { 1, 2, 3, 4, 5 };

            // Act
            var result1 = _secureDataService.ProtectBytes(input);
            var result2 = _secureDataService.ProtectBytes(input);

            // Assert
            Assert.NotEqual(result1, result2); // Should be different due to random IV
            
            // But both should decrypt to the same original
            Assert.Equal(input, _secureDataService.UnprotectBytes(result1));
            Assert.Equal(input, _secureDataService.UnprotectBytes(result2));
        }

        [Fact]
        public void ProtectString_WithUnicodeCharacters_ShouldHandleCorrectly()
        {
            // Arrange
            var unicodeString = "Hello 世界 🌍 émojis 🚀";

            // Act
            var protected_ = _secureDataService.ProtectString(unicodeString);
            var unprotected = _secureDataService.UnprotectString(protected_);

            // Assert
            Assert.Equal(unicodeString, unprotected);
        }

        [Fact]
        public void UnprotectBytes_WithInvalidData_ShouldThrowException()
        {
            // Arrange
            var invalidData = new byte[] { 1, 2, 3 }; // Too short to contain valid IV + encrypted data

            // Act & Assert
            var exception = Assert.ThrowsAny<Exception>(() => 
                _secureDataService.UnprotectBytes(invalidData));
            Assert.NotNull(exception);
        }

        [Fact]
        public void ProtectBytes_WithSingleByte_ShouldWork()
        {
            // Arrange
            var singleByte = new byte[] { 42 };

            // Act
            var protected_ = _secureDataService.ProtectBytes(singleByte);
            var unprotected = _secureDataService.UnprotectBytes(protected_);

            // Assert
            Assert.Equal(singleByte, unprotected);
        }

        [Fact]
        public void SecureDataService_ImplementsAllInterfaces()
        {
            // Assert
            Assert.IsAssignableFrom<ISecureDataService>(_secureDataService);
            Assert.IsAssignableFrom<IStringDataProtector>(_secureDataService);
            Assert.IsAssignableFrom<IBinaryDataProtector>(_secureDataService);
        }

        [Fact]
        public void ProtectString_WithMaxLengthString_ShouldWork()
        {
            // Arrange - Test with a reasonably large string
            var maxString = new string('X', 1000);

            // Act
            var protected_ = _secureDataService.ProtectString(maxString);
            var unprotected = _secureDataService.UnprotectString(protected_);

            // Assert
            Assert.Equal(maxString, unprotected);
        }

        public void Dispose()
        {
            // Cleanup if needed
        }
    }
}
