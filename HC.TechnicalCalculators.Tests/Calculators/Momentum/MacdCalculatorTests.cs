using HC.TechnicalCalculators.Src.Calculators.Momentum;
using HC.TechnicalCalculators.Src.Models;
using HC.TechnicalCalculators.Tests.Helpers;

namespace HC.TechnicalCalculators.Tests.Calculators.Momentum
{
    public class MacdCalculatorTests
    {
        [Fact]
        public void Constructor_ShouldThrowArgumentException_WhenFastPeriodParameterIsMissing()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.SlowPeriod), "26" },
                { nameof(ParameterNamesEnum.SignalPeriod), "9" }
            };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new MacdCalculator(parameters, nameof(CalculatorNameEnum.MACD)));
            Assert.Equal("Parameters FastPeriod, SlowPeriod, and SignalPeriod are required.", exception.Message);
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentException_WhenSlowPeriodParameterIsMissing()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.FastPeriod), "12" },
                { nameof(ParameterNamesEnum.SlowPeriod), "9" }
            };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new MacdCalculator(parameters, nameof(CalculatorNameEnum.MACD)));
            Assert.Equal("Parameters FastPeriod, SlowPeriod, and SignalPeriod are required.", exception.Message);
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentException_WhenSignalPeriodParameterIsMissing()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.FastPeriod), "12" },
                { nameof(ParameterNamesEnum.SlowPeriod), "26" }
            };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new MacdCalculator(parameters, nameof(CalculatorNameEnum.MACD)));
            Assert.Equal("Parameters FastPeriod, SlowPeriod, and SignalPeriod are required.", exception.Message);
        }

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenAllRequiredParametersArePresent()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.FastPeriod), "12" },
                { nameof(ParameterNamesEnum.SlowPeriod), "26" },
                { nameof(ParameterNamesEnum.SignalPeriod), "9" }
            };

            // Act
            var calculator = new MacdCalculator(parameters, nameof(CalculatorNameEnum.MACD));

            // Assert
            Assert.NotNull(calculator);
        }

        [Theory]
        [InlineData("0", "26", "9", "FastPeriod must be between 2 and 50.")]
        [InlineData("12", "0", "9", "SlowPeriod must be between 2 and 100.")]
        [InlineData("12", "26", "0", "SignalPeriod must be between 2 and 50.")]
        [InlineData("-1", "26", "9", "FastPeriod must be between 2 and 50.")]
        public void Calculate_ShouldThrowArgumentException_WhenPeriodsAreInvalid(string fastPeriod, string slowPeriod, string signalPeriod, string expectedMessage)
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.FastPeriod), fastPeriod },
                { nameof(ParameterNamesEnum.SlowPeriod), slowPeriod },
                { nameof(ParameterNamesEnum.SignalPeriod), signalPeriod }
            };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new MacdCalculator(parameters, nameof(CalculatorNameEnum.MACD)));
            Assert.Equal(expectedMessage, exception.Message);
        }

        [Fact]
        public void Calculate_ShouldReturnValidResults_WithTypicalParameters()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.FastPeriod), "12" },
                { nameof(ParameterNamesEnum.SlowPeriod), "26" },
                { nameof(ParameterNamesEnum.SignalPeriod), "9" }
            };
            var calculator = new MacdCalculator(parameters, nameof(CalculatorNameEnum.MACD));
            var prices = Pricedata.GetPrices(100); // Need enough data for meaningful calculation

            // Act
            var results = calculator.Calculate(prices);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(nameof(CalculatorNameEnum.MACD), results.Name);
            Assert.NotEmpty(results.Results);

            // Verify structure of results
            foreach (var result in results.Results)
            {
                Assert.Equal(3, result.Value.Length); // Should have MACD, Signal, and Histogram values

                // Check if all required values are present
                Assert.Contains(result.Value, kv => kv.Key == nameof(TechnicalNamesEnum.MACD));
                Assert.Contains(result.Value, kv => kv.Key == nameof(TechnicalNamesEnum.MACDSIGNAL));
                Assert.Contains(result.Value, kv => kv.Key == nameof(TechnicalNamesEnum.MACDHIST));

                // Get values for additional checks
                var macdValue = result.Value.First(kv => kv.Key == nameof(TechnicalNamesEnum.MACD)).Value;
                var signalValue = result.Value.First(kv => kv.Key == nameof(TechnicalNamesEnum.MACDSIGNAL)).Value;
                var histValue = result.Value.First(kv => kv.Key == nameof(TechnicalNamesEnum.MACDHIST)).Value;

                // Verify histogram calculation (MACD - Signal = Hist)
                Assert.Equal(macdValue - signalValue, histValue, 6); // 6 decimal places precision
            }
        }

        [Fact]
        public void Calculate_ShouldHandleLargeDataset()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.FastPeriod), "12" },
                { nameof(ParameterNamesEnum.SlowPeriod), "26" },
                { nameof(ParameterNamesEnum.SignalPeriod), "9" }
            };
            var calculator = new MacdCalculator(parameters, nameof(CalculatorNameEnum.MACD));
            var prices = Pricedata.GetPrices(500); // Large dataset

            // Act
            var results = calculator.Calculate(prices);

            // Assert
            Assert.NotNull(results);
            Assert.NotEmpty(results.Results);
        }
    }
}
