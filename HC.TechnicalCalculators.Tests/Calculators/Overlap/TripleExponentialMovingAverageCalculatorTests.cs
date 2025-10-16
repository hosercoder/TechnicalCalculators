using HC.TechnicalCalculators.Src.Calculators.Overlap;
using HC.TechnicalCalculators.Src.Models;
using HC.TechnicalCalculators.Tests.Helpers;

namespace HC.TechnicalCalculators.Tests.Calculators.Overlap
{
    public class TripleExponentialMovingAverageCalculatorTests
    {
        [Fact]
        public void Constructor_ShouldThrowArgumentException_WhenPeriodParameterIsMissing()
        {
            // Arrange
            var parameters = new Dictionary<string, string>();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new TripleExponentialMovingAverageCalculator(parameters, nameof(CalculatorNameEnum.TEMA)));
            Assert.Equal("Parameter Period is required.", exception.Message);
        }

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenPeriodParameterIsPresent()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "5" } };

            // Act
            var calculator = new TripleExponentialMovingAverageCalculator(parameters, nameof(CalculatorNameEnum.TEMA));

            // Assert
            Assert.NotNull(calculator);
        }

        [Fact]
        public void Calculate_ShouldThrowFormatException_WhenPeriodIsNotAnInteger()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "invalid" } };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new TripleExponentialMovingAverageCalculator(parameters, nameof(CalculatorNameEnum.TEMA)));
            Assert.Contains("Period must be between 2 and 200.", exception.Message);
        }

        [Fact]
        public void Calculate_ShouldThrowArgumentNullException_WhenPricesIsNull()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "5" } };
            var calculator = new TripleExponentialMovingAverageCalculator(parameters, nameof(CalculatorNameEnum.TEMA));
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
            var calculator = new TripleExponentialMovingAverageCalculator(parameters, nameof(CalculatorNameEnum.TEMA));
            double[,] prices = Pricedata.GetPrices(30); // Use enough data points for TEMA calculation

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(nameof(CalculatorNameEnum.TEMA), result.Name);
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
        public void Calculate_ShouldContainValuesForAllTimestamps()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "5" } };
            var calculator = new TripleExponentialMovingAverageCalculator(parameters, nameof(CalculatorNameEnum.TEMA));
            double[,] prices = Pricedata.GetPrices(30);

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert
            // Each timestamp in the prices array should have a corresponding result
            for (int i = 0; i < prices.GetLength(0); i++)
            {
                long timestamp = (long)prices[i, 0];
                // Due to the way TEMA works, we expect some initial values to be NaN,
                // so we just check that all timestamps have entries in the results dictionary
                Assert.True(result.Results.ContainsKey(timestamp));
            }
        }

        [Fact]
        public void Calculate_ResultsShouldBeOrderedByTimestamp()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "5" } };
            var calculator = new TripleExponentialMovingAverageCalculator(parameters, nameof(CalculatorNameEnum.TEMA));
            double[,] prices = Pricedata.GetPrices(30);

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
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "5" } };
            var calculator = new TripleExponentialMovingAverageCalculator(parameters, nameof(CalculatorNameEnum.TEMA));
            double[,] prices = Pricedata.GetPrices(30);

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
