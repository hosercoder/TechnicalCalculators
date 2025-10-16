using HC.TechnicalCalculators.Src.Calculators.Overlap;
using HC.TechnicalCalculators.Src.Models;
using HC.TechnicalCalculators.Tests.Helpers;

namespace HC.TechnicalCalculators.Tests.Calculators.Overlap
{
    public class WeightedMovingAverageTests
    {
        [Fact]
        public void Constructor_ShouldThrowArgumentException_WhenPeriodParameterIsMissing()
        {
            // Arrange
            var parameters = new Dictionary<string, string>();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new WeightedMovingAverage(parameters, nameof(CalculatorNameEnum.WMA)));
            Assert.Equal("Parameter Period is required.", exception.Message);
        }

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenPeriodParameterIsPresent()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "5" } };

            // Act
            var calculator = new WeightedMovingAverage(parameters, nameof(CalculatorNameEnum.WMA));

            // Assert
            Assert.NotNull(calculator);
        }

        [Fact]
        public void Calculate_ShouldThrowFormatException_WhenPeriodIsNotAnInteger()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "invalid" } };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new WeightedMovingAverage(parameters, nameof(CalculatorNameEnum.WMA)));
            Assert.Contains("Period must be between 2 and 200.", exception.Message);
        }

        [Fact]
        public void Calculate_ShouldThrowArgumentNullException_WhenPricesIsNull()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "5" } };
            var calculator = new WeightedMovingAverage(parameters, nameof(CalculatorNameEnum.WMA));
            double[,] prices = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => calculator.Calculate(prices));
            Assert.Contains("prices", exception.Message);
        }

        [Fact]
        public void Calculate_ShouldReturnCorrectResults_WhenParametersAreValid()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "5" } };
            var calculator = new WeightedMovingAverage(parameters, nameof(CalculatorNameEnum.WMA));
            double[,] prices = Pricedata.GetPrices(15); // Use enough data points

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(nameof(CalculatorNameEnum.WMA), result.Name);
            Assert.True(result.Results.Count > 0);

            // Check the structure of results
            foreach (var timestamp in result.Results.Keys)
            {
                var values = result.Results[timestamp];
                Assert.Single(values);
                Assert.Equal(nameof(TechnicalNamesEnum.MOVINGAVERAGE), values[0].Key);
                Assert.IsType<double>(values[0].Value);
            }
        }

        [Fact]
        public void Calculate_ShouldProduceValuesWithinRangeOfInputPrices()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "5" } };
            var calculator = new WeightedMovingAverage(parameters, nameof(CalculatorNameEnum.WMA));
            double[,] prices = Pricedata.GetPrices(15);

            // Find min and max close prices
            double minClose = double.MaxValue;
            double maxClose = double.MinValue;

            for (int i = 0; i < prices.GetLength(0); i++)
            {
                double closePrice = prices[i, 4];
                minClose = Math.Min(minClose, closePrice);
                maxClose = Math.Max(maxClose, closePrice);
            }

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert
            // Each WMA value should be within the range of input prices
            foreach (var value in result.Results.Values)
            {
                double wma = value[0].Value;
                Assert.InRange(wma, minClose, maxClose);
            }
        }

        [Fact]
        public void Calculate_DifferentPeriods_ShouldProduceDifferentResults()
        {
            // Arrange
            var parameters1 = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "3" } };
            var parameters2 = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "5" } };

            var calculator1 = new WeightedMovingAverage(parameters1, nameof(CalculatorNameEnum.WMA));
            var calculator2 = new WeightedMovingAverage(parameters2, nameof(CalculatorNameEnum.WMA));

            double[,] prices = Pricedata.GetPrices(15);

            // Act
            var result1 = calculator1.Calculate(prices, true);
            var result2 = calculator2.Calculate(prices, true);

            // Assert
            // Different periods should produce different numbers of results
            Assert.NotEqual(result1.Results.Count, result2.Results.Count);

            // Common timestamps should have different values
            var commonTimestamps = result1.Results.Keys.Intersect(result2.Results.Keys);
            if (commonTimestamps.Any())
            {
                var commonTimestamp = commonTimestamps.First();
                var wma1 = result1.Results[commonTimestamp][0].Value;
                var wma2 = result2.Results[commonTimestamp][0].Value;

                // Different periods should produce different WMA values
                Assert.NotEqual(wma1, wma2);
            }
        }

        [Fact]
        public void Calculate_WithConstantPrices_ShouldReturnConstantWMA()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "5" } };
            var calculator = new WeightedMovingAverage(parameters, nameof(CalculatorNameEnum.WMA));

            // Create a price array with constant values
            double[,] prices = new double[10, 6];
            double constantPrice = 100;

            // Set timestamps and constant close prices
            for (int i = 0; i < 10; i++)
            {
                prices[i, 0] = 1000 + i;  // Timestamps
                prices[i, 4] = constantPrice;  // All close prices = 100
            }

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert
            // All WMA values should equal the constant price
            foreach (var value in result.Results.Values)
            {
                Assert.Equal(constantPrice, value[0].Value, 6); // Compare with 6 decimal precision
            }
        }

        [Fact]
        public void Calculate_ResultsShouldBeOrderedByTimestamp()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "5" } };
            var calculator = new WeightedMovingAverage(parameters, nameof(CalculatorNameEnum.WMA));
            double[,] prices = Pricedata.GetPrices(15);

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert
            var timestamps = result.Results.Keys.ToList();
            var orderedTimestamps = timestamps.OrderBy(t => t).ToList();

            // Check if timestamps are ordered
            Assert.Equal(orderedTimestamps, timestamps);
        }

        [Fact]
        public void Calculate_ShouldNotModifyInputPrices()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "5" } };
            var calculator = new WeightedMovingAverage(parameters, nameof(CalculatorNameEnum.WMA));
            double[,] prices = Pricedata.GetPrices(15);

            // Make a copy of the prices array for comparison
            double[,] pricesCopy = new double[prices.GetLength(0), prices.GetLength(1)];
            Array.Copy(prices, pricesCopy, prices.Length);

            // Act
            calculator.Calculate(prices, true);

            // Assert
            for (int i = 0; i < prices.GetLength(0); i++)
            {
                for (int j = 0; j < prices.GetLength(1); j++)
                {
                    Assert.Equal(pricesCopy[i, j], prices[i, j]);
                }
            }
        }


        [Fact]
        public void Calculate_ShouldHandleEmptyPricesArray()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "5" } };
            var calculator = new WeightedMovingAverage(parameters, nameof(CalculatorNameEnum.WMA));
            double[,] prices = new double[0, 6];

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => calculator.Calculate(prices));
            Assert.Equal("Invalid price data provided.", exception.Message);
        }
    }
}
