using HC.TechnicalCalculators.Src.Calculators.Volatility;
using HC.TechnicalCalculators.Src.Models;
using HC.TechnicalCalculators.Tests.Helpers;

namespace HC.TechnicalCalculators.Tests.Calculators.Volatility
{
    public class TrangeCalculatorTests
    {

        [Fact]
        public void Constructor_ShouldCreateInstance()
        {
            // Arrange
            var parameters = new Dictionary<string, string>();

            // Act
            var calculator = new TrangeCalculator(parameters, nameof(CalculatorNameEnum.TRANGE));

            // Assert
            Assert.NotNull(calculator);
        }

        [Fact]
        public void Calculate_ShouldThrowArgumentNullException_WhenPricesIsNull()
        {
            // Arrange
            var calculator = new TrangeCalculator(new Dictionary<string, string>(), nameof(CalculatorNameEnum.TRANGE));

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => calculator.Calculate(null));
            Assert.Contains("prices", exception.ParamName);
        }

        [Fact]
        public void Calculate_ShouldThrowArgumentException_WhenPricesHasInvalidColumns()
        {
            // Arrange
            var calculator = new TrangeCalculator(new Dictionary<string, string>(), nameof(CalculatorNameEnum.TRANGE));
            double[,] invalidPrices = new double[5, 3]; // Only 3 columns instead of required 6

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => calculator.Calculate(invalidPrices));
            Assert.Contains("columns", exception.Message);
        }

        [Fact]
        public void Calculate_ShouldReturnCorrectResults_WhenPricesAreValid()
        {
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.NA), "" } };
            // Arrange
            var calculator = new TrangeCalculator(parameters, nameof(CalculatorNameEnum.TRANGE));
            var prices = Pricedata.GetPrices(10);

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
            Assert.Equal(nameof(CalculatorNameEnum.TRANGE), result.Name);
            Assert.Equal(10, result.Results.Count);

            foreach (var timestamp in timestamps)
            {
                Assert.Contains(timestamp, result.Results.Keys);
            }

            // Check structure of results
            foreach (var entry in result.Results)
            {
                Assert.Single(entry.Value);
                Assert.Equal(nameof(TechnicalNamesEnum.TRANGE), entry.Value[0].Key);
                Assert.IsType<double>(entry.Value[0].Value);
            }
        }

        [Fact]
        public void Calculate_ShouldHandleEmptyPricesArray()
        {
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.NA), "" } };
            // Arrange
            var calculator = new TrangeCalculator(parameters, nameof(CalculatorNameEnum.TRANGE));
            double[,] emptyPrices = new double[0, 6];

            // Act
            var result = calculator.Calculate(emptyPrices, true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(nameof(CalculatorNameEnum.TRANGE), result.Name);
            Assert.Empty(result.Results);
        }

        [Fact]
        public void Calculate_ShouldProducePositiveValues()
        {
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.NA), "" } };
            // Arrange
            var calculator = new TrangeCalculator(parameters, nameof(CalculatorNameEnum.TRANGE));
            var prices = Pricedata.GetPrices(20);

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert
            // True Range should always be non-negative
            foreach (var entry in result.Results)
            {
                double trangeValue = entry.Value[0].Value;
                Assert.True(trangeValue >= 0, $"TRANGE value {trangeValue} should be non-negative");
            }
        }

        [Fact(Skip = "Work in progress")]
        public void Calculate_ShouldComputeTrueRangeCorrectly()
        {
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.NA), "" } };
            // Arrange
            var calculator = new TrangeCalculator(parameters, nameof(CalculatorNameEnum.TRANGE));

            // Create custom price data with known values
            double[,] prices = new double[5, 6];

            // timestamp, open, high, low, close, volume
            // First row has no previous close
            prices[0, 0] = 1000000; // timestamp
            prices[0, 1] = 100; // open
            prices[0, 2] = 110; // high
            prices[0, 3] = 90; // low
            prices[0, 4] = 105; // close
            prices[0, 5] = 1000; // volume

            // Subsequent rows
            prices[1, 0] = 1000001; // timestamp
            prices[1, 1] = 104; // open
            prices[1, 2] = 120; // high
            prices[1, 3] = 100; // low
            prices[1, 4] = 115; // close
            prices[1, 5] = 1000; // volume

            prices[2, 0] = 1000002; // timestamp
            prices[2, 1] = 116; // open
            prices[2, 2] = 125; // high
            prices[2, 3] = 105; // low
            prices[2, 4] = 110; // close
            prices[2, 5] = 1000; // volume

            prices[3, 0] = 1000003; // timestamp
            prices[3, 1] = 109; // open
            prices[3, 2] = 115; // high
            prices[3, 3] = 90; // low
            prices[3, 4] = 95; // close
            prices[3, 5] = 1000; // volume

            prices[4, 0] = 1000004; // timestamp
            prices[4, 1] = 94; // open
            prices[4, 2] = 100; // high
            prices[4, 3] = 85; // low
            prices[4, 4] = 90; // close
            prices[4, 5] = 1000; // volume

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert
            // Manually calculate TRANGE for each row and compare
            // TRANGE = MAX(High - Low, ABS(High - PrevClose), ABS(Low - PrevClose))

            // Row 0: TRANGE = High - Low (no previous close)
            double expected0 = prices[0, 2] - prices[0, 3]; // 110 - 90 = 20
            Assert.Equal(expected0, result.Results[1000000][0].Value, 6);

            // Row 1: TRANGE = MAX(High - Low, ABS(High - PrevClose), ABS(Low - PrevClose))
            double highMinusLow1 = prices[1, 2] - prices[1, 3]; // 120 - 100 = 20
            double absHighMinusPrevClose1 = Math.Abs(prices[1, 2] - prices[0, 4]); // |120 - 105| = 15
            double absLowMinusPrevClose1 = Math.Abs(prices[1, 3] - prices[0, 4]); // |100 - 105| = 5
            double expected1 = Math.Max(highMinusLow1, Math.Max(absHighMinusPrevClose1, absLowMinusPrevClose1));
            Assert.Equal(expected1, result.Results[1000001][0].Value, 6);

            // Row 2: TRANGE = MAX(High - Low, ABS(High - PrevClose), ABS(Low - PrevClose))
            double highMinusLow2 = prices[2, 2] - prices[2, 3]; // 125 - 105 = 20
            double absHighMinusPrevClose2 = Math.Abs(prices[2, 2] - prices[1, 4]); // |125 - 115| = 10
            double absLowMinusPrevClose2 = Math.Abs(prices[2, 3] - prices[1, 4]); // |105 - 115| = 10
            double expected2 = Math.Max(highMinusLow2, Math.Max(absHighMinusPrevClose2, absLowMinusPrevClose2));
            Assert.Equal(expected2, result.Results[1000002][0].Value, 6);

            // Row 3: TRANGE = MAX(High - Low, ABS(High - PrevClose), ABS(Low - PrevClose))
            double highMinusLow3 = prices[3, 2] - prices[3, 3]; // 115 - 90 = 25
            double absHighMinusPrevClose3 = Math.Abs(prices[3, 2] - prices[2, 4]); // |115 - 110| = 5
            double absLowMinusPrevClose3 = Math.Abs(prices[3, 3] - prices[2, 4]); // |90 - 110| = 20
            double expected3 = Math.Max(highMinusLow3, Math.Max(absHighMinusPrevClose3, absLowMinusPrevClose3));
            Assert.Equal(expected3, result.Results[1000003][0].Value, 6);

            // Row 4: TRANGE = MAX(High - Low, ABS(High - PrevClose), ABS(Low - PrevClose))
            double highMinusLow4 = prices[4, 2] - prices[4, 3]; // 100 - 85 = 15
            double absHighMinusPrevClose4 = Math.Abs(prices[4, 2] - prices[3, 4]); // |100 - 95| = 5
            double absLowMinusPrevClose4 = Math.Abs(prices[4, 3] - prices[3, 4]); // |85 - 95| = 10
            double expected4 = Math.Max(highMinusLow4, Math.Max(absHighMinusPrevClose4, absLowMinusPrevClose4));
            Assert.Equal(expected4, result.Results[1000004][0].Value, 6);
        }

        [Fact(Skip = "Work in progress")]
        public void Calculate_HigherVolatility_ShouldReturnHigherTRANGE()
        {
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.NA), "" } };
            // Arrange
            var calculator = new TrangeCalculator(parameters, nameof(CalculatorNameEnum.TRANGE));

            // Create two price datasets: one with low volatility, one with high
            double[,] lowVolatilityPrices = new double[5, 6];
            double[,] highVolatilityPrices = new double[5, 6];

            // Initialize both with the same baseline
            for (int i = 0; i < 5; i++)
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
            // Compare TRANGE values for corresponding timestamps
            for (int i = 0; i < 5; i++)
            {
                long timestamp = 1000000 + i;
                double lowVolTRange = lowVolResult.Results[timestamp][0].Value;
                double highVolTRange = highVolResult.Results[timestamp][0].Value;

                Assert.True(highVolTRange > lowVolTRange,
                    $"High volatility TRANGE ({highVolTRange}) should be greater than low volatility TRANGE ({lowVolTRange}) for timestamp {timestamp}");
            }
        }

        [Fact]
        public void Calculate_ShouldProduceConsistentResults_WhenCalledMultipleTimes()
        {
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.NA), "" } };
            // Arrange
            var calculator = new TrangeCalculator(parameters, nameof(CalculatorNameEnum.TRANGE));
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
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.NA), "" } };
            // Arrange
            var calculator = new TrangeCalculator(parameters, nameof(CalculatorNameEnum.TRANGE));
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
        public void Calculate_ShouldHandleIdenticalHighLowClose()
        {
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.NA), "" } };
            // Arrange
            var calculator = new TrangeCalculator(parameters, nameof(CalculatorNameEnum.TRANGE));

            // Create price data with identical high, low, and close values
            double[,] prices = new double[5, 6];

            for (int i = 0; i < 5; i++)
            {
                prices[i, 0] = 1000000 + i; // timestamp
                prices[i, 1] = 100; // open
                prices[i, 2] = 100; // high (identical)
                prices[i, 3] = 100; // low (identical)
                prices[i, 4] = 100; // close (identical)
                prices[i, 5] = 1000; // volume
            }

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert
            // For identical high/low/close, TRANGE should be 0 (no price movement)
            foreach (var timestamp in result.Results.Keys)
            {
                Assert.Equal(0, result.Results[timestamp][0].Value);
            }
        }

        [Fact(Skip = "Work in progress")]
        public void Calculate_ShouldHandlePriceGaps()
        {
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.NA), "" } };
            // Arrange
            var calculator = new TrangeCalculator(parameters, nameof(CalculatorNameEnum.TRANGE));

            // Create price data with a significant gap between sessions
            double[,] prices = new double[3, 6];

            // First day
            prices[0, 0] = 1000000; // timestamp
            prices[0, 1] = 100; // open
            prices[0, 2] = 105; // high
            prices[0, 3] = 95; // low
            prices[0, 4] = 103; // close
            prices[0, 5] = 1000; // volume

            // Second day with gap up
            prices[1, 0] = 1000001; // timestamp
            prices[1, 1] = 120; // open (gap up)
            prices[1, 2] = 125; // high
            prices[1, 3] = 115; // low
            prices[1, 4] = 122; // close
            prices[1, 5] = 1000; // volume

            // Third day with gap down
            prices[2, 0] = 1000002; // timestamp
            prices[2, 1] = 110; // open (gap down)
            prices[2, 2] = 115; // high
            prices[2, 3] = 105; // low
            prices[2, 4] = 108; // close
            prices[2, 5] = 1000; // volume

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert
            // First day TRANGE = high - low
            Assert.Equal(10, result.Results[1000000][0].Value); // 105 - 95 = 10

            // Second day TRANGE = MAX(high - low, |high - prevClose|, |low - prevClose|)
            // MAX(10, |125 - 103|, |115 - 103|) = MAX(10, 22, 12) = 22
            Assert.Equal(22, result.Results[1000001][0].Value);

            // Third day TRANGE = MAX(high - low, |high - prevClose|, |low - prevClose|)
            // MAX(10, |115 - 122|, |105 - 122|) = MAX(10, 7, 17) = 17
            Assert.Equal(17, result.Results[1000002][0].Value);
        }

        [Fact(Skip = "Work in progress")]
        public void Calculate_ShouldHandleZeroPrices()
        {
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.NA), "" } };
            // Arrange
            var calculator = new TrangeCalculator(parameters, nameof(CalculatorNameEnum.TRANGE));

            // Create price data with zero values
            double[,] prices = new double[3, 6];

            // First day - normal prices
            prices[0, 0] = 1000000; // timestamp
            prices[0, 1] = 100; // open
            prices[0, 2] = 105; // high
            prices[0, 3] = 95; // low
            prices[0, 4] = 103; // close
            prices[0, 5] = 1000; // volume

            // Second day - zero prices (might represent an error or no trading)
            prices[1, 0] = 1000001; // timestamp
            prices[1, 1] = 0; // open
            prices[1, 2] = 0; // high
            prices[1, 3] = 0; // low
            prices[1, 4] = 0; // close
            prices[1, 5] = 0; // volume

            // Third day - back to normal
            prices[2, 0] = 1000002; // timestamp
            prices[2, 1] = 100; // open
            prices[2, 2] = 110; // high
            prices[2, 3] = 90; // low
            prices[2, 4] = 105; // close
            prices[2, 5] = 1000; // volume

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert
            // First day TRANGE = high - low
            Assert.Equal(10, result.Results[1000000][0].Value); // 105 - 95 = 10

            // Second day TRANGE = MAX(high - low, |high - prevClose|, |low - prevClose|)
            // With zeros, this should be MAX(0, |0 - 103|, |0 - 103|) = 103
            Assert.Equal(103, result.Results[1000001][0].Value);

            // Third day TRANGE = MAX(high - low, |high - prevClose|, |low - prevClose|)
            // MAX(20, |110 - 0|, |90 - 0|) = MAX(20, 110, 90) = 110
            Assert.Equal(110, result.Results[1000002][0].Value);
        }

        [Fact]
        public void Calculate_TRANGE_ShouldRespondToVolatilityChanges()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.NA), "" } };

            var calculator = new TrangeCalculator(parameters, nameof(CalculatorNameEnum.TRANGE));

            // Create a custom price series with increasing then decreasing volatility
            double[,] prices = new double[15, 6];

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

            // Next 5 periods with high volatility
            for (int i = 5; i < 10; i++)
            {
                prices[i, 0] = 1000000 + i; // timestamp
                prices[i, 1] = 100; // open
                prices[i, 2] = 110; // high
                prices[i, 3] = 90; // low
                prices[i, 4] = 100; // close
                prices[i, 5] = 1000; // volume
            }

            // Last 5 periods with low volatility again
            for (int i = 10; i < 15; i++)
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
            // Get average TRANGE for each volatility segment
            double avgLowVol1 = 0;
            double avgHighVol = 0;
            double avgLowVol2 = 0;

            for (int i = 0; i < 5; i++)
                avgLowVol1 += result.Results[1000000 + i][0].Value;
            avgLowVol1 /= 5;

            for (int i = 5; i < 10; i++)
                avgHighVol += result.Results[1000000 + i][0].Value;
            avgHighVol /= 5;

            for (int i = 10; i < 15; i++)
                avgLowVol2 += result.Results[1000000 + i][0].Value;
            avgLowVol2 /= 5;

            // High volatility segment should have higher average TRANGE
            Assert.True(avgHighVol > avgLowVol1,
                $"High volatility average TRANGE ({avgHighVol}) should be greater than first low volatility average ({avgLowVol1})");
            Assert.True(avgHighVol > avgLowVol2,
                $"High volatility average TRANGE ({avgHighVol}) should be greater than second low volatility average ({avgLowVol2})");
        }
    }
}
