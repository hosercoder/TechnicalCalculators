using HC.TechnicalCalculators.Src.Calculators.Momentum;
using HC.TechnicalCalculators.Src.Models;
using HC.TechnicalCalculators.Tests.Helpers;

namespace HC.TechnicalCalculators.Tests.Calculators.Momentum
{
    public class ApoCalculatorTests
    {

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenAllRequiredParametersArePresent()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.FastPeriod), "12" },
                { nameof(ParameterNamesEnum.SlowPeriod), "26" }
            };

            // Act
            var calculator = new ApoCalculator(parameters, nameof(CalculatorNameEnum.APO));

            // Assert
            Assert.NotNull(calculator);
        }

        [Fact]
        public void Calculate_ShouldThrowArgumentException_WhenFastPeriodIsZeroOrNegative()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.FastPeriod), "0" },
                { nameof(ParameterNamesEnum.SlowPeriod), "26" }
            };
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new ApoCalculator(parameters, nameof(CalculatorNameEnum.APO)));
            Assert.Equal("FastPeriod must be between 2 and 50.", exception.Message);
        }

        [Fact]
        public void Calculate_ShouldThrowArgumentException_WhenSlowPeriodIsZeroOrNegative()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.FastPeriod), "12" },
                { nameof(ParameterNamesEnum.SlowPeriod), "-1" }
            };
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new ApoCalculator(parameters, nameof(CalculatorNameEnum.APO)));
            Assert.Equal("SlowPeriod must be between 3 and 100.", exception.Message);
        }

        [Fact]
        public void Calculate_ShouldReturnValidResults_WithTypicalParameters()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.FastPeriod), "12" },
                { nameof(ParameterNamesEnum.SlowPeriod), "26" }
            };
            var calculator = new ApoCalculator(parameters, nameof(CalculatorNameEnum.APO));
            var prices = Pricedata.GetPrices(100); // Need enough data for meaningful calculation

            // Act
            var results = calculator.Calculate(prices);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(nameof(CalculatorNameEnum.APO), results.Name);
            Assert.NotEmpty(results.Results);

            // Verify structure of results
            foreach (var result in results.Results)
            {
                Assert.Single(result.Value);
                Assert.Equal(nameof(TechnicalNamesEnum.APO), result.Value[0].Key);
                // APO can be positive or negative, so no range check here
            }
        }

        [Fact]
        public void Calculate_ShouldHandleLargeDataset()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.FastPeriod), "12" },
                { nameof(ParameterNamesEnum.SlowPeriod), "26" }
            };
            var calculator = new ApoCalculator(parameters, nameof(CalculatorNameEnum.APO));
            var prices = Pricedata.GetPrices(500); // Large dataset

            // Act
            var results = calculator.Calculate(prices);

            // Assert
            Assert.NotNull(results);
            Assert.NotEmpty(results.Results);
        }

        [Fact]
        public void Calculate_ShouldHandleEqualFastAndSlowPeriods()
        {
            // Arrange - unusual case where fast and slow periods are the same
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.FastPeriod), "14" },
                { nameof(ParameterNamesEnum.SlowPeriod), "14" }
            };
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new ApoCalculator(parameters, nameof(CalculatorNameEnum.APO)));
            Assert.Equal("FastPeriod must be less than SlowPeriod.", exception.Message);
        }
    }
}
