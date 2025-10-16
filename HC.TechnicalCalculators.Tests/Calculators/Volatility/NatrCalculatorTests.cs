using HC.TechnicalCalculators.Src.Calculators.Volatility;
using HC.TechnicalCalculators.Src.Models;
using HC.TechnicalCalculators.Tests.Helpers;

namespace HC.TechnicalCalculators.Tests.Calculators.Volatility
{
    public class NatrCalculatorTests
    {
        [Fact]
        public void Constructor_ShouldCreateInstance_WhenPeriodParameterIsMissing()
        {
            // Arrange
            var parameters = new Dictionary<string, string>();

            // Act
            var calculator = new NatrCalculator(parameters, nameof(CalculatorNameEnum.NATR));

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
            var calculator = new NatrCalculator(parameters, nameof(CalculatorNameEnum.NATR));

            // Assert
            Assert.NotNull(calculator);
        }

        [Fact]
        public void Calculate_ShouldThrowArgumentNullException_WhenPricesIsNull()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "14" } };
            var calculator = new NatrCalculator(parameters, nameof(CalculatorNameEnum.NATR));

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => calculator.Calculate(null));
            Assert.Contains("prices", exception.ParamName);
        }

        [Fact]
        public void Calculate_ShouldThrowFormatException_WhenPeriodIsNotAnInteger()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "invalid" } };
            var calculator = new NatrCalculator(parameters, nameof(CalculatorNameEnum.NATR));
            var prices = Pricedata.GetPrices(20);

            // Act & Assert
            Assert.Throws<FormatException>(() => calculator.Calculate(prices, true));
        }

        [Fact]
        public void Calculate_ShouldThrowArgumentException_WhenPricesArrayIsTooShortForPeriod()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "20" } };
            var calculator = new NatrCalculator(parameters, nameof(CalculatorNameEnum.NATR));
            var prices = Pricedata.GetPrices(10); // Less data points than the period

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => calculator.Calculate(prices));
            Assert.Contains("Not enough data points for NATR calculation. Minimum required: 20", exception.Message);
        }

        [Fact]
        public void Calculate_ShouldReturnCorrectResults_WhenPricesAreValid()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "5" } };
            var calculator = new NatrCalculator(parameters, nameof(CalculatorNameEnum.NATR));
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
            Assert.Equal(nameof(CalculatorNameEnum.NATR), result.Name);
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
                Assert.Equal(nameof(TechnicalNamesEnum.NATR), entry.Value[0].Key);
                Assert.IsType<double>(entry.Value[0].Value);
            }
        }

        [Fact]
        public void Calculate_ShouldReturnPercentageValues()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "5" } };
            var calculator = new NatrCalculator(parameters, nameof(CalculatorNameEnum.NATR));
            var prices = Pricedata.GetPrices(20);

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert
            // NATR returns percentage values (typically between 0 and 100)
            foreach (var entry in result.Results)
            {
                double natrValue = entry.Value[0].Value;
                // Skip zeros that may appear during initial calculation
                if (natrValue != 0)
                {
                    // Percentage value should be reasonable (not extremely high)
                    Assert.True(natrValue >= 0 && natrValue < 100,
                        $"NATR value {natrValue} should be a percentage value between 0 and 100");
                }
            }
        }

        [Fact]
        public void Calculate_HigherVolatility_ShouldReturnHigherNATR()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "5" } };
            var calculator = new NatrCalculator(parameters, nameof(CalculatorNameEnum.NATR));

            // Create two price datasets: one with low volatility, one with high
            double[,] lowVolatilityPrices = new double[15, 6];
            double[,] highVolatilityPrices = new double[15, 6];

            // Initialize both with the same baseline
            for (int i = 0; i < 15; i++)
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
            // We should have at least one entry where we can compare NATR values
            bool foundComparableValues = false;
            foreach (var timestamp in lowVolResult.Results.Keys.Intersect(highVolResult.Results.Keys))
            {
                double lowVolNATR = lowVolResult.Results[timestamp][0].Value;
                double highVolNATR = highVolResult.Results[timestamp][0].Value;

                // Skip entries with zero values (initial calculation period)
                if (lowVolNATR > 0 && highVolNATR > 0)
                {
                    Assert.True(highVolNATR > lowVolNATR,
                        $"High volatility NATR ({highVolNATR}) should be greater than low volatility NATR ({lowVolNATR})");
                    foundComparableValues = true;
                    break;
                }
            }

            Assert.True(foundComparableValues, "Should find at least one pair of comparable NATR values");
        }

        [Fact]
        public void Calculate_ShouldProduceNormalizedValues_RelativeToClosePrices()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "5" } };
            var calculator = new NatrCalculator(parameters, nameof(CalculatorNameEnum.NATR));
            var atrCalculator = new AtrCalculator(parameters, nameof(CalculatorNameEnum.ATR));

            var prices = Pricedata.GetPrices(20);

            // Act
            var natrResult = calculator.Calculate(prices, true);
            var atrResult = atrCalculator.Calculate(prices, true);

            // Assert
            // NATR is ATR normalized by close price (ATR/close * 100)
            // Check for at least one data point where we can verify the calculation
            bool validationPerformed = false;

            foreach (var timestamp in natrResult.Results.Keys.Intersect(atrResult.Results.Keys))
            {
                double natrValue = natrResult.Results[timestamp][0].Value;
                double atrValue = atrResult.Results[timestamp][0].Value;

                // Find the corresponding close price for this timestamp
                int rowIndex = -1;
                for (int i = 0; i < prices.GetLength(0); i++)
                {
                    if ((long)prices[i, 0] == timestamp)
                    {
                        rowIndex = i;
                        break;
                    }
                }

                if (rowIndex >= 0)
                {
                    double closePrice = prices[rowIndex, 4];

                    // Skip entries with zero values or zero close price
                    if (atrValue > 0 && closePrice > 0)
                    {
                        // Expected NATR = (ATR/Close) * 100
                        double expectedNatr = (atrValue / closePrice) * 100;
                        Assert.Equal(expectedNatr, natrValue, 3); // Allow small floating point differences
                        validationPerformed = true;
                        break;
                    }
                }
            }

            Assert.True(validationPerformed, "Should be able to verify NATR calculation against ATR");
        }

        [Fact]
        public void Calculate_DifferentPeriods_ShouldProduceDifferentResults()
        {
            // Arrange
            var shortPeriodParams = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "5" } };
            var longPeriodParams = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "14" } };

            var shortPeriodCalc = new NatrCalculator(shortPeriodParams, nameof(CalculatorNameEnum.NATR));
            var longPeriodCalc = new NatrCalculator(longPeriodParams, nameof(CalculatorNameEnum.NATR));

            var prices = Pricedata.GetPrices(30);

            // Act
            var shortPeriodResult = shortPeriodCalc.Calculate(prices, true);
            var longPeriodResult = longPeriodCalc.Calculate(prices, true);

            // Assert
            // Different periods should produce different NATR values
            bool foundDifference = false;
            var commonTimestamps = shortPeriodResult.Results.Keys.Intersect(longPeriodResult.Results.Keys);
            foreach (var timestamp in commonTimestamps)
            {
                double shortPeriodNATR = shortPeriodResult.Results[timestamp][0].Value;
                double longPeriodNATR = longPeriodResult.Results[timestamp][0].Value;

                // Skip entries with zero values (initial calculation period)
                if (shortPeriodNATR > 0 && longPeriodNATR > 0 &&
                    Math.Abs(shortPeriodNATR - longPeriodNATR) > 0.0001)
                {
                    foundDifference = true;
                    break;
                }
            }

            Assert.True(foundDifference, "Different periods should produce different NATR values");
        }

        [Fact]
        public void Calculate_ShouldProduceConsistentResults_WhenCalledMultipleTimes()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "10" } };
            var calculator = new NatrCalculator(parameters, nameof(CalculatorNameEnum.NATR));
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
            var calculator = new NatrCalculator(parameters, nameof(CalculatorNameEnum.NATR));
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
            var calculator = new NatrCalculator(parameters, nameof(CalculatorNameEnum.NATR));

            // Create price data with constant values
            double[,] prices = new double[15, 6];
            double constantPrice = 100.0;

            for (int i = 0; i < 15; i++)
            {
                prices[i, 0] = 1000000 + i; // timestamp
                prices[i, 1] = constantPrice; // open
                prices[i, 2] = constantPrice; // high
                prices[i, 3] = constantPrice; // low
                prices[i, 4] = constantPrice; // close
                prices[i, 5] = 1000; // volume
            }

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert
            // For constant prices, NATR should be zero after the initial period
            var lastFewTimestamps = result.Results.Keys.OrderByDescending(k => k).Take(5);
            foreach (var timestamp in lastFewTimestamps)
            {
                Assert.True(result.Results[timestamp][0].Value == 0,
                    $"NATR for constant prices should be 0, got {result.Results[timestamp][0].Value}");
            }
        }

        [Fact(Skip = "Work in progress")]
        public void Calculate_NATR_ShouldRespond_ToVolatilityChanges()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Period), "3" } };
            var calculator = new NatrCalculator(parameters, nameof(CalculatorNameEnum.NATR));

            // Create a custom price series with increasing then decreasing volatility
            double[,] prices = new double[20, 6];

            // First 5 periods with low volatility
            for (int i = 0; i < 5; i++)
            {
                prices[i, 0] = 1000000 + i; // timestamp
                prices[i, 1] = 100; // open
                prices[i, 2] = 101; // high
                prices[i, 3] = 99; // low
                prices[i, 4] = 100; // close
                prices[i, 5] = 1000; // volume
            }

            // Next 5 periods with medium volatility
            for (int i = 5; i < 10; i++)
            {
                prices[i, 0] = 1000000 + i; // timestamp
                prices[i, 1] = 100; // open
                prices[i, 2] = 105; // high
                prices[i, 3] = 95; // low
                prices[i, 4] = 100; // close
                prices[i, 5] = 1000; // volume
            }

            // Next 5 periods with high volatility
            for (int i = 10; i < 15; i++)
            {
                prices[i, 0] = 1000000 + i; // timestamp
                prices[i, 1] = 100; // open
                prices[i, 2] = 110; // high
                prices[i, 3] = 90; // low
                prices[i, 4] = 100; // close
                prices[i, 5] = 1000; // volume
            }

            // Last 5 periods with low volatility again
            for (int i = 15; i < 20; i++)
            {
                prices[i, 0] = 1000000 + i; // timestamp
                prices[i, 1] = 100; // open
                prices[i, 2] = 101; // high
                prices[i, 3] = 99; // low
                prices[i, 4] = 100; // close
                prices[i, 5] = 1000; // volume
            }

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert
            // Get NATR values for comparison at stable points after each volatility change
            double lowVolNATR1 = result.Results[(long)1000004][0].Value;
            double mediumVolNATR = result.Results[(long)1000009][0].Value;
            double highVolNATR = result.Results[(long)1000014][0].Value;
            double lowVolNATR2 = result.Results[(long)1000019][0].Value;

            // Verify volatility progression
            Assert.True(mediumVolNATR > lowVolNATR1,
                $"Medium volatility NATR ({mediumVolNATR}) should be greater than first low volatility NATR ({lowVolNATR1})");
            Assert.True(highVolNATR > mediumVolNATR,
                $"High volatility NATR ({highVolNATR}) should be greater than medium volatility NATR ({mediumVolNATR})");
            Assert.True(highVolNATR > lowVolNATR2,
                $"High volatility NATR ({highVolNATR}) should be greater than second low volatility NATR ({lowVolNATR2})");
        }
    }
}
