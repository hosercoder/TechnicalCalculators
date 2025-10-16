using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using HC.TechnicalCalculators.Src.Calculators.Momentum;
using HC.TechnicalCalculators.Src.Calculators.Overlap;
using HC.TechnicalCalculators.Src.Calculators.Price;
using HC.TechnicalCalculators.Src.Calculators.Volume;
using HC.TechnicalCalculators.Src.Factory;
using HC.TechnicalCalculators.Src.Interfaces;
using HC.TechnicalCalculators.Src.Models;
using HC.TechnicalCalculators.Src.Security;
using Xunit;

namespace HC.TechnicalCalculators.Tests.Factory
{
    public class DependencyInjectionCalculatorFactoryTests //: IDisposable
    {
        /*
        private readonly Mock<IServiceProvider> _mockServiceProvider;
        private readonly Mock<INewsFeedService> _mockNewsFeedService;
        private readonly Mock<IInputValidationService> _mockValidationService;
        private readonly DependencyInjectionCalculatorFactory _factory;

        public DependencyInjectionCalculatorFactoryTests()
        {
            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockNewsFeedService = new Mock<INewsFeedService>();
            _mockValidationService = new Mock<IInputValidationService>();

            // Setup validation service
            _mockValidationService.Setup(v => v.ValidateStringLength(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()));
            
            // Setup service provider to return validation service
            _mockServiceProvider.Setup(sp => sp.GetService(typeof(IInputValidationService)))
                .Returns(_mockValidationService.Object);
            _mockServiceProvider.Setup(sp => sp.GetService(typeof(INewsFeedService)))
                .Returns(_mockNewsFeedService.Object);

            //_factory = new DependencyInjectionCalculatorFactory(_mockServiceProvider.Object, 
            //    _mockValidationService.Object);
        }

        [Theory]
        [InlineData(CalculatorNameEnum.SMA, typeof(SimpleMovingAverage))]
        [InlineData(CalculatorNameEnum.RSI, typeof(RsiCalculator))]
        [InlineData(CalculatorNameEnum.AVGPRICE, typeof(AvgPriceCalculator))]
        [InlineData(CalculatorNameEnum.OBV, typeof(ObvCalculator))]
        public void CreateCalculator_ShouldReturnCorrectCalculatorType(CalculatorNameEnum calculatorType, Type expectedType)
        {
            // Arrange
            var parameters = new Dictionary<string, string>();
            if (calculatorType == CalculatorNameEnum.RSI || calculatorType == CalculatorNameEnum.SMA)
            {
                parameters[nameof(ParameterNamesEnum.Period)] = "14";
            }

            // Act
            var calculator = _factory.CreateCalculator(calculatorType, parameters, nameof(calculatorType));

            // Assert
            Assert.NotNull(calculator);
            Assert.IsType(expectedType, calculator);
        }

        [Fact]
        public void CreateCalculator_ShouldThrowNotSupportedException_ForUnsupportedCalculator()
        {
            // Arrange - Use reflection to get a value that's not in the enum
            var invalidCalculatorName = (CalculatorNameEnum)999;
            var parameters = new Dictionary<string, string>();

            // Act & Assert
            var exception = Assert.Throws<NotSupportedException>(
                () => _factory.CreateCalculator(invalidCalculatorName, parameters, nameof(invalidCalculatorName)));

            Assert.Contains("not supported", exception.Message);
        }

        [Fact]
        public void GetTechnicalIndicators_ShouldReturnCorrectNames_ForSupportedCalculator()
        {
            // Arrange
            var calculatorType = CalculatorNameEnum.SMA;

            // Act
            var names = _factory.GetTechnicalIndicators(calculatorType);

            // Assert
            Assert.NotNull(names);
            Assert.NotEmpty(names);
            Assert.Contains("MOVINGAVERAGE", names);
        }

        [Fact]
        public void Constructor_WithNullServiceProvider_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => 
                new DependencyInjectionCalculatorFactory(null!, _mockValidationService.Object));
            Assert.Equal("serviceProvider", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullValidationService_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => 
                new DependencyInjectionCalculatorFactory(_mockServiceProvider.Object, null!));
            Assert.Equal("validationService", exception.ParamName);
        }

        [Fact]
        public void CreateCalculator_WithNullParameters_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => 
                _factory.CreateCalculator(CalculatorNameEnum.RSI, null!, "TestRSI"));
            Assert.Equal("parameters", exception.ParamName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void CreateCalculator_WithInvalidName_ShouldThrowArgumentException(string name)
        {
            // Arrange
            var parameters = new Dictionary<string, string>();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _factory.CreateCalculator(CalculatorNameEnum.RSI, parameters, name));
            Assert.Equal("name", exception.ParamName);
        }

        [Fact]
        public void CreateCalculator_ShouldValidateParameterKeys()
        {
            // Arrange
            var parameters = new Dictionary<string, string> 
            { 
                { "Period", "14" },
                { "LongParameterKey", "value" }
            };

            // Act
            _factory.CreateCalculator(CalculatorNameEnum.RSI, parameters, "TestRSI");

            // Assert
            _mockValidationService.Verify(x => x.ValidateStringLength("Period", 50, "parameter key"), Times.Once);
            _mockValidationService.Verify(x => x.ValidateStringLength("LongParameterKey", 50, "parameter key"), Times.Once);
        }

        [Fact]
        public void CreateCalculator_ShouldValidateParameterValues()
        {
            // Arrange
            var parameters = new Dictionary<string, string> 
            { 
                { "Period", "14" },
                { "TestParam", "TestValue" }
            };

            // Act
            _factory.CreateCalculator(CalculatorNameEnum.RSI, parameters, "TestRSI");

            // Assert
            _mockValidationService.Verify(x => x.ValidateStringLength("14", 100, "parameter value"), Times.Once);
            _mockValidationService.Verify(x => x.ValidateStringLength("TestValue", 100, "parameter value"), Times.Once);
        }

        [Fact]
        public void CreateCalculator_WithEmptyParameters_ShouldNotThrowForParameters()
        {
            // Arrange
            var parameters = new Dictionary<string, string>();

            // Act
            var calculator = _factory.CreateCalculator(CalculatorNameEnum.AVGPRICE, parameters, "TestAvgPrice");

            // Assert
            Assert.NotNull(calculator);
        }

        [Theory]
        [InlineData(CalculatorNameEnum.EMA)]
        public void CreateCalculator_WithMultipleValidCalculatorTypes_ShouldCreateSuccessfully(CalculatorNameEnum calculatorType)
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { "Period", "14" } };

            // Act
            var calculator = _factory.CreateCalculator(calculatorType, parameters, $"Test{calculatorType}");

            // Assert
            Assert.NotNull(calculator);
        }

        [Fact]
        public void GetTechnicalIndicators_WithUnsupportedCalculator_ShouldThrowNotSupportedException()
        {
            // Arrange
            var unsupportedType = (CalculatorNameEnum)999;

            // Act & Assert
            var exception = Assert.Throws<NotSupportedException>(() => 
                _factory.GetTechnicalIndicators(unsupportedType));
            Assert.Contains("not supported", exception.Message);
        }

        [Theory]
        [InlineData(CalculatorNameEnum.RSI)]
        [InlineData(CalculatorNameEnum.SMA)]
        [InlineData(CalculatorNameEnum.AVGPRICE)]
        [InlineData(CalculatorNameEnum.OBV)]
        public void GetTechnicalIndicators_WithValidCalculators_ShouldReturnNonEmptyResults(CalculatorNameEnum calculatorType)
        {
            // Act
            var indicators = _factory.GetTechnicalIndicators(calculatorType);

            // Assert
            Assert.NotNull(indicators);
            Assert.NotEmpty(indicators);
        }

        [Fact]
        public void CreateCalculator_WithComplexParameters_ShouldValidateAllParameters()
        {
            // Arrange
            var parameters = new Dictionary<string, string> 
            { 
                { "Period", "14" },
                { "FastPeriod", "12" },
                { "SlowPeriod", "26" },
                { "SignalPeriod", "9" },
                { "Multiplier", "2.0" }
            };

            // Act
            _factory.CreateCalculator(CalculatorNameEnum.RSI, parameters, "ComplexTest");

            // Assert - Verify all parameter keys and values are validated
            _mockValidationService.Verify(x => x.ValidateStringLength(It.IsAny<string>(), 50, "parameter key"), Times.Exactly(5));
            _mockValidationService.Verify(x => x.ValidateStringLength(It.IsAny<string>(), 100, "parameter value"), Times.Exactly(5));
        }

        [Fact]
        public void CreateCalculator_WithSpecialCharactersInName_ShouldSucceed()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { "Period", "14" } };
            var nameWithSpecialChars = "Test_Calculator-Name.123";

            // Act
            var calculator = _factory.CreateCalculator(CalculatorNameEnum.RSI, parameters, nameWithSpecialChars);

            // Assert
            Assert.NotNull(calculator);
        }

        [Fact]
        public void CreateCalculator_ValidationServiceThrows_ShouldPropagateException()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { "InvalidKey", "value" } };
            _mockValidationService.Setup(x => x.ValidateStringLength("InvalidKey", 50, "parameter key"))
                .Throws(new ArgumentException("Invalid parameter key"));

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => 
                _factory.CreateCalculator(CalculatorNameEnum.RSI, parameters, "TestRSI"));
            Assert.Equal("Invalid parameter key", exception.Message);
        }

        [Fact]
        public void CreateCalculator_WithLongParameterValues_ShouldValidateWithCorrectLimits()
        {
            // Arrange
            var longValue = new string('A', 150); // Longer than the 100 character limit
            var parameters = new Dictionary<string, string> { { "LongParam", longValue } };

            // Act
            var exceptions = Assert.Throws<ArgumentException>( () =>_factory.CreateCalculator(CalculatorNameEnum.RSI, parameters, "TestLongParam"));

            // Assert
           Assert.Contains("Parameter 'parameter value' exceeds maximum length of 100 characters", exceptions.Message);
        }

        [Theory]
        [InlineData(CalculatorNameEnum.ADX)]
        [InlineData(CalculatorNameEnum.ATR)]
        [InlineData(CalculatorNameEnum.MFI)]
        [InlineData(CalculatorNameEnum.STOCH)]
        public void CreateCalculator_WithAdditionalCalculatorTypes_ShouldCreateSuccessfully(CalculatorNameEnum calculatorType)
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { "Period", "14" } };

            // Act
            var calculator = _factory.CreateCalculator(calculatorType, parameters, $"Test{calculatorType}");

            // Assert
            Assert.NotNull(calculator);
        }

        [Fact]
        public void GetTechnicalIndicators_WithMultipleCallsSameCalculator_ShouldReturnConsistentResults()
        {
            // Arrange
            var calculatorType = CalculatorNameEnum.RSI;

            // Act
            var indicators1 = _factory.GetTechnicalIndicators(calculatorType);
            var indicators2 = _factory.GetTechnicalIndicators(calculatorType);

            // Assert
            Assert.Equal(indicators1.Count(), indicators2.Count());
            Assert.True(indicators1.SequenceEqual(indicators2));
        }

        [Fact]
        public void CreateCalculator_WithNumericParameterValues_ShouldValidateCorrectly()
        {
            // Arrange
            var parameters = new Dictionary<string, string> 
            { 
                { "Period", "14" },
                { "Percentage", "0.5" },
                { "Count", "100" }
            };

            // Act
            _factory.CreateCalculator(CalculatorNameEnum.RSI, parameters, "NumericTest");

            // Assert
            _mockValidationService.Verify(x => x.ValidateStringLength("14", 100, "parameter value"), Times.Once);
            _mockValidationService.Verify(x => x.ValidateStringLength("0.5", 100, "parameter value"), Times.Once);
            _mockValidationService.Verify(x => x.ValidateStringLength("100", 100, "parameter value"), Times.Once);
        }

        public void Dispose()
        {
            // Cleanup if needed
        }
        */
    }
}
