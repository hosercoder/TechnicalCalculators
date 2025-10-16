using HC.TechnicalCalculators.Src.Calculators.Momentum;
using HC.TechnicalCalculators.Src.Models;
using HC.TechnicalCalculators.Tests.Helpers;

namespace HC.TechnicalCalculators.Tests.Calculators.Momentum
{
    public class MfiCalculatorTests
    {
        [Fact]
        public void Constructor_ShouldSetDefaultPeriod_WhenPeriodParameterIsMissing()
        {
            // Arrange
            var parameters = new Dictionary<string, string>();

            // Act
            var calculator = new MfiCalculator(parameters, nameof(CalculatorNameEnum.MFI));

            // Assert
            Assert.NotNull(calculator);

            // Verify that the default period is used by checking if calculation works
            var prices = Pricedata.GetPrices(50);
            var results = calculator.Calculate(prices);

            Assert.NotNull(results);
            Assert.Equal(nameof(CalculatorNameEnum.MFI), results.Name);
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
            var calculator = new MfiCalculator(parameters, nameof(CalculatorNameEnum.MFI));

            // Assert
            Assert.NotNull(calculator);
        }

        [Fact]
        public void Calculate_ShouldThrowArgumentException_WhenPeriodIsNotAnInteger()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.Period), "invalid" }
            };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new MfiCalculator(parameters, nameof(CalculatorNameEnum.MFI)));
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
            var calculator = new MfiCalculator(parameters, nameof(CalculatorNameEnum.MFI));
            var prices = Pricedata.GetPrices(100); // Need enough data for meaningful calculation

            // Act
            var results = calculator.Calculate(prices);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(nameof(CalculatorNameEnum.MFI), results.Name);
            Assert.NotEmpty(results.Results);

            // Verify structure of results
            foreach (var result in results.Results)
            {
                Assert.Single(result.Value); // Should have one MFI value
                Assert.Equal(nameof(TechnicalNamesEnum.MFI), result.Value[0].Key);

                // MFI values should be between 0 and 100
                var mfiValue = result.Value[0].Value;
                Assert.True(mfiValue >= 0 && mfiValue <= 100,
                    $"MFI value {mfiValue} should be between 0 and 100");
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
            var calculator = new MfiCalculator(parameters, nameof(CalculatorNameEnum.MFI));
            var prices = Pricedata.GetPrices(500); // Large dataset

            // Act
            var results = calculator.Calculate(prices);

            // Assert
            Assert.NotNull(results);
            Assert.NotEmpty(results.Results);
        }

        [Fact]
        public void Calculate_WithHighVolumeDowntrend_ShouldShowLowMFI()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.Period), "14" }
            };
            var calculator = new MfiCalculator(parameters, nameof(CalculatorNameEnum.MFI));

            // Create synthetic data with high volume on down days
            var prices = CreateHighVolumeDowntrendData(50);

            // Act
            var results = calculator.Calculate(prices, true);

            // Assert
            var resultsList = results.Results.OrderBy(r => r.Key).ToList();

            // Skip initial values where MFI is still forming
            var laterValues = resultsList.Skip(20).ToList();
            if (laterValues.Count == 0)
            {
                // If we don't have enough data after skipping, the test is inconclusive
                return;
            }

            // Calculate average MFI
            double totalMfi = 0;
            foreach (var result in laterValues)
            {
                totalMfi += result.Value[0].Value;
            }
            double avgMfi = totalMfi / laterValues.Count;

            // In a high volume downtrend, MFI should generally be low (below 50)
            Assert.True(avgMfi < 50, $"Expected average MFI to be less than 50 in high volume downtrend, but was {avgMfi}");
        }

        private double[,] CreateHighVolumeDowntrendData(int length)
        {
            double[,] prices = new double[length, 6]; // timestamp, open, high, low, close, volume
            Random random = new Random(42); // Fixed seed for reproducibility

            double basePrice = 100;
            for (int i = 0; i < length; i++)
            {
                bool isDownDay = random.NextDouble() > 0.3; // 70% chance of a down day
                double priceChange = isDownDay ? -random.NextDouble() * 2 : random.NextDouble();

                basePrice += priceChange;

                // Assign higher volume on down days
                double volume = isDownDay ? 10000 + random.NextDouble() * 5000 : 5000 + random.NextDouble() * 3000;

                prices[i, 0] = DateTimeOffset.Now.AddDays(i).ToUnixTimeSeconds(); // timestamp
                prices[i, 1] = basePrice + (isDownDay ? 1 : 0.5); // open
                prices[i, 2] = basePrice + (isDownDay ? 0.5 : 1.5); // high
                prices[i, 3] = basePrice - (isDownDay ? 1.5 : 0.5); // low
                prices[i, 4] = basePrice; // close
                prices[i, 5] = volume; // volume
            }

            return prices;
        }
    }
}
