using HC.TechnicalCalculators.Src.Calculators;
using HC.TechnicalCalculators.Src.Interfaces;
using HC.TechnicalCalculators.Src.Models;
using HC.TechnicalCalculators.Src.Security;
using HC.TechnicalCalculators.Tests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace HC.TechnicalCalculators.Tests.Calculators
{
    public class BaseCalculatorAdvancedTests
    {
        private readonly Mock<IInputValidationService> _mockValidationService;

        public BaseCalculatorAdvancedTests()
        {
            _mockValidationService = new Mock<IInputValidationService>();
            _mockValidationService.Setup(x => x.ValidateStringLength(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()));
            _mockValidationService.Setup(x => x.IsValidPriceData(It.IsAny<double[,]>())).Returns(true);
            _mockValidationService.Setup(x => x.ValidateArraySize(It.IsAny<double[,]>(), It.IsAny<int>()));
        }

        [Fact]
        public void Constructor_WithValidationService_ShouldUseProvidedService()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { "test", "value" } };
            var mockService = new Mock<IInputValidationService>();
            mockService.Setup(x => x.ValidateStringLength(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()));

            // Act
            var calculator = new SimpleTestBaseCalculator("TestCalc", parameters, mockService.Object);

            // Assert
            Assert.NotNull(calculator);
            mockService.Verify(x => x.ValidateStringLength("TestCalc", It.IsAny<int>(), "name"), Times.Once);
            mockService.Verify(x => x.ValidateStringLength("test", It.IsAny<int>(), "parameter key"), Times.Once);
            mockService.Verify(x => x.ValidateStringLength("value", It.IsAny<int>(), "parameter value"), Times.Once);
        }

        [Fact]
        public void Constructor_WithNullValidationService_ShouldThrowArgumentNullException()
        {
            // Arrange
            var parameters = new Dictionary<string, string>();

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => 
                new SimpleTestBaseCalculator("TestCalc", parameters, null!));
            Assert.Equal("validationService", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullParameters_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => 
                new SimpleTestBaseCalculator("TestCalc", null!, _mockValidationService.Object));
            Assert.Equal("para", exception.ParamName);
            Assert.Contains("Parameters dictionary cannot be null", exception.Message);
        }

        [Fact]
        public void Constructor_WithNullOrWhitespaceName_ShouldThrowArgumentException()
        {
            // Arrange
            var parameters = new Dictionary<string, string>();

            // Act & Assert - Null name
            var exception1 = Assert.Throws<ArgumentException>(() => 
                new SimpleTestBaseCalculator(null!, parameters, _mockValidationService.Object));
            Assert.Equal("name", exception1.ParamName);

            // Act & Assert - Empty name
            var exception2 = Assert.Throws<ArgumentException>(() => 
                new SimpleTestBaseCalculator("", parameters, _mockValidationService.Object));
            Assert.Equal("name", exception2.ParamName);

            // Act & Assert - Whitespace name
            var exception3 = Assert.Throws<ArgumentException>(() => 
                new SimpleTestBaseCalculator("   ", parameters, _mockValidationService.Object));
            Assert.Equal("name", exception3.ParamName);
        }

        [Fact]
        public void Constructor_WithNAParameter_ShouldSkipValidation()
        {
            // Arrange
            var parameters = new Dictionary<string, string> 
            { 
                { nameof(ParameterNamesEnum.NA), "ignored" },
                { "valid", "parameter" }
            };

            // Act
            var calculator = new SimpleTestBaseCalculator("TestCalc", parameters, _mockValidationService.Object);

            // Assert
            Assert.NotNull(calculator);
            // Should only validate the non-NA parameter
            _mockValidationService.Verify(x => x.ValidateStringLength("valid", It.IsAny<int>(), "parameter key"), Times.Once);
            _mockValidationService.Verify(x => x.ValidateStringLength("parameter", It.IsAny<int>(), "parameter value"), Times.Once);
        }

        [Fact]
        public void Constructor_WithEmptyParameterKey_ShouldThrowArgumentException()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { "", "value" } };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                new SimpleTestBaseCalculator("TestCalc", parameters, _mockValidationService.Object));
            Assert.Contains("Parameter key cannot be null or empty", exception.Message);
        }

        [Fact]
        public void Calculate_WithSkipValidationTrue_ShouldSkipMostValidation()
        {
            // Arrange
            var parameters = new Dictionary<string, string>();
            var calculator = new SimpleTestBaseCalculator("TestCalc", parameters, _mockValidationService.Object);
            var prices = Pricedata.GetPrices(10);

            // Act
            var result = calculator.Calculate(prices, skipValidation: true);

            // Assert
            Assert.NotNull(result);
            // Should not call validation methods when skipValidation is true
            _mockValidationService.Verify(x => x.IsValidPriceData(It.IsAny<double[,]>()), Times.Never);
            _mockValidationService.Verify(x => x.ValidateArraySize(It.IsAny<double[,]>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void Calculate_WithInvalidPriceData_ShouldThrowArgumentException()
        {
            // Arrange
            var parameters = new Dictionary<string, string>();
            var calculator = new SimpleTestBaseCalculator("TestCalc", parameters, _mockValidationService.Object);
            var prices = Pricedata.GetPrices(10);
            
            _mockValidationService.Setup(x => x.IsValidPriceData(It.IsAny<double[,]>())).Returns(false);

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => calculator.Calculate(prices));
            Assert.Contains("Invalid price data provided", exception.Message);
        }

        [Fact]
        public void Calculate_WithWrongNumberOfColumns_ShouldThrowArgumentException()
        {
            // Arrange
            var parameters = new Dictionary<string, string>();
            var calculator = new SimpleTestBaseCalculator("TestCalc", parameters, _mockValidationService.Object);
            var prices = new double[10, 5]; // Wrong number of columns (should be 6)

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => calculator.Calculate(prices));
            Assert.Contains("Prices array must have 6 columns", exception.Message);
        }

        [Fact]
        public void Calculate_ShouldPopulatePriceArraysCorrectly()
        {
            // Arrange
            var parameters = new Dictionary<string, string>();
            var calculator = new SimpleTestBaseCalculator("TestCalc", parameters, _mockValidationService.Object);
            var prices = new double[3, 6]
            {
                { 1000, 100, 105, 99, 102, 50000 },   // timestamp, open, high, low, close, volume
                { 2000, 102, 108, 101, 107, 60000 },
                { 3000, 107, 110, 106, 109, 55000 }
            };

            // Act
            calculator.Calculate(prices);

            // Assert
            Assert.Equal(3, calculator.Timestamp.Length);
            Assert.Equal(3, calculator.Open.Length);
            Assert.Equal(3, calculator.High.Length);
            Assert.Equal(3, calculator.Low.Length);
            Assert.Equal(3, calculator.Close.Length);
            Assert.Equal(3, calculator.Volume.Length);

            // Check first row
            Assert.Equal(1000, calculator.Timestamp[0]);
            Assert.Equal(100, calculator.Open[0]);
            Assert.Equal(105, calculator.High[0]);
            Assert.Equal(99, calculator.Low[0]);
            Assert.Equal(102, calculator.Close[0]);
            Assert.Equal(50000, calculator.Volume[0]);

            // Check last row
            Assert.Equal(3000, calculator.Timestamp[2]);
            Assert.Equal(107, calculator.Open[2]);
            Assert.Equal(110, calculator.High[2]);
            Assert.Equal(106, calculator.Low[2]);
            Assert.Equal(109, calculator.Close[2]);
            Assert.Equal(55000, calculator.Volume[2]);
        }

        [Fact]
        public void LegacyConstructor_ShouldCreateDefaultValidationService()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { "test", "value" } };

            // Act
            var calculator = new SimpleTestBaseCalculator("TestCalc", parameters);

            // Assert
            Assert.NotNull(calculator);
            // Should work without throwing exceptions
        }

        [Fact]
        public void Calculate_WithNullPricesAndSkipValidation_ShouldStillThrowArgumentNullException()
        {
            // Arrange
            var parameters = new Dictionary<string, string>();
            var calculator = new SimpleTestBaseCalculator("TestCalc", parameters, _mockValidationService.Object);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => calculator.Calculate(null!, skipValidation: true));
            Assert.Equal("prices", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithTooLongParameterName_ShouldCallValidation()
        {
            // Arrange
            var longParameterName = new string('a', 150);
            var parameters = new Dictionary<string, string> { { longParameterName, "value" } };

            // Act
            var calculator = new SimpleTestBaseCalculator("TestCalc", parameters, _mockValidationService.Object);

            // Assert
            _mockValidationService.Verify(x => x.ValidateStringLength(longParameterName, 100, "parameter key"), Times.Once);
        }

        [Fact]
        public void Constructor_WithTooLongParameterValue_ShouldCallValidation()
        {
            // Arrange
            var longParameterValue = new string('b', 150);
            var parameters = new Dictionary<string, string> { { "test", longParameterValue } };

            // Act
            var calculator = new SimpleTestBaseCalculator("TestCalc", parameters, _mockValidationService.Object);

            // Assert
            _mockValidationService.Verify(x => x.ValidateStringLength(longParameterValue, 100, "parameter value"), Times.Once);
        }

        [Fact]
        public void Calculate_ShouldCallValidateArraySize()
        {
            // Arrange
            var parameters = new Dictionary<string, string>();
            var calculator = new SimpleTestBaseCalculator("TestCalc", parameters, _mockValidationService.Object);
            var prices = Pricedata.GetPrices(10);

            // Act
            calculator.Calculate(prices);

            // Assert
            _mockValidationService.Verify(x => x.ValidateArraySize(prices, 1000000), Times.Once);
        }

        [Fact]
        public void PriceArrays_ShouldBeInitializedAsEmpty()
        {
            // Arrange
            var parameters = new Dictionary<string, string>();

            // Act
            var calculator = new SimpleTestBaseCalculator("TestCalc", parameters, _mockValidationService.Object);

            // Assert
            Assert.Empty(calculator.High);
            Assert.Empty(calculator.Low);
            Assert.Empty(calculator.Close);
            Assert.Empty(calculator.Volume);
            Assert.Empty(calculator.Open);
            Assert.Empty(calculator.Timestamp);
        }
    }

    // Test implementation of BaseCalculator for testing purposes
    public class SimpleTestBaseCalculator : BaseCalculator
    {
        public SimpleTestBaseCalculator(string name, Dictionary<string, string> parameters, IInputValidationService validationService)
            : base(name, parameters, validationService)
        {
        }

        public SimpleTestBaseCalculator(string name, Dictionary<string, string> parameters)
            : base(name, parameters)
        {
        }

        public SimpleTestBaseCalculator(Dictionary<string, string> parameters)
            : base("TestCalculator", parameters)
        {
        }

        protected override CalculatorResults CalculateInternal(double[,] prices)
        {
            return new CalculatorResults
            {
                Name = "TestCalculator",
                Results = new Dictionary<long, KeyValuePair<string, double>[]>()
            };
        }

        // Expose arrays for testing
        public new double[] High => base.High;
        public new double[] Low => base.Low;
        public new double[] Close => base.Close;
        public new double[] Volume => base.Volume;
        public new double[] Open => base.Open;
        public new double[] Timestamp => base.Timestamp;
    }
}
