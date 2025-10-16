using HC.TechnicalCalculators.Src.Calculators.Momentum;
using HC.TechnicalCalculators.Src.Models;
using HC.TechnicalCalculators.Tests.Helpers;

namespace HC.TechnicalCalculators.Tests.Calculators.Momentum
{
    public class DmiCalculatorTests
    {

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenPeriodParameterIsPresent()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.Period), "14" }
            };

            // Act
            var calculator = new DmiCalculator(parameters, nameof(CalculatorNameEnum.DMI));

            // Assert
            Assert.NotNull(calculator);
        }

        [Fact]
        public void Calculate_ShouldThrowFormatException_WhenPeriodIsNotAnInteger()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.Period), "invalid" }
            };

            var exception = Assert.Throws<ArgumentException>(() => new DmiCalculator(parameters, nameof(CalculatorNameEnum.DMI)));
            Assert.Equal("Period must be between 2 and 100.", exception.Message);
        }

        [Fact]
        public void Calculate_ShouldReturnValidResults_WithTypicalParameters()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.Period), "14" }
            };
            var calculator = new DmiCalculator(parameters, nameof(CalculatorNameEnum.DMI));
            var prices = Pricedata.GetPrices(100); // Need enough data for meaningful calculation

            // Act
            var results = calculator.Calculate(prices);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(nameof(CalculatorNameEnum.DMI), results.Name);
            Assert.NotEmpty(results.Results);

            // Verify structure of results
            foreach (var result in results.Results)
            {
                Assert.Equal(3, result.Value.Length); // Should have PlusDI, MinusDI, and ADX values

                // Check if all required values are present
                Assert.Contains(result.Value, kv => kv.Key == nameof(TechnicalNamesEnum.PLUSDI));
                Assert.Contains(result.Value, kv => kv.Key == nameof(TechnicalNamesEnum.MINUSDI));
                Assert.Contains(result.Value, kv => kv.Key == nameof(TechnicalNamesEnum.ADX));

                // Values should be within expected ranges
                var plusDiValue = result.Value.First(kv => kv.Key == nameof(TechnicalNamesEnum.PLUSDI)).Value;
                var minusDiValue = result.Value.First(kv => kv.Key == nameof(TechnicalNamesEnum.MINUSDI)).Value;
                var adxValue = result.Value.First(kv => kv.Key == nameof(TechnicalNamesEnum.ADX)).Value;

                Assert.True(plusDiValue >= 0 && plusDiValue <= 100,
                    $"PlusDI value {plusDiValue} should be between 0 and 100");
                Assert.True(minusDiValue >= 0 && minusDiValue <= 100,
                    $"MinusDI value {minusDiValue} should be between 0 and 100");
                Assert.True(adxValue >= 0 && adxValue <= 100,
                    $"ADX value {adxValue} should be between 0 and 100");
            }
        }

        [Fact]
        public void Calculate_ShouldHandleLargeDataset()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.Period), "14" }
            };
            var calculator = new DmiCalculator(parameters, nameof(CalculatorNameEnum.DMI));
            var prices = Pricedata.GetPrices(500); // Large dataset

            // Act
            var results = calculator.Calculate(prices);

            // Assert
            Assert.NotNull(results);
            Assert.NotEmpty(results.Results);
        }

        [Fact]
        public void Calculate_ResultsShouldBeOrderedByTimestamp()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.Period), "14" }
            };
            var calculator = new DmiCalculator(parameters, nameof(CalculatorNameEnum.DMI));
            var prices = Pricedata.GetPrices(100);

            // Act
            var results = calculator.Calculate(prices);

            // Assert
            var timestamps = results.Results.Keys.ToList();
            var orderedTimestamps = timestamps.OrderByDescending(t => t).ToList();

            Assert.Equal(orderedTimestamps, timestamps);
        }

        [Fact]
        public void Calculate_WithUptrend_ShouldShowHigherPlusDI()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.Period), "5" }
            };
            var calculator = new DmiCalculator(parameters, nameof(CalculatorNameEnum.DMI));

            // Create synthetic uptrend data
            var prices = CreateUptrendPriceData(30);

            // Act
            var results = calculator.Calculate(prices, true);

            // Assert
            Assert.NotNull(results);
            Assert.NotEmpty(results.Results);

            // In an uptrend, PlusDI should generally be higher than MinusDI
            double totalPlusDI = 0;
            double totalMinusDI = 0;

            foreach (var result in results.Results)
            {
                var plusDiValue = result.Value.First(kv => kv.Key == nameof(TechnicalNamesEnum.PLUSDI)).Value;
                var minusDiValue = result.Value.First(kv => kv.Key == nameof(TechnicalNamesEnum.MINUSDI)).Value;

                totalPlusDI += plusDiValue;
                totalMinusDI += minusDiValue;
            }

            Assert.True(totalPlusDI > totalMinusDI,
                $"In an uptrend, total PlusDI ({totalPlusDI}) should be greater than total MinusDI ({totalMinusDI})");
        }

        [Fact]
        public void Calculate_WithDowntrend_ShouldShowHigherMinusDI()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.Period), "5" }
            };
            var calculator = new DmiCalculator(parameters, nameof(CalculatorNameEnum.DMI));

            // Create synthetic downtrend data
            var prices = CreateDowntrendPriceData(30);

            // Act
            var results = calculator.Calculate(prices, true);

            // Assert
            Assert.NotNull(results);
            Assert.NotEmpty(results.Results);

            // In a downtrend, MinusDI should generally be higher than PlusDI
            double totalPlusDI = 0;
            double totalMinusDI = 0;

            foreach (var result in results.Results)
            {
                var plusDiValue = result.Value.First(kv => kv.Key == nameof(TechnicalNamesEnum.PLUSDI)).Value;
                var minusDiValue = result.Value.First(kv => kv.Key == nameof(TechnicalNamesEnum.MINUSDI)).Value;

                totalPlusDI += plusDiValue;
                totalMinusDI += minusDiValue;
            }

            Assert.True(totalMinusDI > totalPlusDI,
                $"In a downtrend, total MinusDI ({totalMinusDI}) should be greater than total PlusDI ({totalPlusDI})");
        }

        // Helper methods for creating synthetic price data
        private double[,] CreateUptrendPriceData(int length)
        {
            double[,] prices = new double[length, 6]; // timestamp, open, high, low, close, volume

            for (int i = 0; i < length; i++)
            {
                double basePrice = 100 + i;
                prices[i, 0] = DateTimeOffset.Now.AddDays(i).ToUnixTimeSeconds(); // timestamp
                prices[i, 1] = basePrice; // open
                prices[i, 2] = basePrice + 2 + (i % 3); // high (increasing)
                prices[i, 3] = basePrice - 1; // low
                prices[i, 4] = basePrice + 1 + (i % 2); // close (increasing)
                prices[i, 5] = 1000 + (i * 10); // volume
            }

            return prices;
        }

        private double[,] CreateDowntrendPriceData(int length)
        {
            double[,] prices = new double[length, 6]; // timestamp, open, high, low, close, volume

            for (int i = 0; i < length; i++)
            {
                double basePrice = 200 - i;
                prices[i, 0] = DateTimeOffset.Now.AddDays(i).ToUnixTimeSeconds(); // timestamp
                prices[i, 1] = basePrice; // open
                prices[i, 2] = basePrice + 1; // high
                prices[i, 3] = basePrice - 2 - (i % 3); // low (decreasing)
                prices[i, 4] = basePrice - 1 - (i % 2); // close (decreasing)
                prices[i, 5] = 1000 + (i * 10); // volume
            }

            return prices;
        }
    }
}
