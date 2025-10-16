using HC.TechnicalCalculators.Src.Calculators.Momentum;
using HC.TechnicalCalculators.Src.Models;
using HC.TechnicalCalculators.Tests.Helpers;

namespace HC.TechnicalCalculators.Tests.Calculators.Momentum
{
    public class DmCalculatorTests
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
            var calculator = new DmCalculator(parameters, nameof(CalculatorNameEnum.DM));

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
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new DmCalculator(parameters, nameof(CalculatorNameEnum.DM)));
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
            var calculator = new DmCalculator(parameters, nameof(CalculatorNameEnum.DM));
            var prices = Pricedata.GetPrices(100); // Need enough data for meaningful calculation

            // Act
            var results = calculator.Calculate(prices);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(nameof(CalculatorNameEnum.DM), results.Name);
            Assert.NotEmpty(results.Results);

            // Verify structure of results
            foreach (var result in results.Results)
            {
                Assert.Equal(2, result.Value.Length); // Should have both PlusDM and MinusDM values
                Assert.Equal(nameof(TechnicalNamesEnum.PLUSDM), result.Value[0].Key);
                Assert.Equal(nameof(TechnicalNamesEnum.MINUSDM), result.Value[1].Key);
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
            var calculator = new DmCalculator(parameters, nameof(CalculatorNameEnum.DM));
            var prices = Pricedata.GetPrices(500); // Large dataset

            // Act
            var results = calculator.Calculate(prices);

            // Assert
            Assert.NotNull(results);
            Assert.NotEmpty(results.Results);
        }

        [Fact]
        public void Calculate_WithUptrend_ShouldShowHigherPlusDM()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.Period), "5" }
            };
            var calculator = new DmCalculator(parameters, nameof(CalculatorNameEnum.DM));

            // Create synthetic uptrend data
            var prices = CreateUptrendPriceData(30);

            // Act
            var results = calculator.Calculate(prices, true);

            // Assert
            Assert.NotNull(results);
            Assert.NotEmpty(results.Results);

            // In an uptrend, PlusDM should generally be higher than MinusDM
            double totalPlusDM = 0;
            double totalMinusDM = 0;

            foreach (var result in results.Results)
            {
                var plusDMValue = result.Value.First(kv => kv.Key == nameof(TechnicalNamesEnum.PLUSDM)).Value;
                var minusDMValue = result.Value.First(kv => kv.Key == nameof(TechnicalNamesEnum.MINUSDM)).Value;

                totalPlusDM += plusDMValue;
                totalMinusDM += minusDMValue;
            }

            Assert.True(totalPlusDM > totalMinusDM,
                $"In an uptrend, total PlusDM ({totalPlusDM}) should be greater than total MinusDM ({totalMinusDM})");
        }

        [Fact]
        public void Calculate_WithDowntrend_ShouldShowHigherMinusDM()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.Period), "5" }
            };
            var calculator = new DmCalculator(parameters, nameof(CalculatorNameEnum.DM));

            // Create synthetic downtrend data
            var prices = CreateDowntrendPriceData(30);

            // Act
            var results = calculator.Calculate(prices, true);

            // Assert
            Assert.NotNull(results);
            Assert.NotEmpty(results.Results);

            // In a downtrend, MinusDM should generally be higher than PlusDM
            double totalPlusDM = 0;
            double totalMinusDM = 0;

            foreach (var result in results.Results)
            {
                var plusDMValue = result.Value.First(kv => kv.Key == nameof(TechnicalNamesEnum.PLUSDM)).Value;
                var minusDMValue = result.Value.First(kv => kv.Key == nameof(TechnicalNamesEnum.MINUSDM)).Value;

                totalPlusDM += plusDMValue;
                totalMinusDM += minusDMValue;
            }

            Assert.True(totalMinusDM > totalPlusDM,
                $"In a downtrend, total MinusDM ({totalMinusDM}) should be greater than total PlusDM ({totalPlusDM})");
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
