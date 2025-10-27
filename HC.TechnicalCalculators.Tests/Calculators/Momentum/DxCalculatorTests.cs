using HC.TechnicalCalculators.Src.Calculators.Momentum;
using HC.TechnicalCalculators.Src.Models;
using HC.TechnicalCalculators.Tests.Helpers;

namespace HC.TechnicalCalculators.Tests.Calculators.Momentum
{
    public class DxCalculatorTests
    {
        [Fact]
        public void Constructor_ShouldCreateInstance_WhenPeriodParameterIsMissing()
        {
            // Arrange
            var parameters = new Dictionary<string, string>();

            // Act
            var calculator = new DxCalculator(parameters, nameof(CalculatorNameEnum.DX));

            // Assert
            Assert.NotNull(calculator);
            // The calculator should set the default period value of 14
        }

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenPeriodParameterIsPresent()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.Period), "14" }
            };

            // Act
            var calculator = new DxCalculator(parameters, nameof(CalculatorNameEnum.DX));

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
            var exception = Assert.Throws<ArgumentException>(() => new DxCalculator(parameters, nameof(CalculatorNameEnum.DX)));
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
            var calculator = new DxCalculator(parameters, nameof(CalculatorNameEnum.DX));
            var prices = Pricedata.GetPrices(100); // Need enough data for meaningful calculation

            // Act
            var results = calculator.Calculate(prices);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(nameof(CalculatorNameEnum.DX), results.Name);
            Assert.NotEmpty(results.Results);

            // Verify structure of results
            foreach (var result in results.Results)
            {
                Assert.Single(result.Value); // Should have one DX value
                Assert.Equal(nameof(TechnicalNamesEnum.DX), result.Value[0].Key);

                // DX values should be between 0 and 100
                var dxValue = result.Value[0].Value;
                Assert.True(dxValue >= 0 && dxValue <= 100,
                    $"DX value {dxValue} should be between 0 and 100");
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
            var calculator = new DxCalculator(parameters, nameof(CalculatorNameEnum.DX));
            var prices = Pricedata.GetPrices(500); // Large dataset

            // Act
            var results = calculator.Calculate(prices);

            // Assert
            Assert.NotNull(results);
            Assert.NotEmpty(results.Results);
        }

        [Fact]
        public void Calculate_WithTrendyData_ShouldShowHigherDXValues()
        {
            // Arrange - Create data with strong trends
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.Period), "14" }
            };
            var calculator = new DxCalculator(parameters, nameof(CalculatorNameEnum.DX));

            // Create synthetic trendy data - alternating uptrend and downtrend
            var trendyPrices = CreateTrendyPriceData(100);
            var sidewaysPrices = CreateSidewaysPriceData(100);

            // Act
            var trendyResults = calculator.Calculate(trendyPrices);
            var sidewaysResults = calculator.Calculate(sidewaysPrices);

            // Assert
            Assert.NotNull(trendyResults);
            Assert.NotEmpty(trendyResults.Results);
            Assert.NotNull(sidewaysResults);
            Assert.NotEmpty(sidewaysResults.Results);

            // Calculate average DX values
            double avgTrendyDX = trendyResults.Results
                .Select(r => r.Value[0].Value)
                .Where(v => !double.IsNaN(v)) // Filter out NaN values
                .Average();

            double avgSidewaysDX = sidewaysResults.Results
                .Select(r => r.Value[0].Value)
                .Where(v => !double.IsNaN(v)) // Filter out NaN values
                .Average();

            // Trendy data should show higher DX values compared to sideways data
            Assert.True(avgTrendyDX > avgSidewaysDX,
                $"Average DX for trendy data ({avgTrendyDX}) should be higher than sideways data ({avgSidewaysDX})");
        }

        // Helper methods for creating synthetic price data
        private double[,] CreateTrendyPriceData(int length)
        {
            double[,] prices = new double[length, 6]; // timestamp, open, high, low, close, volume
            double basePrice = 100;
            bool isUptrend = true;

            for (int i = 0; i < length; i++)
            {
                // Switch trend direction every 20 bars
                if (i % 20 == 0 && i > 0)
                {
                    isUptrend = !isUptrend;
                }

                double trend = isUptrend ? 2 : -2;
                basePrice += trend;

                prices[i, 0] = DateTimeOffset.Now.AddDays(i).ToUnixTimeSeconds(); // timestamp
                prices[i, 1] = basePrice - 1; // open
                prices[i, 2] = isUptrend ? basePrice + 2 : basePrice + 0.5; // high
                prices[i, 3] = isUptrend ? basePrice - 0.5 : basePrice - 2; // low
                prices[i, 4] = basePrice; // close
                prices[i, 5] = 1000 + (i * 10); // volume
            }

            return prices;
        }

        private double[,] CreateSidewaysPriceData(int length)
        {
            double[,] prices = new double[length, 6]; // timestamp, open, high, low, close, volume
            double basePrice = 100;
            Random random = new Random(42); // Fixed seed for reproducibility

            for (int i = 0; i < length; i++)
            {
                // Random small movements around base price
                double variation = (random.NextDouble() - 0.5) * 2; // -1 to +1

                prices[i, 0] = DateTimeOffset.Now.AddDays(i).ToUnixTimeSeconds(); // timestamp
                prices[i, 1] = basePrice + variation; // open
                prices[i, 2] = basePrice + variation + random.NextDouble(); // high
                prices[i, 3] = basePrice + variation - random.NextDouble(); // low
                prices[i, 4] = basePrice + variation + (random.NextDouble() - 0.5); // close
                prices[i, 5] = 1000 + (random.NextDouble() * 200); // volume
            }

            return prices;
        }
    }
}
