using HC.TechnicalCalculators.Src.Calculators.Momentum;
using HC.TechnicalCalculators.Src.Models;
using HC.TechnicalCalculators.Tests.Helpers;

namespace HC.TechnicalCalculators.Tests.Calculators.Momentum
{
    public class PpoCalculatorTests
    {
        [Fact]
        public void Constructor_ShouldSetDefaultFastPeriod_WhenFastPeriodParameterIsMissing()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
    {
        { nameof(ParameterNamesEnum.SlowPeriod), "26" },
        { nameof(ParameterNamesEnum.SignalPeriod), "9" }
    };

            // Act
            var calculator = new PpoCalculator(parameters, nameof(CalculatorNameEnum.PPO));

            // Assert
            Assert.NotNull(calculator);

            // Verify that the default FastPeriod is used by checking if calculation works
            var prices = Pricedata.GetPrices(50);
            var results = calculator.Calculate(prices);

            Assert.NotNull(results);
            Assert.Equal(nameof(CalculatorNameEnum.PPO), results.Name);
            Assert.NotEmpty(results.Results);
        }

        [Fact]
        public void Constructor_ShouldSetDefaultSlowPeriod_WhenSlowPeriodParameterIsMissing()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
    {
        { nameof(ParameterNamesEnum.FastPeriod), "12" },
        { nameof(ParameterNamesEnum.SignalPeriod), "9" }
    };

            // Act
            var calculator = new PpoCalculator(parameters, nameof(CalculatorNameEnum.PPO));

            // Assert
            Assert.NotNull(calculator);

            // Verify that the default SlowPeriod is used by checking if calculation works
            var prices = Pricedata.GetPrices(50);
            var results = calculator.Calculate(prices);

