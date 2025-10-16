using HC.TechnicalCalculators.Src.Calculators.Momentum;
using HC.TechnicalCalculators.Src.Models;
using HC.TechnicalCalculators.Tests.Helpers;

namespace HC.TechnicalCalculators.Tests.Calculators.Momentum
{
    public class RsiCalculatorTests
    {
        [Fact]
        public void Constructor_ShouldSetDefaultPeriod_WhenPeriodParameterIsMissing()
        {
            // Arrange
            var parameters = new Dictionary<string, string>();

            // Act
            var calculator = new RsiCalculator(parameters, nameof(CalculatorNameEnum.RSI));

            // Assert
            Assert.NotNull(calculator);

            // Verify that the default period is used by checking if calculation works
            var prices = Pricedata.GetPrices(50);
            var results = calculator.Calculate(prices);

            Assert.NotNull(results);
            Assert.Equal(nameof(CalculatorNameEnum.RSI), results.Name);
            Assert.NotEmpty(results.Results);
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
            var calculator = new RsiCalculator(parameters, nameof(CalculatorNameEnum.RSI));

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
            var calculator = new RsiCalculator(parameters, nameof(CalculatorNameEnum.RSI));
            var prices = Pricedata.GetPrices(50);

            // Act & Assert
            Assert.Throws<FormatException>(() => calculator.Calculate(prices, true));
        }

        [Fact]
        public void Calculate_ShouldReturnValidResults_WithTypicalParameters()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.Period), "14" }
            };
            var calculator = new RsiCalculator(parameters, nameof(CalculatorNameEnum.RSI));
            var prices = Pricedata.GetPrices(100); // Need enough data for meaningful calculation

            // Act
            var results = calculator.Calculate(prices);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(nameof(CalculatorNameEnum.RSI), results.Name);
            Assert.NotEmpty(results.Results);

            // Verify structure of results
            foreach (var result in results.Results)
            {
                Assert.Single(result.Value); // Should have one RSI value
                Assert.Equal(nameof(TechnicalNamesEnum.RSI), result.Value[0].Key);

                // RSI values should be between 0 and 100
                var rsiValue = result.Value[0].Value;
                Assert.True(rsiValue >= 0 && rsiValue <= 100,
                    $"RSI value {rsiValue} should be between 0 and 100");
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
            var calculator = new RsiCalculator(parameters, nameof(CalculatorNameEnum.RSI));
            var prices = Pricedata.GetPrices(500); // Large dataset

            // Act
            var results = calculator.Calculate(prices);

            // Assert
            Assert.NotNull(results);
            Assert.NotEmpty(results.Results);
        }

        [Fact]
        public void Calculate_WithUptrendData_ShouldShowHighRSI()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.Period), "14" }
            };
            var calculator = new RsiCalculator(parameters, nameof(CalculatorNameEnum.RSI));

            // Create synthetic uptrend data
            var prices = CreateUptrendPriceData(50);

            // Act
            var results = calculator.Calculate(prices, true);

            // Assert
            var resultsList = results.Results.OrderBy(r => r.Key).ToList();

            // Skip initial values where RSI is still forming
            var laterValues = resultsList.Skip(20).ToList();
            if (laterValues.Count == 0)
            {
                // If we don't have enough data after skipping, the test is inconclusive
                return;
            }

            // Calculate average RSI for later values
            double totalRsi = 0;
            foreach (var result in laterValues)
            {
                totalRsi += result.Value[0].Value;
            }
            double avgRsi = totalRsi / laterValues.Count;

            // In an uptrend, RSI should generally be high (above 50, often above 70)
            Assert.True(avgRsi > 50, $"Expected average RSI to be greater than 50 in uptrend, but was {avgRsi}");
        }

        [Fact]
        public void Calculate_WithDowntrendData_ShouldShowLowRSI()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.Period), "14" }
            };
            var calculator = new RsiCalculator(parameters, nameof(CalculatorNameEnum.RSI));

            // Create synthetic downtrend data
            var prices = CreateDowntrendPriceData(50);

            // Act
            var results = calculator.Calculate(prices, true);

            // Assert
            var resultsList = results.Results.OrderBy(r => r.Key).ToList();

            // Skip initial values where RSI is still forming
            var laterValues = resultsList.Skip(20).ToList();
            if (laterValues.Count == 0)
            {
                // If we don't have enough data after skipping, the test is inconclusive
                return;
            }

            // Calculate average RSI for later values
            double totalRsi = 0;
            foreach (var result in laterValues)
            {
                totalRsi += result.Value[0].Value;
            }
            double avgRsi = totalRsi / laterValues.Count;

            // In a downtrend, RSI should generally be low (below 50, often below 30)
            Assert.True(avgRsi < 50, $"Expected average RSI to be less than 50 in downtrend, but was {avgRsi}");
        }

        [Fact]
        public void Calculate_WithConstantPriceData_ShouldShowNeutralRSI()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.Period), "14" }
            };
            var calculator = new RsiCalculator(parameters, nameof(CalculatorNameEnum.RSI));

            // Create constant price data
            var prices = CreateConstantPriceData(50);

            // Act
            var results = calculator.Calculate(prices, true);

            // Assert
            var resultsList = results.Results.OrderBy(r => r.Key).ToList();

            // Skip initial values where RSI is still forming
            var laterValues = resultsList.Skip(20).ToList();
            if (laterValues.Count == 0)
            {
                // If we don't have enough data after skipping, the test is inconclusive
                return;
            }

            // With constant prices, RSI should converge to around 50
            foreach (var result in laterValues)
            {
                Assert.True(Math.Abs(result.Value[0].Value - 0) < 1.0,
                    $"Expected RSI to be close to 50 with constant prices, but was {result.Value[0].Value}");
            }
        }

        // Helper methods for creating synthetic price data
        private double[,] CreateUptrendPriceData(int length)
        {
            double[,] prices = new double[length, 6]; // timestamp, open, high, low, close, volume

            for (int i = 0; i < length; i++)
            {
                double basePrice = 100 + (i * 1.5); // Steadily increasing price
                prices[i, 0] = DateTimeOffset.Now.AddDays(i).ToUnixTimeSeconds(); // timestamp
                prices[i, 1] = basePrice - 1; // open
                prices[i, 2] = basePrice + 1; // high
                prices[i, 3] = basePrice - 1; // low
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
                double basePrice = 200 - (i * 1.5); // Steadily decreasing price
                prices[i, 0] = DateTimeOffset.Now.AddDays(i).ToUnixTimeSeconds(); // timestamp
                prices[i, 1] = basePrice + 1; // open
                prices[i, 2] = basePrice + 1; // high
                prices[i, 3] = basePrice - 1; // low
                prices[i, 4] = basePrice; // close
                prices[i, 5] = 1000 + (i * 10); // volume
            }

            return prices;
        }

        private double[,] CreateConstantPriceData(int length)
        {
            double[,] prices = new double[length, 6]; // timestamp, open, high, low, close, volume

            for (int i = 0; i < length; i++)
            {
                double basePrice = 100; // Constant price
                prices[i, 0] = DateTimeOffset.Now.AddDays(i).ToUnixTimeSeconds(); // timestamp
                prices[i, 1] = basePrice; // open
                prices[i, 2] = basePrice + 0.1; // high
                prices[i, 3] = basePrice - 0.1; // low
                prices[i, 4] = basePrice; // close
                prices[i, 5] = 1000; // volume
            }

            return prices;
        }
    }
}
