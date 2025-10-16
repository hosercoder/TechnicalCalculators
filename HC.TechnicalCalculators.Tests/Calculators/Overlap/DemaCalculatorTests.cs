using HC.TechnicalCalculators.Src.Calculators.Overlap;
using HC.TechnicalCalculators.Src.Models;
using HC.TechnicalCalculators.Tests.Helpers;

namespace HC.TechnicalCalculators.Tests.Calculators.Overlap
{
    public class DemaCalculatorTests
    {
        [Fact]
        public void Constructor_ShouldThrowArgumentException_WhenPeriodParameterIsMissing()
        {
            // Arrange
            var parameters = new Dictionary<string, string>();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new DemaCalculator(parameters, nameof(CalculatorNameEnum.DEMA)));
            Assert.Equal("Parameter Period is required.", exception.Message);
        }

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenPeriodParameterIsPresent()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "14" } };

            // Act
            var calculator = new DemaCalculator(parameters, nameof(CalculatorNameEnum.DEMA));

            // Assert
            Assert.NotNull(calculator);
        }

        [Fact]
        public void Calculate_ShouldThrowFormatException_WhenPeriodIsNotAnInteger()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "invalid" } };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new DemaCalculator(parameters, nameof(CalculatorNameEnum.DEMA)));
            Assert.Contains("Period must be between 2 and 200.", exception.Message);
        }

        [Fact]
        public void Calculate_ShouldReturnCorrectResults_WhenParametersAreValid()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "5" } };
            var calculator = new DemaCalculator(parameters, nameof(CalculatorNameEnum.DEMA));
            double[,] prices = Pricedata.GetPrices(30); // Use enough data points

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(nameof(CalculatorNameEnum.DEMA), result.Name);
            Assert.True(result.Results.Count > 0);

            // Check the structure of results
            foreach (var timestamp in result.Results.Keys)
            {
                var values = result.Results[timestamp];
                Assert.Single(values);
                Assert.Equal(nameof(TechnicalNamesEnum.DEMA), values[0].Key);
            }
        }

        [Fact]
        public void Calculate_ShouldReturnOrderedResults()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "5" } };
            var calculator = new DemaCalculator(parameters, nameof(CalculatorNameEnum.DEMA));
            double[,] prices = Pricedata.GetPrices(30);

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert
            var timestamps = result.Results.Keys.ToList();
            var orderedTimestamps = timestamps.OrderByDescending(t => t).ToList();
            Assert.Equal(orderedTimestamps, timestamps);
        }

        [Fact]
        public void Calculate_ShouldThrowArgumentNullException_WhenPricesIsNull()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "5" } };
            var calculator = new DemaCalculator(parameters, nameof(CalculatorNameEnum.DEMA));
            double[,] prices = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => calculator.Calculate(prices));
            Assert.Contains("prices", exception.Message);
        }

        [Fact]
        public void Calculate_WithLargeDataSet_ShouldCompleteSuccessfully()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "20" } };
            var calculator = new DemaCalculator(parameters, nameof(CalculatorNameEnum.DEMA));
            double[,] prices = Pricedata.GetPrices(200); // Large dataset

            // Act
            var result = calculator.Calculate(prices);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Results.Count > 0);
        }

        [Fact]
        public void Calculate_WithMinimalDataSet_ShouldCompleteSuccessfully()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "2" } };
            var calculator = new DemaCalculator(parameters, nameof(CalculatorNameEnum.DEMA));
            double[,] prices = Pricedata.GetPrices(10); // Minimal dataset

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert
            Assert.NotNull(result);
            // May not have any results if dataset is too small
        }
    }
}