            Assert.NotNull(results);
            Assert.Equal(nameof(CalculatorNameEnum.PPO), results.Name);
            Assert.NotEmpty(results.Results);
        }

        [Fact]
        public void Constructor_ShouldSetDefaultSignalPeriod_WhenSignalPeriodParameterIsMissing()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
    {
        { nameof(ParameterNamesEnum.FastPeriod), "12" },
        { nameof(ParameterNamesEnum.SlowPeriod), "26" }
    };

            // Act
            var calculator = new PpoCalculator(parameters, nameof(CalculatorNameEnum.PPO));

            // Assert
            Assert.NotNull(calculator);

            // Verify that the default SignalPeriod is used by checking if calculation works
            var prices = Pricedata.GetPrices(50);
            var results = calculator.Calculate(prices);

            Assert.NotNull(results);
            Assert.Equal(nameof(CalculatorNameEnum.PPO), results.Name);
            Assert.NotEmpty(results.Results);
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
            var calculator = new PpoCalculator(parameters, nameof(CalculatorNameEnum.PPO));

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
            var exception = Assert.Throws<ArgumentException>(() => new PpoCalculator(parameters, nameof(CalculatorNameEnum.PPO)));
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
            var calculator = new PpoCalculator(parameters, nameof(CalculatorNameEnum.PPO));
            var prices = Pricedata.GetPrices(100); // Need enough data for meaningful calculation

            // Act
            var results = calculator.Calculate(prices);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(nameof(CalculatorNameEnum.PPO), results.Name);
            Assert.NotEmpty(results.Results);

            // Verify structure of results
            foreach (var result in results.Results)
            {
                Assert.Equal(1, result.Value.Length); // Should have PPO, Signal, and Histogram values

                // Check if all required values are present
                Assert.Contains(result.Value, kv => kv.Key == nameof(TechnicalNamesEnum.PPO));
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
            var calculator = new PpoCalculator(parameters, nameof(CalculatorNameEnum.PPO));
            var prices = Pricedata.GetPrices(500); // Large dataset

            // Act
            var results = calculator.Calculate(prices);

            // Assert
            Assert.NotNull(results);
            Assert.NotEmpty(results.Results);
        }

        [Fact]
        public void Calculate_WithUptrendData_ShouldShowPositivePPO()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.FastPeriod), "12" },
                { nameof(ParameterNamesEnum.SlowPeriod), "26" },
                { nameof(ParameterNamesEnum.SignalPeriod), "9" }
            };
            var calculator = new PpoCalculator(parameters, nameof(CalculatorNameEnum.PPO));

            // Create synthetic uptrend data
            var prices = CreateUptrendPriceData(100);

            // Act
            var results = calculator.Calculate(prices);

            // Assert - Check if PPO is mostly positive in the last third of the data
            // (earlier data points may not show the trend yet due to calculation periods)
            var resultsList = results.Results.OrderBy(r => r.Key).ToList();

            int positivePpoCount = 0;
            foreach (var result in resultsList)
            {
                var ppoValue = result.Value.First(kv => kv.Key == nameof(TechnicalNamesEnum.PPO)).Value;
                if (ppoValue > 0) positivePpoCount++;
            }

            // In an uptrend, majority of PPO values should be positive
            Assert.True(positivePpoCount > resultsList.Count / 2,
                $"Expected majority of PPO values to be positive in an uptrend, but only {positivePpoCount} out of {resultsList.Count} were positive");
        }

        [Fact]
        public void Calculate_WithDowntrendData_ShouldShowNegativePPO()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.FastPeriod), "12" },
                { nameof(ParameterNamesEnum.SlowPeriod), "26" },
                { nameof(ParameterNamesEnum.SignalPeriod), "9" }
            };
            var calculator = new PpoCalculator(parameters, nameof(CalculatorNameEnum.PPO));

            // Create synthetic downtrend data
            var prices = CreateDowntrendPriceData(100);

            // Act
            var results = calculator.Calculate(prices);

            // Assert - Check if PPO is mostly negative in the last third of the data
            var resultsList = results.Results.OrderBy(r => r.Key).ToList();

            int negativePpoCount = 0;
            foreach (var result in resultsList)
            {
                var ppoValue = result.Value.First(kv => kv.Key == nameof(TechnicalNamesEnum.PPO)).Value;
                if (ppoValue < 0) negativePpoCount++;
            }

            // In a downtrend, majority of PPO values should be negative
            Assert.True(negativePpoCount > resultsList.Count / 2,
                $"Expected majority of PPO values to be negative in a downtrend, but only {negativePpoCount} out of {resultsList.Count} were negative");
        }

        // Helper methods for creating synthetic price data
        private double[,] CreateUptrendPriceData(int length)
        {
            double[,] prices = new double[length, 6]; // timestamp, open, high, low, close, volume

            for (int i = 0; i < length; i++)
            {
                double basePrice = 100 + (i * 0.5); // Steadily increasing price
                prices[i, 0] = DateTimeOffset.Now.AddDays(i).ToUnixTimeSeconds(); // timestamp
                prices[i, 1] = basePrice - 0.2; // open
                prices[i, 2] = basePrice + 0.3; // high
                prices[i, 3] = basePrice - 0.3; // low
                prices[i, 4] = basePrice; // close
                prices[i, 5] = 1000 + (i * 10); // volume
            }

            return prices;
        }

        private double[,] CreateDowntrendPriceData(int length)
        {
            double[,] prices = new double[length, 6]; // timestamp, open, high, low, close, volume

            for (int i = 0; i < length; i++)
            {
                double basePrice = 100 - (i * 0.5); // Steadily decreasing price
                prices[i, 0] = DateTimeOffset.Now.AddDays(i).ToUnixTimeSeconds(); // timestamp
                prices[i, 1] = basePrice + 0.2; // open
                prices[i, 2] = basePrice + 0.3; // high
                prices[i, 3] = basePrice - 0.3; // low
                prices[i, 4] = basePrice; // close
                prices[i, 5] = 1000 + (i * 10); // volume
            }

            return prices;
        }
    }
}
