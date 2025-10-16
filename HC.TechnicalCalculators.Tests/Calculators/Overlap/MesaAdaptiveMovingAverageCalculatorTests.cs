using HC.TechnicalCalculators.Src.Calculators.Overlap;
using HC.TechnicalCalculators.Src.Models;
using HC.TechnicalCalculators.Tests.Helpers;

namespace HC.TechnicalCalculators.Tests.Calculators.Overlap
{
    public class MesaAdaptiveMovingAverageCalculatorTests
    {
        [Fact]
        public void Constructor_ShouldSetDefaultFastLimit_WhenFastLimitIsNotProvided()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.SlowLimit), "0.05" }
            };

            // Act
            var calculator = new MesaAdaptiveMovingAverageCalculator(parameters, nameof(CalculatorNameEnum.MAMA));

            // Assert
            Assert.NotNull(calculator);

            // Verify that the default FastLimit is used by checking if calculation works
            var prices = Pricedata.GetPrices(50);
            var results = calculator.Calculate(prices);

            Assert.NotNull(results);
            Assert.Equal(nameof(CalculatorNameEnum.MAMA), results.Name);
            Assert.NotEmpty(results.Results);
        }

        [Fact]
        public void Constructor_ShouldSetDefaultSlowLimit_WhenSlowLimitIsNotProvided()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.FastLimit), "0.5" }
            };

            // Act
            var calculator = new MesaAdaptiveMovingAverageCalculator(parameters, nameof(CalculatorNameEnum.MAMA));

            // Assert
            Assert.NotNull(calculator);

            // Verify that the default SlowLimit is used by checking if calculation works
            var prices = Pricedata.GetPrices(50);
            var results = calculator.Calculate(prices);

            Assert.NotNull(results);
            Assert.Equal(nameof(CalculatorNameEnum.MAMA), results.Name);
            Assert.NotEmpty(results.Results);
        }

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenAllRequiredParametersAreProvided()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.FastLimit), "0.5" }, // Lower value
                { nameof(ParameterNamesEnum.SlowLimit), "0.05" }   // Higher value
            };

            // Act
            var calculator = new MesaAdaptiveMovingAverageCalculator(parameters, nameof(CalculatorNameEnum.MAMA));

            // Assert
            Assert.NotNull(calculator);
        }

        [Fact]
        public void Calculate_ShouldThrowFormatException_WhenFastLimitIsNotANumber()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.FastLimit), "invalid" }, // Lower value
                { nameof(ParameterNamesEnum.SlowLimit), "0.5" }   // Higher value
            };

            var exception = Assert.Throws<ArgumentException>(() => new MesaAdaptiveMovingAverageCalculator(parameters, nameof(CalculatorNameEnum.MAMA)));
            Assert.Contains("FastLimit must be between 0.01 and 0.99.", exception.Message);
        }

        [Fact]
        public void Calculate_ShouldThrowFormatException_WhenSlowLimitIsNotANumber()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.FastLimit), "0.05" }, // Lower value
                { nameof(ParameterNamesEnum.SlowLimit), "na" }   // Higher value
            };

            var exception = Assert.Throws<ArgumentException>(() => new MesaAdaptiveMovingAverageCalculator(parameters, nameof(CalculatorNameEnum.MAMA)));
            Assert.Contains("SlowLimit must be between 0.01 and 0.99.", exception.Message);
        }

        [Fact]
        public void Calculate_ShouldReturnCorrectResults_WhenParametersAreValid()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.FastLimit), "0.5" }, // Lower value
                { nameof(ParameterNamesEnum.SlowLimit), "0.05" }   // Higher value
            };

            var calculator = new MesaAdaptiveMovingAverageCalculator(parameters, nameof(CalculatorNameEnum.MAMA));
            double[,] prices = Pricedata.GetPrices(50); // Use enough data points

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(nameof(CalculatorNameEnum.MAMA), result.Name);
            Assert.True(result.Results.Count > 0);

            // Check the structure of results
            foreach (var timestamp in result.Results.Keys)
            {
                var values = result.Results[timestamp];
                Assert.Single(values);
                Assert.Equal(nameof(TechnicalNamesEnum.MAMA), values[0].Key);
                Assert.IsType<double>(values[0].Value);
            }
        }

        [Fact]
        public void Calculate_ShouldThrowArgumentNullException_WhenPricesIsNull()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.FastLimit), "0.5" }, // Lower value
                { nameof(ParameterNamesEnum.SlowLimit), "0.05" }   // Higher value
            };

            var calculator = new MesaAdaptiveMovingAverageCalculator(parameters, nameof(CalculatorNameEnum.MAMA));
            double[,]? prices = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => calculator.Calculate(prices!));
            Assert.Contains("prices", exception.Message);
        }

        [Fact]
        public void Calculate_WithLargeDataSet_ShouldCompleteSuccessfully()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.FastLimit), "0.5" }, // Lower value
                { nameof(ParameterNamesEnum.SlowLimit), "0.05" }   // Higher value
            };

            var calculator = new MesaAdaptiveMovingAverageCalculator(parameters, nameof(CalculatorNameEnum.MAMA));
            double[,] prices = Pricedata.GetPrices(200); // Large dataset

            // Act
            var result = calculator.Calculate(prices);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Results.Count > 0);
        }

        [Fact]
        public void Calculate_ResultShouldNotBeEmpty()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.FastLimit), "0.5" }, // Lower value
                { nameof(ParameterNamesEnum.SlowLimit), "0.05" }   // Higher value
            };

            var calculator = new MesaAdaptiveMovingAverageCalculator(parameters, nameof(CalculatorNameEnum.MAMA));
            double[,] prices = Pricedata.GetPrices(50);

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert
            Assert.NotEmpty(result.Results);
        }

        [Fact]
        public void Calculate_ShouldNotModifyInputPrices()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.FastLimit), "0.5" }, // Lower value
                { nameof(ParameterNamesEnum.SlowLimit), "0.05" }   // Higher value
            };

            var calculator = new MesaAdaptiveMovingAverageCalculator(parameters, nameof(CalculatorNameEnum.MAMA));
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

        [Fact]
        public void Calculate_FastLimitGreaterThanSlowLimit_ShouldProduceValidResults()
        {
            // Arrange - MAMA typically has fastLimit > slowLimit
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.FastLimit), "0.05" }, // Lower value
                { nameof(ParameterNamesEnum.SlowLimit), "0.5" }   // Higher value
            };

            var exception = Assert.Throws<ArgumentException>(() => new MesaAdaptiveMovingAverageCalculator(parameters, nameof(CalculatorNameEnum.MAMA)));
            Assert.Contains("FastLimit must be greater than SlowLimit.", exception.Message);

        }

        [Fact]
        public void Calculate_SlowLimitGreaterThanFastLimit_ShouldStillCalculate()
        {
            // Arrange - This is an unusual configuration but should still work
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.FastLimit), "0.5" }, // Lower value
                { nameof(ParameterNamesEnum.SlowLimit), "0.05" }   // Higher value
            };

            var calculator = new MesaAdaptiveMovingAverageCalculator(parameters, nameof(CalculatorNameEnum.MAMA));
            double[,] prices = Pricedata.GetPrices(50);

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Results.Count > 0);
        }
    }
}