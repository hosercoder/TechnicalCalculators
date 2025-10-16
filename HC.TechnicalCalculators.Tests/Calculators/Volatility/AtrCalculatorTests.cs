using HC.TechnicalCalculators.Src.Calculators.Volatility;
using HC.TechnicalCalculators.Src.Models;
using HC.TechnicalCalculators.Tests.Helpers;

namespace HC.TechnicalCalculators.Tests.Calculators.Volatility
{
    public class AtrCalculatorTests
    {

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenPeriodParameterIsMissing()
        {
            // Arrange
            var parameters = new Dictionary<string, string>();

            // Act
            var calculator = new AtrCalculator(parameters, nameof(CalculatorNameEnum.ATR));

            // Assert
            Assert.NotNull(calculator);
            // The calculator should set the default period value
        }

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenPeriodParameterIsProvided()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "14" } };

            // Act
            var calculator = new AtrCalculator(parameters, nameof(CalculatorNameEnum.ATR));

            // Assert
            Assert.NotNull(calculator);
        }

        [Fact]
        public void Calculate_ShouldThrowArgumentNullException_WhenPricesIsNull()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "14" } };
            var calculator = new AtrCalculator(parameters, nameof(CalculatorNameEnum.ATR));

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => calculator.Calculate(null));
            Assert.Contains("prices", exception.ParamName);
        }

        [Fact]
        public void Calculate_ShouldThrowFormatException_WhenPeriodIsNotAnInteger()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "invalid" } };
            var calculator = new AtrCalculator(parameters, nameof(CalculatorNameEnum.ATR));
            var prices = Pricedata.GetPrices(20);

            // Act & Assert
            Assert.Throws<FormatException>(() => calculator.Calculate(prices, true));
        }

        [Fact]
        public void Calculate_ShouldThrowArgumentException_WhenPricesArrayIsTooShortForPeriod()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "20" } };
            var calculator = new AtrCalculator(parameters, nameof(CalculatorNameEnum.ATR));
            var prices = Pricedata.GetPrices(10); // Less data points than the period

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => calculator.Calculate(prices));
            Assert.Contains("Not enough data points for ATR calculation. Minimum required: 20", exception.Message);
        }

        [Fact]
        public void Calculate_ShouldReturnCorrectResults_WhenPricesAreValid()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "5" } };
            var calculator = new AtrCalculator(parameters, nameof(CalculatorNameEnum.ATR));
            var prices = Pricedata.GetPrices(20);

            // Extract timestamps for verification
            long[] timestamps = new long[prices.GetLength(0)];
            for (int i = 0; i < prices.GetLength(0); i++)
            {
                timestamps[i] = (long)prices[i, 0];
            }

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(nameof(CalculatorNameEnum.ATR), result.Name);
            Assert.Equal(20, result.Results.Count);

            // Check all timestamps are present
            foreach (var timestamp in timestamps)
            {
                Assert.Contains(timestamp, result.Results.Keys);
            }

            // Check structure of results
            foreach (var entry in result.Results)
            {
                Assert.Single(entry.Value);
                Assert.Equal(nameof(TechnicalNamesEnum.ATR), entry.Value[0].Key);
                Assert.IsType<double>(entry.Value[0].Value);
            }
        }

        [Fact]
        public void Calculate_ShouldReturnPositiveValues_ForATR()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "5" } };
            var calculator = new AtrCalculator(parameters, nameof(CalculatorNameEnum.ATR));
            var prices = Pricedata.GetPrices(20);

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert
            // ATR should always be positive as it measures volatility
            foreach (var entry in result.Results)
            {
                double atrValue = entry.Value[0].Value;
                Assert.True(atrValue >= 0, $"ATR value {atrValue} should be non-negative");
            }
        }

        [Fact]
        public void Calculate_HigherVolatility_ShouldReturnHigherATR()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "5" } };
            var calculator = new AtrCalculator(parameters, nameof(CalculatorNameEnum.ATR));

            // Create two price datasets: one with low volatility, one with high
            double[,] lowVolatilityPrices = new double[10, 6];
            double[,] highVolatilityPrices = new double[10, 6];

            // Initialize both with the same baseline
            for (int i = 0; i < 10; i++)
            {
                // timestamp, open, high, low, close, volume
                lowVolatilityPrices[i, 0] = 1000000 + i;
                highVolatilityPrices[i, 0] = 1000000 + i;

                lowVolatilityPrices[i, 5] = 1000; // volume
                highVolatilityPrices[i, 5] = 1000; // volume

                // Low volatility - prices close together
                lowVolatilityPrices[i, 1] = 100; // open
                lowVolatilityPrices[i, 2] = 101; // high
                lowVolatilityPrices[i, 3] = 99; // low
                lowVolatilityPrices[i, 4] = 100; // close

                // High volatility - prices far apart
                highVolatilityPrices[i, 1] = 100; // open
                highVolatilityPrices[i, 2] = 120; // high
                highVolatilityPrices[i, 3] = 80; // low
                highVolatilityPrices[i, 4] = 100; // close
            }

            // Act
            var lowVolResult = calculator.Calculate(lowVolatilityPrices, true);
            var highVolResult = calculator.Calculate(highVolatilityPrices, true);

            // Assert
            // We should have at least one entry where we can compare ATR values
            bool foundComparableValues = false;
            foreach (var timestamp in lowVolResult.Results.Keys.Intersect(highVolResult.Results.Keys))
            {
                double lowVolATR = lowVolResult.Results[timestamp][0].Value;
                double highVolATR = highVolResult.Results[timestamp][0].Value;

                // Skip entries with zero values (initial calculation period)
                if (lowVolATR > 0 && highVolATR > 0)
                {
                    Assert.True(highVolATR > lowVolATR,
                        $"High volatility ATR ({highVolATR}) should be greater than low volatility ATR ({lowVolATR})");
                    foundComparableValues = true;
                    break;
                }
            }

            Assert.True(foundComparableValues, "Should find at least one pair of comparable ATR values");
        }

        [Fact]
        public void Calculate_DifferentPeriods_ShouldProduceDifferentResults()
        {
            // Arrange
            var shortPeriodParams = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "5" } };
            var longPeriodParams = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "14" } };

            var shortPeriodCalc = new AtrCalculator(shortPeriodParams, nameof(CalculatorNameEnum.ATR));
            var longPeriodCalc = new AtrCalculator(longPeriodParams, nameof(CalculatorNameEnum.ATR));

            var prices = Pricedata.GetPrices(30);

            // Act
            var shortPeriodResult = shortPeriodCalc.Calculate(prices, true);
            var longPeriodResult = longPeriodCalc.Calculate(prices, true);

            // Assert
            // Different periods should produce different ATR values
            bool foundDifference = false;
            var commonTimestamps = shortPeriodResult.Results.Keys.Intersect(longPeriodResult.Results.Keys);
            foreach (var timestamp in commonTimestamps)
            {
                double shortPeriodATR = shortPeriodResult.Results[timestamp][0].Value;
                double longPeriodATR = longPeriodResult.Results[timestamp][0].Value;

                // Skip entries with zero values (initial calculation period)
                if (shortPeriodATR > 0 && longPeriodATR > 0 &&
                    Math.Abs(shortPeriodATR - longPeriodATR) > 0.0001)
                {
                    foundDifference = true;
                    break;
                }
            }

            Assert.True(foundDifference, "Different periods should produce different ATR values");
        }

        [Fact]
        public void Calculate_ShouldProduceConsistentResults_WhenCalledMultipleTimes()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "10" } };
            var calculator = new AtrCalculator(parameters, nameof(CalculatorNameEnum.ATR));
            var prices = Pricedata.GetPrices(20);

            // Act
            var result1 = calculator.Calculate(prices, true);
            var result2 = calculator.Calculate(prices, true);

            // Assert
            Assert.Equal(result1.Results.Count, result2.Results.Count);

            foreach (var key in result1.Results.Keys)
            {
                Assert.Equal(result1.Results[key][0].Value, result2.Results[key][0].Value);
            }
        }

        [Fact]
        public void Calculate_ShouldNotModifyInputPricesArray()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "10" } };
            var calculator = new AtrCalculator(parameters, nameof(CalculatorNameEnum.ATR));
            var prices = Pricedata.GetPrices(20);

            // Create a copy of the prices for comparison
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
        public void Calculate_ShouldHandleConstantPrices()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "5" } };
            var calculator = new AtrCalculator(parameters, nameof(CalculatorNameEnum.ATR));

            // Create price data with constant values
            double[,] prices = new double[10, 6];
            for (int i = 0; i < 10; i++)
            {
                prices[i, 0] = 1000000 + i; // timestamp
                prices[i, 1] = 100; // open
                prices[i, 2] = 100; // high (same as open/close)
                prices[i, 3] = 100; // low (same as open/close)
                prices[i, 4] = 100; // close
                prices[i, 5] = 1000; // volume
            }

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert
            // For constant prices, ATR should approach zero after the initial period
            var lastFewTimestamps = result.Results.Keys.OrderByDescending(k => k).Take(3);
            foreach (var timestamp in lastFewTimestamps)
            {
                Assert.True(result.Results[timestamp][0].Value == 0,
                    $"ATR for constant prices should be 0, got {result.Results[timestamp][0].Value}");
            }
        }
    }
}
