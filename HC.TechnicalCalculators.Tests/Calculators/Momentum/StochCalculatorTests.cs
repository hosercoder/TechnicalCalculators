using HC.TechnicalCalculators.Src.Calculators.Momentum;
using HC.TechnicalCalculators.Src.Models;
using HC.TechnicalCalculators.Tests.Helpers;

namespace HC.TechnicalCalculators.Tests.Calculators.Momentum
{
    public class StochCalculatorTests
    {
        [Fact]
        public void Constructor_ShouldSetDefaultFastKPeriod_WhenFastKPeriodParameterIsMissing()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
    {
        { nameof(ParameterNamesEnum.SlowKPeriod), "3" },
        { nameof(ParameterNamesEnum.SlowDPeriod), "3" }
    };

            // Act
            var calculator = new StochCalculator(parameters, nameof(CalculatorNameEnum.STOCH));

            // Assert
            Assert.NotNull(calculator);

            // Verify that the default FastKPeriod is used by checking if calculation works
            var prices = Pricedata.GetPrices(50);
            var results = calculator.Calculate(prices);

            Assert.NotNull(results);
            Assert.Equal(nameof(CalculatorNameEnum.STOCH), results.Name);
            Assert.NotEmpty(results.Results);
        }

        [Fact]
        public void Constructor_ShouldSetDefaultSlowKPeriod_WhenSlowKPeriodParameterIsMissing()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
                {
                    { nameof(ParameterNamesEnum.FastKPeriod), "5" },
                    { nameof(ParameterNamesEnum.SlowDPeriod), "3" }
                };

            // Act
            var calculator = new StochCalculator(parameters, nameof(CalculatorNameEnum.STOCH));

            // Assert
            Assert.NotNull(calculator);

            // Verify that the default SlowKPeriod is used by checking if calculation works
            var prices = Pricedata.GetPrices(50);
            var results = calculator.Calculate(prices);

            Assert.NotNull(results);
            Assert.Equal(nameof(CalculatorNameEnum.STOCH), results.Name);
            Assert.NotEmpty(results.Results);
        }

        [Fact]
        public void Constructor_ShouldSetDefaultSlowDPeriod_WhenSlowDPeriodParameterIsMissing()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
    {
        { nameof(ParameterNamesEnum.FastKPeriod), "5" },
        { nameof(ParameterNamesEnum.SlowKPeriod), "3" }
    };

            // Act
            var calculator = new StochCalculator(parameters, nameof(CalculatorNameEnum.STOCH));

            // Assert
            Assert.NotNull(calculator);

            // Verify that the default SlowDPeriod is used by checking if calculation works
            var prices = Pricedata.GetPrices(50);
            var results = calculator.Calculate(prices);

            Assert.NotNull(results);
            Assert.Equal(nameof(CalculatorNameEnum.STOCH), results.Name);
            Assert.NotEmpty(results.Results);
        }

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenAllRequiredParametersArePresent()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.FastKPeriod), "5" },
                { nameof(ParameterNamesEnum.SlowKPeriod), "3" },
                { nameof(ParameterNamesEnum.SlowDPeriod), "3" }
            };

            // Act
            var calculator = new StochCalculator(parameters, nameof(CalculatorNameEnum.STOCH));

            // Assert
            Assert.NotNull(calculator);
        }

        [Fact]
        public void Calculate_ShouldReturnCorrectResults_WhenParametersAreValid()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.FastKPeriod), "5" },
                { nameof(ParameterNamesEnum.SlowKPeriod), "3" },
                { nameof(ParameterNamesEnum.SlowDPeriod), "3" }
            };
            var calculator = new StochCalculator(parameters, nameof(CalculatorNameEnum.STOCH));
            var prices = Pricedata.GetPrices(20); // Ensure enough data points

            // Act
            var results = calculator.Calculate(prices, true);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(nameof(CalculatorNameEnum.STOCH), results.Name);
            Assert.True(results.Results.Count > 0);

            // Verify each result contains both K and D values
            foreach (var result in results.Results.Values)
            {
                Assert.Equal(2, result.Length);
                Assert.Contains(result, kvp => kvp.Key == nameof(TechnicalNamesEnum.STOCHK));
                Assert.Contains(result, kvp => kvp.Key == nameof(TechnicalNamesEnum.STOCHD));
            }
        }

        [Fact]
        public void Calculate_ShouldHandleNullPrices()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.FastKPeriod), "5" },
                { nameof(ParameterNamesEnum.SlowKPeriod), "3" },
                { nameof(ParameterNamesEnum.SlowDPeriod), "3" }
            };
            var calculator = new StochCalculator(parameters, nameof(CalculatorNameEnum.STOCH));
            double[,] prices = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => calculator.Calculate(prices));
            Assert.Contains("prices", exception.Message);
        }
    }
}
