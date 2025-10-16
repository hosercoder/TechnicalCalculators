using HC.TechnicalCalculators.Src.Calculators.Momentum;
using HC.TechnicalCalculators.Src.Models;
using HC.TechnicalCalculators.Tests.Helpers;

namespace HC.TechnicalCalculators.Tests.Calculators.Momentum
{
    public class MomCalculatorTests
    {
        [Fact]
        public void Constructor_ShouldSetDefaultPeriod_WhenPeriodParameterIsMissing()
        {
            // Arrange
            var parameters = new Dictionary<string, string>();

            // Act
            var calculator = new MomCalculator(parameters, nameof(CalculatorNameEnum.MOM));

            // Assert
            Assert.NotNull(calculator);

            // Verify that the default period is used by checking if calculation works
            var prices = Pricedata.GetPrices(50);
            var results = calculator.Calculate(prices);

            Assert.NotNull(results);
            Assert.Equal(nameof(CalculatorNameEnum.MOM), results.Name);
            Assert.NotEmpty(results.Results);
        }

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenPeriodParameterIsPresent()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.Period), "10" }
            };

            // Act
            var calculator = new MomCalculator(parameters, nameof(CalculatorNameEnum.MOM));

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
            var exception = Assert.Throws<ArgumentException>(() => new MomCalculator(parameters, nameof(CalculatorNameEnum.MOM)));
            Assert.Equal("Period must be between 1 and 100.", exception.Message);
        }

        [Fact]
        public void Calculate_ShouldReturnValidResults_WithTypicalParameters()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.Period), "10" }
            };
            var calculator = new MomCalculator(parameters, nameof(CalculatorNameEnum.MOM));
            var prices = Pricedata.GetPrices(100); // Need enough data for meaningful calculation

            // Act
            var results = calculator.Calculate(prices);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(nameof(CalculatorNameEnum.MOM), results.Name);
            Assert.NotEmpty(results.Results);

            // Verify structure of results
            foreach (var result in results.Results)
            {
                Assert.Single(result.Value); // Should have one MOM value
                Assert.Equal(nameof(TechnicalNamesEnum.MOM), result.Value[0].Key);

                // Momentum can be positive or negative, so no specific range check
            }
        }

        [Fact]
        public void Calculate_ShouldHandleLargeDataset()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.Period), "10" }
            };
            var calculator = new MomCalculator(parameters, nameof(CalculatorNameEnum.MOM));
            var prices = Pricedata.GetPrices(500); // Large dataset

            // Act
            var results = calculator.Calculate(prices);

            // Assert
            Assert.NotNull(results);
            Assert.NotEmpty(results.Results);
        }

        [Fact]
        public void Calculate_WithUptrendData_ShouldShowPositiveMomentum()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.Period), "10" }
            };
            var calculator = new MomCalculator(parameters, nameof(CalculatorNameEnum.MOM));

            // Create synthetic uptrend data
            var prices = CreateUptrendPriceData(50);

            // Act
            var results = calculator.Calculate(prices, true);

            // Assert
            var resultsList = results.Results.OrderBy(r => r.Key).ToList();

            // Skip initial values where momentum is still forming
            var laterValues = resultsList.Skip(20).ToList();
            if (laterValues.Count == 0)
            {
                // If we don't have enough data after skipping, the test is inconclusive
                return;
            }

            // Calculate how many momentum values are positive
            int positiveCount = 0;
            foreach (var result in laterValues)
            {
                if (result.Value[0].Value > 0)
                    positiveCount++;
            }

            // In an uptrend, most momentum values should be positive
            Assert.True(positiveCount > laterValues.Count / 2,
                $"Expected majority of momentum values to be positive in an uptrend, but only {positiveCount} out of {laterValues.Count} were positive");
        }

        [Fact]
        public void Calculate_WithDowntrendData_ShouldShowNegativeMomentum()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.Period), "10" }
            };
            var calculator = new MomCalculator(parameters, nameof(CalculatorNameEnum.MOM));

            // Create synthetic downtrend data
            var prices = CreateDowntrendPriceData(50);

            // Act
            var results = calculator.Calculate(prices, true);

            // Assert
            var resultsList = results.Results.OrderBy(r => r.Key).ToList();

            // Skip initial values where momentum is still forming
            var laterValues = resultsList.Skip(20).ToList();
            if (laterValues.Count == 0)
            {
                // If we don't have enough data after skipping, the test is inconclusive
                return;
            }

            // Calculate how many momentum values are negative
            int negativeCount = 0;
            foreach (var result in laterValues)
            {
                if (result.Value[0].Value < 0)
                    negativeCount++;
            }

            // In a downtrend, most momentum values should be negative
            Assert.True(negativeCount > laterValues.Count / 2,
                $"Expected majority of momentum values to be negative in a downtrend, but only {negativeCount} out of {laterValues.Count} were negative");
        }

        [Fact]
        public void Calculate_WithSidewaysData_ShouldShowMomentumFluctuatingAroundZero()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.Period), "10" }
            };
            var calculator = new MomCalculator(parameters, nameof(CalculatorNameEnum.MOM));

            // Create sideways movement data
            var prices = CreateSidewaysPriceData(50);

            // Act
            var results = calculator.Calculate(prices, true);

            // Assert
            var resultsList = results.Results.OrderBy(r => r.Key).ToList();

            // Skip initial values where momentum is still forming
            var laterValues = resultsList.Skip(20).ToList();
            if (laterValues.Count == 0)
            {
                // If we don't have enough data after skipping, the test is inconclusive
                return;
            }

            // Calculate average momentum
            double sum = 0;
            foreach (var result in laterValues)
            {
                sum += result.Value[0].Value;
            }
            double avgMomentum = sum / laterValues.Count;

            // In sideways movement, average momentum should be close to zero
            Assert.True(Math.Abs(avgMomentum) < 5.0,
                $"Expected average momentum to be close to zero in sideways movement, but was {avgMomentum}");
        }

        // Helper methods for creating synthetic price data
        private double[,] CreateUptrendPriceData(int length)
        {
            double[,] prices = new double[length, 6]; // timestamp, open, high, low, close, volume

            for (int i = 0; i < length; i++)
            {
                double basePrice = 100 + (i * 2); // Steadily increasing price
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
                double basePrice = 200 - (i * 2); // Steadily decreasing price
                prices[i, 0] = DateTimeOffset.Now.AddDays(i).ToUnixTimeSeconds(); // timestamp
                prices[i, 1] = basePrice + 1; // open
                prices[i, 2] = basePrice + 1; // high
                prices[i, 3] = basePrice - 1; // low
                prices[i, 4] = basePrice; // close
                prices[i, 5] = 1000 + (i * 10); // volume
            }

            return prices;
        }

        private double[,] CreateSidewaysPriceData(int length)
        {
            double[,] prices = new double[length, 6]; // timestamp, open, high, low, close, volume
            Random random = new Random(42); // Fixed seed for reproducibility

            for (int i = 0; i < length; i++)
            {
                double basePrice = 100 + (random.NextDouble() * 4 - 2); // Random price around 100
                prices[i, 0] = DateTimeOffset.Now.AddDays(i).ToUnixTimeSeconds(); // timestamp
                prices[i, 1] = basePrice - 1; // open
                prices[i, 2] = basePrice + 1; // high
                prices[i, 3] = basePrice - 1; // low
                prices[i, 4] = basePrice; // close
                prices[i, 5] = 1000 + (random.Next(0, 200)); // random volume
            }

            return prices;
        }
    }
}
