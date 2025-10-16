using HC.TechnicalCalculators.Src.Calculators.Overlap;
using HC.TechnicalCalculators.Src.Models;
using HC.TechnicalCalculators.Tests.Helpers;

namespace HC.TechnicalCalculators.Tests.Calculators.Overlap
{
    public class ExponentialMovingAverageTests
    {
        [Fact]
        public void Constructor_ShouldSetDefaultPeriod_WhenPeriodParameterIsMissing()
        {
            // Arrange
            var parameters = new Dictionary<string, string>();

            // Act
            var calculator = new ExponentialMovingAverage(parameters, nameof(CalculatorNameEnum.EMA));

            // Assert
            Assert.NotNull(calculator);

            // Verify that the default period is used by checking if calculation works
            var prices = Pricedata.GetPrices(50);
            var results = calculator.Calculate(prices);

            Assert.NotNull(results);
            Assert.Equal(nameof(CalculatorNameEnum.EMA), results.Name);
            Assert.NotEmpty(results.Results);
        }

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenPeriodParameterIsPresent()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "14" } };

            // Act
            var calculator = new ExponentialMovingAverage(parameters, nameof(CalculatorNameEnum.EMA));

            // Assert
            Assert.NotNull(calculator);
        }

        [Fact]
        public void Calculate_ShouldThrowFormatException_WhenPeriodIsNotAnInteger()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "invalid" } };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new ExponentialMovingAverage(parameters, nameof(CalculatorNameEnum.EMA)));
            Assert.Contains("Period must be between 2 and 200.", exception.Message);
        }

        [Fact]
        public void Calculate_ShouldReturnCorrectResults_WhenParametersAreValid()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "5" } };
            var calculator = new ExponentialMovingAverage(parameters, nameof(CalculatorNameEnum.EMA));
            double[,] prices = Pricedata.GetPrices(30); // Use enough data points

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(nameof(CalculatorNameEnum.EMA), result.Name);
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
        public void Calculate_ShouldReturnOrderedResults()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "5" } };
            var calculator = new ExponentialMovingAverage(parameters, nameof(CalculatorNameEnum.EMA));
            double[,] prices = Pricedata.GetPrices(30);

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert
            var timestamps = result.Results.Keys.ToList();
            var orderedTimestamps = timestamps.OrderBy(t => t).ToList();
            Assert.Equal(orderedTimestamps, timestamps);
        }

        [Fact]
        public void Calculate_ShouldThrowArgumentNullException_WhenPricesIsNull()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "5" } };
            var calculator = new ExponentialMovingAverage(parameters, nameof(CalculatorNameEnum.EMA));
            double[,] prices = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => calculator.Calculate(prices));
            Assert.Contains("prices", exception.Message);
        }

        [Fact]
        public void Calculate_DifferentPeriods_ShouldProduceDifferentResults()
        {
            // Arrange
            var parameters1 = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "5" } };
            var parameters2 = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "10" } };

            var calculator1 = new ExponentialMovingAverage(parameters1, nameof(CalculatorNameEnum.EMA));
            var calculator2 = new ExponentialMovingAverage(parameters2, nameof(CalculatorNameEnum.EMA));

            double[,] prices = Pricedata.GetPrices(50); // Need enough data points

            // Act
            var result1 = calculator1.Calculate(prices, true);
            var result2 = calculator2.Calculate(prices, true);

            // Assert
            // Find a common timestamp that exists in both results
            var commonTimestamps = result1.Results.Keys.Intersect(result2.Results.Keys);
            Assert.NotEmpty(commonTimestamps);

            var commonTimestamp = commonTimestamps.Last(); // Use last common timestamp for better comparison
            var ema1 = result1.Results[commonTimestamp][0].Value;
            var ema2 = result2.Results[commonTimestamp][0].Value;

            // Different periods should produce different EMA values
            Assert.NotEqual(ema1, ema2);
        }

        [Fact]
        public void Calculate_ShouldProduceValuesRelatedToInputPrices()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "5" } };
            var calculator = new ExponentialMovingAverage(parameters, nameof(CalculatorNameEnum.EMA));
            double[,] prices = Pricedata.GetPrices(20);

            // Get a range of close prices
            var closePrices = new List<double>();
            for (int i = 0; i < prices.GetLength(0); i++)
            {
                closePrices.Add(prices[i, 4]); // Close price is at index 4
            }

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert
            // Check that EMA values fall within a reasonable range related to close prices
            double minClose = closePrices.Min();
            double maxClose = closePrices.Max();

            foreach (var ema in result.Results.Values.Select(v => v[0].Value))
            {
                // EMA should be reasonably close to the price range
                // We'll allow some margin outside the exact min/max
                double margin = (maxClose - minClose) * 0.2; // 20% margin
                Assert.InRange(ema, minClose - margin, maxClose + margin);
            }
        }

        [Fact]
        public void Calculate_WithLargeDataSet_ShouldCompleteSuccessfully()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "20" } };
            var calculator = new ExponentialMovingAverage(parameters, nameof(CalculatorNameEnum.EMA));
            double[,] prices = Pricedata.GetPrices(200); // Large dataset

            // Act
            var result = calculator.Calculate(prices);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Results.Count > 0);
        }

        [Fact]
        public void Calculate_ResultsShouldHaveLessValuesThanInputForNonZeroPeriod()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "14" } };
            var calculator = new ExponentialMovingAverage(parameters, nameof(CalculatorNameEnum.EMA));
            double[,] prices = Pricedata.GetPrices(100);

            // Act
            var result = calculator.Calculate(prices);

            // Assert
            // EMA with period > 1 will have fewer results than input price points
            Assert.True(result.Results.Count < prices.GetLength(0));
        }
    }
}
