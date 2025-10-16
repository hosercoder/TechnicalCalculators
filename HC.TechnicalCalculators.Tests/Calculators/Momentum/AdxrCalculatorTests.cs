using HC.TechnicalCalculators.Src.Calculators.Momentum;
using HC.TechnicalCalculators.Src.Models;
using HC.TechnicalCalculators.Tests.Helpers;

namespace HC.TechnicalCalculators.Tests.Calculators.Momentum
{
    public class AdxrCalculatorTests
    {
        [Fact]
        public void Constructor_ShouldSetDefaultPeriod_WhenPeriodParameterIsMissing()
        {
            // Arrange
            var parameters = new Dictionary<string, string>();

            // Act
            var calculator = new AdxrCalculator(parameters, nameof(CalculatorNameEnum.ADXR));

            // Assert
            Assert.NotNull(calculator);

            // Verify that the default period is used by checking if calculation works
            var prices = Pricedata.GetPrices(50);
            var results = calculator.Calculate(prices);

            Assert.NotNull(results);
            Assert.Equal(nameof(CalculatorNameEnum.ADXR), results.Name);
            Assert.NotEmpty(results.Results);
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
            var exception = Assert.Throws<ArgumentException>(() => new AdxrCalculator(parameters, nameof(CalculatorNameEnum.ADXR)));
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
            var exception = Assert.Throws<ArgumentException>(() => new AdxrCalculator(parameters, nameof(CalculatorNameEnum.ADXR)));
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
            var calculator = new AdxrCalculator(parameters, nameof(CalculatorNameEnum.ADXR));

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
            var calculator = new AdxrCalculator(parameters, nameof(CalculatorNameEnum.ADXR));
            var prices = Pricedata.GetPrices(100); // Need enough data points for ADXR calculation

            // Act
            var results = calculator.Calculate(prices);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(nameof(CalculatorNameEnum.ADXR), results.Name);
            Assert.NotEmpty(results.Results);

            // Verify structure of results
            foreach (var result in results.Results)
            {
                Assert.Single(result.Value); // Each timestamp should have one value (ADXR)
                Assert.Equal(nameof(TechnicalNamesEnum.ADXR), result.Value[0].Key);
                Assert.True(result.Value[0].Value >= 0 && result.Value[0].Value <= 100,
                    $"ADXR value {result.Value[0].Value} should be between 0 and 100");
            }
        }

        [Fact]
        public void Calculate_ShouldHandleLargeDataset()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.Period), "14" }
            };
            var calculator = new AdxrCalculator(parameters, nameof(CalculatorNameEnum.ADXR));
            var prices = Pricedata.GetPrices(500); // Large dataset

            // Act
            var results = calculator.Calculate(prices);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(nameof(CalculatorNameEnum.ADXR), results.Name);
            Assert.NotEmpty(results.Results);
        }

        [Fact]
        public void Calculate_ResultsShouldDifferWithDifferentPeriods()
        {
            // Arrange
            var parameters14 = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "14" } };
            var calculator14 = new AdxrCalculator(parameters14, nameof(CalculatorNameEnum.ADXR));

            var parameters21 = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "21" } };
            var calculator21 = new AdxrCalculator(parameters21, nameof(CalculatorNameEnum.ADXR));

            var prices = Pricedata.GetPrices(200);

            // Act
            var results14 = calculator14.Calculate(prices);
            var results21 = calculator21.Calculate(prices);

            // Find a timestamp that exists in both result sets
            var commonTimestamp = results14.Results.Keys.Intersect(results21.Results.Keys).FirstOrDefault();

            // Assert
            Assert.NotEqual(0, commonTimestamp); // Ensure we found a common timestamp

            // The ADXR values should be different for different periods
            var adxr14Value = results14.Results[commonTimestamp][0].Value;
            var adxr21Value = results21.Results[commonTimestamp][0].Value;

            // Values might be very close but should almost never be exactly equal
            Assert.NotEqual(adxr14Value, adxr21Value);
        }
    }
}
