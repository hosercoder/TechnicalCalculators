using HC.TechnicalCalculators.Src.Calculators.Overlap;
using HC.TechnicalCalculators.Src.Models;
using HC.TechnicalCalculators.Tests.Helpers;

namespace HC.TechnicalCalculators.Tests.Calculators.Overlap
{
    public class BollingerBandsCalculatorTests
    {
        [Fact]
        public void Constructor_ShouldThrowArgumentException_WhenPeriodIsNotProvided()
        {
            var parameters = new Dictionary<string, string> {
                { nameof(ParameterNamesEnum.Multiplier), "2" } };
            // Arrange

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new BollingerBandsCalculator(parameters, nameof(CalculatorNameEnum.BBANDS)));
            Assert.Equal("Parameters Period and Multiplier are required.", exception.Message);
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentException_WhenMultiplierIsNotProvided()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { {
                    nameof(ParameterNamesEnum.Period), "20" } };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new BollingerBandsCalculator(parameters, nameof(CalculatorNameEnum.BBANDS)));
            Assert.Equal("Parameters Period and Multiplier are required.", exception.Message);
        }

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenAllRequiredParametersAreProvided()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { {
                    nameof(ParameterNamesEnum.Period), "20" },
                { nameof(ParameterNamesEnum.Multiplier), "2" } };

            // Act
            var calculator = new BollingerBandsCalculator(parameters, nameof(CalculatorNameEnum.BBANDS));

            // Assert
            Assert.NotNull(calculator);
        }

        [Fact]
        public void Calculate_ShouldReturnCorrectResults_WhenParametersAreValid()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { {
                    nameof(ParameterNamesEnum.Period), "20" },
                { nameof(ParameterNamesEnum.Multiplier), "2" } };
            var calculator = new BollingerBandsCalculator(parameters, nameof(CalculatorNameEnum.BBANDS));
            double[,] prices = Pricedata.GetPrices(30); // Use enough data points

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(nameof(CalculatorNameEnum.BBANDS), result.Name);
            Assert.True(result.Results.Count > 0);

            // Check the structure of the results
            foreach (var timestamp in result.Results.Keys)
            {
                var bands = result.Results[timestamp];
                Assert.Equal(3, bands.Length);
                Assert.Contains(bands, kvp => kvp.Key == nameof(TechnicalNamesEnum.MIDDLEBAND));
                Assert.Contains(bands, kvp => kvp.Key == nameof(TechnicalNamesEnum.UPPERBAND));
                Assert.Contains(bands, kvp => kvp.Key == nameof(TechnicalNamesEnum.LOWERBAND));
            }
        }

        [Fact]
        public void Calculate_ShouldReturnOrderedResults()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { {
                    nameof(ParameterNamesEnum.Period), "20" },
                { nameof(ParameterNamesEnum.Multiplier), "2" } };
            var calculator = new BollingerBandsCalculator(parameters, nameof(CalculatorNameEnum.BBANDS));
            double[,] prices = Pricedata.GetPrices(30);

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert
            var timestamps = result.Results.Keys.ToList();
            var orderedTimestamps = timestamps.OrderByDescending(t => t).ToList();
            Assert.Equal(orderedTimestamps, timestamps);
        }

        [Fact]
        public void Calculate_UpperBandShouldBeHigherThanMiddleBand()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { {
                    nameof(ParameterNamesEnum.Period), "20" },
                { nameof(ParameterNamesEnum.Multiplier), "2" } };
            var calculator = new BollingerBandsCalculator(parameters, nameof(CalculatorNameEnum.BBANDS));
            double[,] prices = Pricedata.GetPrices(30);

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert
            foreach (var timestamp in result.Results.Keys)
            {
                var bands = result.Results[timestamp];
                var upperBand = bands.First(kvp => kvp.Key == nameof(TechnicalNamesEnum.UPPERBAND)).Value;
                var middleBand = bands.First(kvp => kvp.Key == nameof(TechnicalNamesEnum.MIDDLEBAND)).Value;
                Assert.True(upperBand >= middleBand);
            }
        }

        [Fact]
        public void Calculate_LowerBandShouldBeLowerThanMiddleBand()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { {
                    nameof(ParameterNamesEnum.Period), "20" },
                { nameof(ParameterNamesEnum.Multiplier), "2" } };
            var calculator = new BollingerBandsCalculator(parameters, nameof(CalculatorNameEnum.BBANDS));
            double[,] prices = Pricedata.GetPrices(30);

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert
            foreach (var timestamp in result.Results.Keys)
            {
                var bands = result.Results[timestamp];
                var lowerBand = bands.First(kvp => kvp.Key == nameof(TechnicalNamesEnum.LOWERBAND)).Value;
                var middleBand = bands.First(kvp => kvp.Key == nameof(TechnicalNamesEnum.MIDDLEBAND)).Value;
                Assert.True(lowerBand <= middleBand);
            }
        }

        [Fact]
        public void Calculate_ShouldHandleNullPrices()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { {
                    nameof(ParameterNamesEnum.Period), "20" },
                { nameof(ParameterNamesEnum.Multiplier), "2" } };


            var calculator = new BollingerBandsCalculator(parameters, nameof(CalculatorNameEnum.BBANDS));
            double[,] prices = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => calculator.Calculate(prices));
            Assert.Contains("prices", exception.Message);
        }

    }
}
