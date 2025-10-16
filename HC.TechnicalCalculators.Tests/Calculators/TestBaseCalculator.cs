using HC.TechnicalCalculators.Src.Calculators;
using HC.TechnicalCalculators.Src.Models;
using HC.TechnicalCalculators.Tests.Helpers;

namespace HC.TechnicalCalculators.Tests.Calculators
{
    /// <summary>
    /// Test implementation of BaseCalculator for testing purposes
    /// </summary>
    public class TestBaseCalculator : BaseCalculator, IDisposable
    {
        private bool _disposed;

        public TestBaseCalculator(Dictionary<string, string> para)
            : base("base", para ?? new Dictionary<string, string>()) { }

        protected override CalculatorResults CalculateInternal(double[,] prices)
        {
            // Dummy implementation for testing purposes
            return new CalculatorResults
            {
                Name = nameof(CalculatorNameEnum.TEST),
                Results = new Dictionary<long, KeyValuePair<string, double>[]>()
            };
        }

        public static string[] GetTechnicalIndicatorNames()
        {
            return new string[] { nameof(TechnicalNamesEnum.ATR) };
        }

        public static IReadOnlyList<string> GetRequiredParamNames()
        {
            return new string[] { nameof(ParameterNamesEnum.ShortPeriod) };
        }

        // Expose protected methods for testing
        public void PublicValidatePeriod(Dictionary<string, string> parameters)
        {
            ValidatePeriod(parameters);
        }

        // Mock implementation for testing since ValidateRequiredParameters doesn't exist in BaseCalculator
        public void PublicValidateRequiredParameters()
        {
            var requiredParams = GetRequiredParamNames();
            foreach (var param in requiredParams)
            {
                if (!parameters.ContainsKey(param))
                {
                    throw new ArgumentException($"Required parameter '{param}' is missing.");
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            // If BaseCalculator implements IDisposable, call base.Dispose()
            // base.Dispose(); // Uncomment if BaseCalculator implements IDisposable
            _disposed = true;
        }
    }

    public class BaseCalculatorTests
    {
        [Fact]
        public void Constructor_ShouldThrowArgumentException_WhenParameterValueIsEmpty()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { "test", "" } };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new TestBaseCalculator(parameters));
            Assert.Contains("Parameter 'test' cannot be null or empty", exception.Message);
        }

        [Fact]
        public void Constructor_ShouldAcceptEmptyParameters()
        {
            // Arrange
            var parameters = new Dictionary<string, string>();

            // Act
            var calculator = new TestBaseCalculator(parameters);

            // Assert
            Assert.NotNull(calculator);
        }

