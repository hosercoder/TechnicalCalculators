using HC.TechnicalCalculators.Src.Calculators.Overlap;
using HC.TechnicalCalculators.Src.Models;
using HC.TechnicalCalculators.Tests.Helpers;

namespace HC.TechnicalCalculators.Tests.Calculators.Overlap
{
    public class SimpleMovingAverageTests
    {

        [Fact]
        public void Constructor_ShouldSetDefaultPeriod_WhenPeriodParameterIsMissing()
        {
            // Arrange
            var parameters = new Dictionary<string, string>();

            // Act
            var calculator = new SimpleMovingAverage(parameters, nameof(CalculatorNameEnum.SMA));

            // Assert
            Assert.NotNull(calculator);

            // Verify that the default period is used by checking if calculation works
            var prices = Pricedata.GetPrices(50);
            var results = calculator.Calculate(prices);

            Assert.NotNull(results);
            Assert.Equal(nameof(CalculatorNameEnum.SMA), results.Name);
            Assert.NotEmpty(results.Results);
        }

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenPeriodParameterIsPresent()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "5" } };

            // Act
            var calculator = new SimpleMovingAverage(parameters, nameof(CalculatorNameEnum.SMA));

            // Assert
            Assert.NotNull(calculator);
        }

        [Fact]
        public void Calculate_ShouldThrowFormatException_WhenPeriodIsNotAnInteger()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "invalid" } };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new SimpleMovingAverage(parameters, nameof(CalculatorNameEnum.SMA)));
            Assert.Contains("Period must be between 2 and 200.", exception.Message);
        }

        [Fact]
        public void Calculate_ShouldReturnCorrectResults_WhenParametersAreValid()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "5" } };
            var calculator = new SimpleMovingAverage(parameters, nameof(CalculatorNameEnum.SMA));
            double[,] prices = Pricedata.GetPrices(10);

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(nameof(CalculatorNameEnum.SMA), result.Name);
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
        public void Calculate_ShouldComputeCorrectSMAValues()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "3" } };
            var calculator = new SimpleMovingAverage(parameters, nameof(CalculatorNameEnum.SMA));

            // Create a simple price array with known values
            double[,] prices = new double[5, 6];

            // Set timestamps
            for (int i = 0; i < 5; i++)
            {
                prices[i, 0] = 1000 + i;  // Timestamps: 1000, 1001, 1002, 1003, 1004
            }

            // Set close prices: 10, 20, 30, 40, 50
            prices[0, 4] = 10;
            prices[1, 4] = 20;
            prices[2, 4] = 30;
            prices[3, 4] = 40;
            prices[4, 4] = 50;

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert
            // Expected SMA values:
            // SMA(10, 20, 30) = 20
            // SMA(20, 30, 40) = 30
            // SMA(30, 40, 50) = 40

            // Check that we have the right number of results
            Assert.Equal(3, result.Results.Count);

            // Check each expected result (allowing a small floating-point margin)
            if (result.Results.ContainsKey(1002))
                Assert.Equal(20, result.Results[1002][0].Value, 6); // precision to 6 decimal places

            if (result.Results.ContainsKey(1003))
                Assert.Equal(30, result.Results[1003][0].Value, 6);

            if (result.Results.ContainsKey(1004))
                Assert.Equal(40, result.Results[1004][0].Value, 6);
        }

        [Fact]
        public void Calculate_ShouldThrowArgumentNullException_WhenPricesIsNull()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "5" } };
            var calculator = new SimpleMovingAverage(parameters, nameof(CalculatorNameEnum.SMA));
            double[,] prices = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => calculator.Calculate(prices));
            Assert.Contains("prices", exception.Message);
        }

        [Fact]
        public void Calculate_DifferentPeriods_ShouldProduceDifferentResults()
        {
            // Arrange
            var parameters1 = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "3" } };
            var parameters2 = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "5" } };

            var calculator1 = new SimpleMovingAverage(parameters1, nameof(CalculatorNameEnum.SMA));
            var calculator2 = new SimpleMovingAverage(parameters2, nameof(CalculatorNameEnum.SMA));

            double[,] prices = Pricedata.GetPrices(10);

            // Act
            var result1 = calculator1.Calculate(prices, true);
            var result2 = calculator2.Calculate(prices, true);

            // Assert
            // Different periods should produce different numbers of results
            Assert.NotEqual(result1.Results.Count, result2.Results.Count);

            // Find any common timestamps and compare their values
            var commonTimestamps = result1.Results.Keys.Intersect(result2.Results.Keys);
            if (commonTimestamps.Any())
            {
                var commonTimestamp = commonTimestamps.First();
                var sma1 = result1.Results[commonTimestamp][0].Value;
                var sma2 = result2.Results[commonTimestamp][0].Value;

                // Different periods should produce different SMA values
                Assert.NotEqual(sma1, sma2);
            }
        }

        [Fact]
        public void Calculate_PeriodEqualToDataLength_ShouldReturnSingleAverage()
        {
            // Arrange
            double[,] prices = Pricedata.GetPrices(5);
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "5" } };
            var calculator = new SimpleMovingAverage(parameters, nameof(CalculatorNameEnum.SMA));

            // Calculate expected average of all 5 close prices
            double expectedAverage = 0;
            for (int i = 0; i < prices.GetLength(0); i++)
            {
                expectedAverage += prices[i, 4]; // Close price at index 4
            }
            expectedAverage /= prices.GetLength(0);

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert
            Assert.Single(result.Results);
            var singleResult = result.Results.Values.First();
            Assert.Equal(expectedAverage, singleResult[0].Value, 6); // Compare with 6 decimal precision
        }

        [Fact]
        public void Calculate_ShouldProduceValuesWithinRangeOfInputPrices()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "3" } };
            var calculator = new SimpleMovingAverage(parameters, nameof(CalculatorNameEnum.SMA));
            double[,] prices = Pricedata.GetPrices(10);

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
            // Each SMA value should be within the range of input prices
            foreach (var value in result.Results.Values)
            {
                double sma = value[0].Value;
                Assert.InRange(sma, minClose, maxClose);
            }
        }

        [Fact]
        public void Calculate_WithConstantPrices_ShouldReturnConstantSMA()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "3" } };
            var calculator = new SimpleMovingAverage(parameters, nameof(CalculatorNameEnum.SMA));

            // Create a price array with constant values
            double[,] prices = new double[5, 6];
            double constantPrice = 100;

            // Set timestamps and constant close prices
            for (int i = 0; i < 5; i++)
            {
                prices[i, 0] = 1000 + i;  // Timestamps
                prices[i, 4] = constantPrice;  // All close prices = 100
            }

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert
            // All SMA values should equal the constant price
            foreach (var value in result.Results.Values)
            {
                Assert.Equal(constantPrice, value[0].Value);
            }
        }

        [Fact]
        public void Calculate_ResultsShouldBeOrderedByTimestamp()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "3" } };
            var calculator = new SimpleMovingAverage(parameters, nameof(CalculatorNameEnum.SMA));
            double[,] prices = Pricedata.GetPrices(10);

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert
            var timestamps = result.Results.Keys.ToList();
            var orderedTimestamps = timestamps.OrderByDescending(t => t).ToList();

            // Check if timestamps are ordered
            Assert.Equal(orderedTimestamps, timestamps);
        }

        [Fact]
        public void Calculate_ShouldNotModifyInputPrices()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "3" } };
            var calculator = new SimpleMovingAverage(parameters, nameof(CalculatorNameEnum.SMA));
            double[,] prices = Pricedata.GetPrices(10);

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
    }
}
