using HC.TechnicalCalculators.Src.Calculators.Momentum;
using HC.TechnicalCalculators.Src.Models;
using HC.TechnicalCalculators.Tests.Helpers;

namespace HC.TechnicalCalculators.Tests.Calculators.Momentum
{
    public class AdxCalculatorTests
    {
        [Fact]
        public void Constructor_ShouldThrowArgumentException_WhenPeriodParameterIsMissing()
        {
            // Arrange
            var parameters = new Dictionary<string, string>();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new AdxCalculator(parameters, nameof(CalculatorNameEnum.ADX)));
            Assert.Equal("Parameter Period is required.", exception.Message);
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentException_WhenPeriodParameterIsNotPositiveInteger()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.Period), "0" }
            };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new AdxCalculator(parameters, nameof(CalculatorNameEnum.ADX)));
            Assert.Equal("Period must be between 2 and 100.", exception.Message);
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentException_WhenPeriodParameterIsNotInteger()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.Period), "invalid" }
            };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new AdxCalculator(parameters, nameof(CalculatorNameEnum.ADX)));
            Assert.Equal("Period must be between 2 and 100.", exception.Message);
        }

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenPeriodParameterIsValid()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.Period), "14" }
            };

            // Act
            var calculator = new AdxCalculator(parameters, nameof(CalculatorNameEnum.ADX));

            // Assert
            Assert.NotNull(calculator);
        }

        [Fact]
        public void Calculate_ShouldReturnValidResults()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.Period), "14" }
            };
            var calculator = new AdxCalculator(parameters, nameof(CalculatorNameEnum.ADX));
            var prices = Pricedata.GetPrices(50); // Assuming GetPrices returns test price data with enough rows

            // Act
            var results = calculator.Calculate(prices, true);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(nameof(CalculatorNameEnum.ADX), results.Name);
            Assert.NotEmpty(results.Results);

            // Verify structure of results
            foreach (var result in results.Results)
            {
                Assert.Single(result.Value); // Each timestamp should have one value (ADX)
                Assert.Equal(nameof(TechnicalNamesEnum.ADX), result.Value[0].Key);
                Assert.True(result.Value[0].Value >= 0 && result.Value[0].Value <= 100,
                    $"ADX value {result.Value[0].Value} should be between 0 and 100");
            }
        }

        [Fact]
        public void Calculate_ShouldHandleMinimalValidDataset()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.Period), "2" } // Use minimum viable period
            };
            var calculator = new AdxCalculator(parameters, nameof(CalculatorNameEnum.ADX));
            var prices = Pricedata.GetPrices(5); // Minimum dataset

            // Act
            var results = calculator.Calculate(prices, true);

            // Assert
            Assert.NotNull(results);
            // Further assertions depend on the specific behavior of TALib.Core.Adx with minimal data
        }

    }
}