        [Fact]
        public void Constructor_ShouldAddAllParametersToRequiredList()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.ShortPeriod), "value1" },
            };

            // Act
            var calculator = new TestBaseCalculator(parameters);
            var requiredParams = TestBaseCalculator.GetRequiredParamNames();

            // Assert
            Assert.Single(requiredParams);
            Assert.Contains(nameof(ParameterNamesEnum.ShortPeriod), requiredParams);
        }

        [Fact]
        public void ValidateRequiredParameters_ShouldNotThrow_WhenAllRequiredParametersPresent()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.ShortPeriod), "value" } };
            var calculator = new TestBaseCalculator(parameters);

            // Act & Assert
            var exception = Record.Exception(() => calculator.PublicValidateRequiredParameters());
            Assert.Null(exception);
        }

        [Fact]
        public void Calculate_ShouldThrowArgumentNullException_WhenPricesIsNull()
        {
            // Arrange
            var calculator = new TestBaseCalculator(new Dictionary<string, string> { { nameof(ParameterNamesEnum.ShortPeriod), "value1" } });
            double[,]? prices = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => calculator.Calculate(prices!));
            Assert.Equal("prices", exception.ParamName);
            Assert.Contains("Prices array cannot be null", exception.Message);
        }

        [Fact]
        public void Calculate_ShouldThrowArgumentException_WhenPricesDoesNotHaveSixColumns()
        {
            // Arrange
            var calculator = new TestBaseCalculator(new Dictionary<string, string> { { nameof(ParameterNamesEnum.ShortPeriod), "value1" } });
            double[,] prices = new double[3, 5]; // Only 5 columns instead of required 6

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => calculator.Calculate(prices));
            Assert.Contains("columns", exception.Message);
        }

        [Fact]
        public void Calculate_ShouldReturnResults_WhenPricesAreValid()
        {
            // Arrange
            var calculator = new TestBaseCalculator(new Dictionary<string, string> { { nameof(ParameterNamesEnum.ShortPeriod), "value1" } });
            double[,] prices = Pricedata.GetPrices(5);

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(nameof(CalculatorNameEnum.TEST), result.Name);
            Assert.IsType<Dictionary<long, KeyValuePair<string, double>[]>>(result.Results);
        }

        [Fact]
        public void Calculate_ShouldHandleEmptyPricesArray()
        {
            // Arrange
            var calculator = new TestBaseCalculator(new Dictionary<string, string> { { nameof(ParameterNamesEnum.ShortPeriod), "value1" } });
            double[,] prices = new double[0, 6];

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(nameof(CalculatorNameEnum.TEST), result.Name);
            Assert.Empty(result.Results);
        }

        [Fact]
        public void ValidatePeriod_ShouldThrowArgumentNullException_WhenParametersIsNull()
        {
            // Arrange
            var calculator = new TestBaseCalculator(new Dictionary<string, string> { { nameof(ParameterNamesEnum.ShortPeriod), "value1" } });
            double[,] prices = Pricedata.GetPrices(5);
            calculator.Calculate(prices, true); // Initialize the arrays
            Dictionary<string, string>? nullParameters = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(
                () => calculator.PublicValidatePeriod(nullParameters!));
            Assert.Equal("parameters", exception.ParamName);
            Assert.Contains("Parameters dictionary cannot be null", exception.Message);
        }

        [Fact]
        public void ValidatePeriod_ShouldThrowArgumentException_WhenPriceArrayTooShort()
        {
            // Arrange
            var calculator = new TestBaseCalculator(new Dictionary<string, string> { { nameof(ParameterNamesEnum.ShortPeriod), "value1" } });
            double[,] prices = Pricedata.GetPrices(3); // Only 3 data points
            calculator.Calculate(prices, true); // Initialize the arrays
            var parameters = new Dictionary<string, string> { { "period", "5" } }; // Period larger than array length

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(
                () => calculator.PublicValidatePeriod(parameters));
            Assert.Contains("Price array is too short for the given period", exception.Message);
        }

        [Fact]
        public void ValidatePeriod_ShouldNotThrow_WhenParametersValid()
        {
            // Arrange
            var calculator = new TestBaseCalculator(new Dictionary<string, string> { { nameof(ParameterNamesEnum.ShortPeriod), "value1" } });
            double[,] prices = Pricedata.GetPrices(10); // 10 data points
            calculator.Calculate(prices, true); // Initialize the arrays
            var parameters = new Dictionary<string, string> { { "period", "5" } }; // Period smaller than array length

            // Act & Assert
            var exception = Record.Exception(() => calculator.PublicValidatePeriod(parameters));
            Assert.Null(exception); // No exception should be thrown
        }

        [Fact]
        public void ValidatePeriod_ShouldHandleMissingPeriodParameter()
        {
            // Arrange
            var calculator = new TestBaseCalculator(new Dictionary<string, string> { { nameof(ParameterNamesEnum.ShortPeriod), "value1" } });
            double[,] prices = Pricedata.GetPrices(5);
            calculator.Calculate(prices, true); // Initialize the arrays
            var parameters = new Dictionary<string, string> { { "otherParam", "value" } }; // No period parameter

            // Act & Assert
            var exception = Record.Exception(() => calculator.PublicValidatePeriod(parameters));
            Assert.Null(exception); // No exception should be thrown
        }
    }
}
