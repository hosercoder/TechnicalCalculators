using HC.TechnicalCalculators.Src.Calculators.Momentum;
using HC.TechnicalCalculators.Src.Models;
using HC.TechnicalCalculators.Tests.Helpers;

namespace HC.TechnicalCalculators.Tests.Calculators.Momentum
{
    public class UltOscCalculatorTests
    {
        [Fact]
        public void Constructor_ShouldSetDefaultShortPeriod_WhenShortPeriodIsNotProvided()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
    {
        { nameof(ParameterNamesEnum.MediumPeriod), "14" },
        { nameof(ParameterNamesEnum.LongPeriod), "28" }
    };

            // Act
            var calculator = new UltOscCalculator(parameters, nameof(CalculatorNameEnum.ULTOSC));

            // Assert
            Assert.NotNull(calculator);

            // Verify that the default ShortPeriod is used by checking if calculation works
            var prices = Pricedata.GetPrices(50);
            var results = calculator.Calculate(prices);

            Assert.NotNull(results);
            Assert.Equal(nameof(CalculatorNameEnum.ULTOSC), results.Name);
            Assert.NotEmpty(results.Results);
        }

        [Fact]
        public void Constructor_ShouldSetDefaultMediumPeriod_WhenMediumPeriodIsNotProvided()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
    {
        { nameof(ParameterNamesEnum.ShortPeriod), "7" },
        { nameof(ParameterNamesEnum.LongPeriod), "28" }
    };

            // Act
            var calculator = new UltOscCalculator(parameters, nameof(CalculatorNameEnum.ULTOSC));

            // Assert
            Assert.NotNull(calculator);

            // Verify that the default MediumPeriod is used by checking if calculation works
            var prices = Pricedata.GetPrices(50);
            var results = calculator.Calculate(prices);

            Assert.NotNull(results);
            Assert.Equal(nameof(CalculatorNameEnum.ULTOSC), results.Name);
            Assert.NotEmpty(results.Results);
        }

        [Fact]
        public void Constructor_ShouldSetDefaultLongPeriod_WhenLongPeriodIsNotProvided()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
    {
        { nameof(ParameterNamesEnum.ShortPeriod), "7" },
        { nameof(ParameterNamesEnum.MediumPeriod), "14" }
    };

            // Act
            var calculator = new UltOscCalculator(parameters, nameof(CalculatorNameEnum.ULTOSC));

            // Assert
            Assert.NotNull(calculator);

            // Verify that the default LongPeriod is used by checking if calculation works
            var prices = Pricedata.GetPrices(50);
            var results = calculator.Calculate(prices);

            Assert.NotNull(results);
            Assert.Equal(nameof(CalculatorNameEnum.ULTOSC), results.Name);
            Assert.NotEmpty(results.Results);
        }

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenAllRequiredParametersArePresent()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.ShortPeriod), "5" },
                { nameof(ParameterNamesEnum.MediumPeriod), "10" },
                { nameof(ParameterNamesEnum.LongPeriod), "15" }
            };

            // Act
            var calculator = new UltOscCalculator(parameters, nameof(CalculatorNameEnum.ULTOSC));

            // Assert
            Assert.NotNull(calculator);
        }

        [Theory]
        [InlineData("0", "5", "10", "ShortPeriod must be between 1 and 100.")]
        [InlineData("3", "0", "10", "MediumPeriod must be between 1 and 100.")]
        [InlineData("3", "5", "0", "LongPeriod must be between 1 and 100.")]
        public void Calculate_ShouldThrowArgumentException_WhenAnyPeriodIsInvalid(
            string shortPeriod, string mediumPeriod, string longPeriod, string expectedMessage)
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.ShortPeriod), shortPeriod },
                { nameof(ParameterNamesEnum.MediumPeriod), mediumPeriod },
                { nameof(ParameterNamesEnum.LongPeriod), longPeriod }
            };

            var exception = Assert.Throws<ArgumentException>(() => new UltOscCalculator(parameters, nameof(CalculatorNameEnum.ULTOSC)));
            Assert.Equal(expectedMessage, exception.Message);
        }

        [Fact]
        public void Calculate_ShouldReturnCorrectResults_WhenParametersAreValid()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.ShortPeriod), "5" },
                { nameof(ParameterNamesEnum.MediumPeriod), "10" },
                { nameof(ParameterNamesEnum.LongPeriod), "15" }
            };

            var calculator = new UltOscCalculator(parameters, nameof(CalculatorNameEnum.ULTOSC));
            var prices = Pricedata.GetPrices(20); // Use more data points to ensure calculation works

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(nameof(CalculatorNameEnum.ULTOSC), result.Name);
            Assert.True(result.Results.Count > 0);

            // Verify structure of results
            foreach (var item in result.Results)
            {
                Assert.Single(item.Value); // Should have exactly one value per timestamp
                Assert.Equal(nameof(TechnicalNamesEnum.ULTOSC), item.Value[0].Key);
                Assert.IsType<double>(item.Value[0].Value);
            }
        }

        [Fact]
        public void Calculate_ShouldHandlePeriodsNotInAcendingOrder()
        {
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.ShortPeriod), "5" },
                { nameof(ParameterNamesEnum.MediumPeriod), "5" },
                { nameof(ParameterNamesEnum.LongPeriod), "10" }
            };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new UltOscCalculator(parameters, nameof(CalculatorNameEnum.ULTOSC)));
            Assert.Equal("Periods must be in ascending order: ShortPeriod < MediumPeriod < LongPeriod.", exception.Message);
        }

        [Fact]
        public void Calculate_ShouldHandleNullPrices()
        {
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.ShortPeriod), "5" },
                { nameof(ParameterNamesEnum.MediumPeriod), "10" },
                { nameof(ParameterNamesEnum.LongPeriod), "15" }
            };

            var calculator = new UltOscCalculator(parameters, nameof(CalculatorNameEnum.ULTOSC));

            var prices = new double[0, 6]; // Empty prices array
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => calculator.Calculate(prices));
            Assert.Equal("Invalid price data provided.", exception.Message);
        }
    }
}
