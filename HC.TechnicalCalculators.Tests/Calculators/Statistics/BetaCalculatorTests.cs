using HC.TechnicalCalculators.Src.Calculators.Statistics;
using HC.TechnicalCalculators.Tests.Helpers;

namespace HC.TechnicalCalculators.Tests.Calculators.Statistics
{
    public class BetaCalculatorTests
    {

        [Fact]
        public void Calculate_ShouldThrowArgumentNullException_WhenPricesIsNull()
        {
            // Arrange
            var calculator = new BetaCalculator();
            double[,] prices = null;
            double[,] marketData = new double[3, 5];
            var parameters = new Dictionary<string, string> { { "period", "14" }, { "marketPrices", "true" } };

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => calculator.Calculate(prices, marketData, parameters));
            Assert.Equal("Prices array cannot be null. (Parameter 'prices')", exception.Message);
        }

        [Fact]
        public void Calculate_ShouldThrowArgumentNullException_WhenMarketDataIsNull()
        {
            // Arrange
            var calculator = new BetaCalculator();
            double[,] prices = new double[3, 5];
            double[,] marketData = null;
            var parameters = new Dictionary<string, string> { { "period", "14" }, { "marketPrices", "true" } };

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => calculator.Calculate(prices, marketData, parameters));
            Assert.Equal("Prices array cannot be null. (Parameter 'prices')", exception.Message);
        }

        [Fact]
        public void Calculate_ShouldThrowArgumentException_WhenMarketPricesIsNotProvided()
        {
            // Arrange
            var calculator = new BetaCalculator();
            double[,] prices = new double[3, 5];
            double[,] marketData = new double[3, 5];
            var parameters = new Dictionary<string, string> { { "period", "14" } };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => calculator.Calculate(prices, marketData, parameters));
            Assert.Equal("Parameter 'marketPrices' is required.", exception.Message);
        }

        [Fact]
        public void Calculate_ShouldThrowArgumentException_WhenPeriodIsNotProvided()
        {
            // Arrange
            var calculator = new BetaCalculator();
            double[,] prices = new double[3, 5];
            double[,] marketData = new double[3, 5];
            var parameters = new Dictionary<string, string> { { "marketPrices", "true" } };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => calculator.Calculate(prices, marketData, parameters));
            Assert.Equal("Parameter 'period' is required.", exception.Message);
        }

        [Fact]
        public void Calculate_ShouldThrowFormatException_WhenPeriodIsNotAnInteger()
        {
            // Arrange
            var calculator = new BetaCalculator();
            double[,] prices = new double[3, 5];
            double[,] marketData = new double[3, 5];
            var parameters = new Dictionary<string, string> { { "period", "invalid" }, { "marketPrices", "true" } };

            // Act & Assert
            Assert.Throws<FormatException>(() => calculator.Calculate(prices, marketData, parameters));
        }

        [Fact]
        public void Calculate_ShouldThrowArgumentException_WhenPeriodIsLessThanOrEqualToZero()
        {
            // Arrange
            var calculator = new BetaCalculator();
            double[,] prices = new double[3, 5];
            double[,] marketData = new double[3, 5];
            var parameters = new Dictionary<string, string> { { "period", "0" }, { "marketPrices", "true" } };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => calculator.Calculate(prices, marketData, parameters));
            Assert.Equal("Period must be greater than zero.", exception.Message);
        }

        [Fact]
        public void Calculate_ShouldThrowArgumentException_WhenStockPricesArrayIsTooShortForPeriod()
        {
            // Arrange
            var calculator = new BetaCalculator();
            double[,] prices = new double[3, 5];
            double[,] marketData = new double[10, 5]; // Market data is long enough
            var parameters = new Dictionary<string, string> { { "period", "5" }, { "marketPrices", "true" } };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => calculator.Calculate(prices, marketData, parameters));
            Assert.Equal("Price array is too short for the given period.", exception.Message);
        }

        [Fact]
        public void Calculate_ShouldThrowArgumentException_WhenMarketPricesArrayIsTooShortForPeriod()
        {
            // Arrange
            var calculator = new BetaCalculator();
            double[,] prices = new double[10, 5]; // Stock data is long enough
            double[,] marketData = new double[3, 5]; // Market data is too short
            var parameters = new Dictionary<string, string> { { "period", "5" }, { "marketPrices", "true" } };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => calculator.Calculate(prices, marketData, parameters));
            Assert.Equal("Price array is too short for the given period.", exception.Message);
        }

        [Fact]
        public void Calculate_ShouldReturnCorrectResults_WhenInputsAreValid()
        {
            // Arrange
            var calculator = new BetaCalculator();
            double[,] prices = Pricedata.GetPrices(20); // Stock prices
            double[,] marketData = Pricedata.GetPrices(20); // Market prices

            // Modify market data to make it different from stock prices
            for (int i = 0; i < marketData.GetLength(0); i++)
            {
                marketData[i, 3] = marketData[i, 3] * 1.1; // Slightly different close prices
            }

            var parameters = new Dictionary<string, string> { { "period", "10" }, { "marketPrices", "true" } };

            // Extract timestamps for verification
            long[] timestamps = new long[prices.GetLength(0)];
            for (int i = 0; i < prices.GetLength(0); i++)
            {
                timestamps[i] = (long)prices[i, 0];
            }

            // Act
            var result = calculator.Calculate(prices, marketData, parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("BETA", result.Name);
            Assert.Equal(prices.GetLength(0), result.Results.Count);

            // Check that all timestamps are present in results
            foreach (var timestamp in timestamps)
            {
                Assert.Contains(timestamp, result.Results.Keys);
            }

            // Check structure of results
            foreach (var entry in result.Results)
            {
                Assert.Single(entry.Value);
                Assert.Equal("BETA", entry.Value[0].Key);
                Assert.IsType<double>(entry.Value[0].Value);
            }
        }


        [Fact]
        public void Calculate_ShouldReturnBetaLessThanOne_WhenStockIsLessVolatileThanMarket()
        {
            // Arrange
            var calculator = new BetaCalculator();
            double[,] prices = Pricedata.GetPrices(30);
            double[,] marketData = new double[prices.GetLength(0), prices.GetLength(1)];
            Array.Copy(prices, marketData, marketData.Length);

            // Make market prices more volatile
            for (int i = 0; i < marketData.GetLength(0); i++)
            {
                // Amplify market price movements
                double baseClose = marketData[i, 3];
                marketData[i, 3] = baseClose * (1 + (i % 2 == 0 ? 0.1 : -0.1)); // 10% higher volatility
            }

            var parameters = new Dictionary<string, string> { { "period", "15" }, { "marketPrices", "true" } };

            // Act
            var result = calculator.Calculate(prices, marketData, parameters);

            // Assert
            // Skip the first few values while beta is stabilizing
            int skipCount = 20;
            bool foundLowerBeta = false;

            foreach (var timestamp in result.Results.Keys.Skip(skipCount))
            {
                var beta = result.Results[timestamp][0].Value;
                // For less volatile stock, beta should be < 1.0
                if (beta < 1.0)
                {
                    foundLowerBeta = true;
                    break;
                }
            }

            Assert.True(foundLowerBeta, "Expected to find beta values less than 1.0 for less volatile stock");
        }

        [Fact]
        public void Calculate_ShouldReturnConsistentResults_WhenCalledMultipleTimes()
        {
            // Arrange
            var calculator = new BetaCalculator();
            double[,] prices = Pricedata.GetPrices(20);
            double[,] marketData = Pricedata.GetPrices(20);
            var parameters = new Dictionary<string, string> { { "period", "10" }, { "marketPrices", "true" } };

            // Act
            var result1 = calculator.Calculate(prices, marketData, parameters);
            var result2 = calculator.Calculate(prices, marketData, parameters);

            // Assert
            Assert.Equal(result1.Results.Count, result2.Results.Count);

            foreach (var key in result1.Results.Keys)
            {
                Assert.Equal(result1.Results[key][0].Value, result2.Results[key][0].Value);
            }
        }

        [Fact]
        public void Calculate_ShouldNotModifyInputArrays()
        {
            // Arrange
            var calculator = new BetaCalculator();
            double[,] prices = Pricedata.GetPrices(20);
            double[,] marketData = Pricedata.GetPrices(20);

            // Create copies for comparison after calculation
            double[,] originalPrices = new double[prices.GetLength(0), prices.GetLength(1)];
            double[,] originalMarketData = new double[marketData.GetLength(0), marketData.GetLength(1)];
            Array.Copy(prices, originalPrices, prices.Length);
            Array.Copy(marketData, originalMarketData, marketData.Length);

            var parameters = new Dictionary<string, string> { { "period", "10" }, { "marketPrices", "true" } };

            // Act
            calculator.Calculate(prices, marketData, parameters);

            // Assert - Input arrays should remain unchanged
            for (int i = 0; i < prices.GetLength(0); i++)
            {
                for (int j = 0; j < prices.GetLength(1); j++)
                {
                    Assert.Equal(originalPrices[i, j], prices[i, j]);
                }
            }

            for (int i = 0; i < marketData.GetLength(0); i++)
            {
                for (int j = 0; j < marketData.GetLength(1); j++)
                {
                    Assert.Equal(originalMarketData[i, j], marketData[i, j]);
                }
            }
        }

        [Fact]
        public void Calculate_ShouldHandleDifferentPeriods()
        {
            // Arrange
            var calculator = new BetaCalculator();
            double[,] prices = Pricedata.GetPrices(30);
            double[,] marketData = Pricedata.GetPrices(30);

            var parametersPeriod5 = new Dictionary<string, string> { { "period", "5" }, { "marketPrices", "true" } };
            var parametersPeriod15 = new Dictionary<string, string> { { "period", "15" }, { "marketPrices", "true" } };

            // Act
            var resultPeriod5 = calculator.Calculate(prices, marketData, parametersPeriod5);
            var resultPeriod15 = calculator.Calculate(prices, marketData, parametersPeriod15);

            // Assert
            // Different periods should produce different results
            bool foundDifference = false;
            var commonTimestamps = resultPeriod5.Results.Keys.Intersect(resultPeriod15.Results.Keys);

            foreach (var timestamp in commonTimestamps)
            {
                if (Math.Abs(resultPeriod5.Results[timestamp][0].Value - resultPeriod15.Results[timestamp][0].Value) > 0.001)
                {
                    foundDifference = true;
                    break;
                }
            }

            Assert.True(foundDifference, "Expected different periods to produce different beta values");
        }
    }
}
