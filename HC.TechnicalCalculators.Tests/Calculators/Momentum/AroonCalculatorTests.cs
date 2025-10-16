using HC.TechnicalCalculators.Src.Calculators.Momentum;
using HC.TechnicalCalculators.Src.Models;
using HC.TechnicalCalculators.Tests.Helpers;

namespace HC.TechnicalCalculators.Tests.Calculators.Momentum
{
    public class AroonCalculatorTests
    {

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenPeriodParameterIsPresent()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.Period), "14" }
            };

            // Act
            var calculator = new AroonCalculator(parameters, nameof(CalculatorNameEnum.AROON));

            // Assert
            Assert.NotNull(calculator);
        }

        [Fact]
        public void Calculate_ShouldThrowArgumentException_WhenPeriodIsNotAValidInteger()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.Period), "invalid" }
            };
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new AroonCalculator(parameters, nameof(CalculatorNameEnum.AROON)));
            Assert.Equal("Period must be between 2 and 100.", exception.Message);
        }

        [Fact]
        public void Calculate_ShouldReturnValidResults_WithTypicalParameters()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.Period), "14" }
            };
            var calculator = new AroonCalculator(parameters, nameof(CalculatorNameEnum.AROON));
            var prices = Pricedata.GetPrices(100); // Need enough data for meaningful calculation

            // Act
            var results = calculator.Calculate(prices);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(nameof(CalculatorNameEnum.AROON), results.Name);
            Assert.NotEmpty(results.Results);

            // Verify structure of results
            foreach (var result in results.Results)
            {
                Assert.Equal(2, result.Value.Length); // Should have both AroonUp and AroonDown values

                // Check if AroonUp and AroonDown are present
                Assert.Contains(result.Value, kv => kv.Key == nameof(TechnicalNamesEnum.AROONUP));
                Assert.Contains(result.Value, kv => kv.Key == nameof(TechnicalNamesEnum.AROONDOWN));

                // Check if values are within expected range (0-100)
                var aroonUpValue = result.Value.First(kv => kv.Key == nameof(TechnicalNamesEnum.AROONUP)).Value;
                var aroonDownValue = result.Value.First(kv => kv.Key == nameof(TechnicalNamesEnum.AROONDOWN)).Value;

                Assert.True(aroonUpValue >= 0 && aroonUpValue <= 100,
                    $"AroonUp value {aroonUpValue} should be between 0 and 100");
                Assert.True(aroonDownValue >= 0 && aroonDownValue <= 100,
                    $"AroonDown value {aroonDownValue} should be between 0 and 100");
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
            var calculator = new AroonCalculator(parameters, nameof(CalculatorNameEnum.AROON));
            var prices = Pricedata.GetPrices(500); // Large dataset

            // Act
            var results = calculator.Calculate(prices);

            // Assert
            Assert.NotNull(results);
            Assert.NotEmpty(results.Results);
        }

        [Fact]
        public void Calculate_ShouldHandleSmallPeriod()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.Period), "1" } // Small period
            };
            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new AroonCalculator(parameters, nameof(CalculatorNameEnum.AROON)));
            Assert.Equal("Period must be between 2 and 100.", exception.Message);
        }

        [Fact(Skip = "Working in progress")]
        public void Calculate_ShouldReturnExpectedAroonValues_ForKnownData()
        {
            // Arrange: Test data from the mathematical proof
            // 7 periods
            double[,] prices = new double[7, 6];
            // timestamp, open, high, low, close, volume
            double[] highs = { 10, 12, 14, 13, 15, 11, 16 };
            double[] lows = { 5, 6, 7, 8, 7, 6, 5 };
            for (int i = 0; i < 7; i++)
            {
                prices[i, 0] = 1000 + i; // timestamp
                prices[i, 1] = 0;        // open
                prices[i, 2] = highs[i]; // high
                prices[i, 3] = lows[i];  // low
                prices[i, 4] = 0;        // close
                prices[i, 5] = 0;        // volume
            }

            var parameters = new Dictionary<string, string> { { "period", "7" } };
            var calculator = new AroonCalculator(parameters, nameof(CalculatorNameEnum.AROON));

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert: At index 6, highest high and lowest low are both at index 6
            // So periodsSinceHigh = 0, periodsSinceLow = 0
            // AroonUp = 100 * (7 - 0) / 7 = 100
            // AroonDown = 100 * (7 - 0) / 7 = 100
            long lastTimestamp = (long)prices[6, 0];
            var lastResult = result.Results[lastTimestamp];

            Assert.Equal(nameof(TechnicalNamesEnum.AROONUP), lastResult[0].Key);
            Assert.Equal(nameof(TechnicalNamesEnum.AROONDOWN), lastResult[1].Key);

            Assert.Equal(100.0, lastResult[0].Value, 2); // AroonUp
            Assert.Equal(100.0, lastResult[1].Value, 2); // AroonDown

            // Additional: At index 5, highest high is at index 4 (value 15), lowest low is at index 0 (value 5)
            // periodsSinceHigh = 5 - 4 = 1, periodsSinceLow = 5 - 0 = 5
            // AroonUp = 100 * (7 - 1) / 7 = 85.71
            // AroonDown = 100 * (7 - 5) / 7 = 28.57
            long timestamp5 = (long)prices[5, 0];
            var result5 = result.Results[timestamp5];

            Assert.Equal(nameof(TechnicalNamesEnum.AROONUP), result5[0].Key);
            Assert.Equal(nameof(TechnicalNamesEnum.AROONDOWN), result5[1].Key);

            Assert.Equal(85.71, result5[0].Value, 2); // AroonUp
            Assert.Equal(28.57, result5[1].Value, 2); // AroonDown
        }
    }
}
