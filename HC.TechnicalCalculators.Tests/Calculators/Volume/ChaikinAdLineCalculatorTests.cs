using HC.TechnicalCalculators.Src.Calculators.Volume;
using HC.TechnicalCalculators.Src.Models;
using HC.TechnicalCalculators.Tests.Helpers;

namespace HC.TechnicalCalculators.Tests.Calculators.Volume
{
    public class ChaikinAdLineCalculatorTests
    {
        [Fact]
        public void Constructor_ShouldCreateInstance()
        {
            // Arrange
            var parameters = new Dictionary<string, string>();

            // Act
            var calculator = new ChaikinAdLineCalculator(parameters, nameof(CalculatorNameEnum.CHAIKINADLINE));

            // Assert
            Assert.NotNull(calculator);
        }

        [Fact]
        public void Calculate_ShouldThrowArgumentNullException_WhenPricesIsNull()
        {
            // Arrange
            var calculator = new ChaikinAdLineCalculator(new Dictionary<string, string>(), nameof(CalculatorNameEnum.CHAIKINADLINE));

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => calculator.Calculate(null));
            Assert.Contains("prices", exception.ParamName);
        }

        [Fact]
        public void Calculate_ShouldThrowArgumentException_WhenPricesHasInvalidColumns()
        {
            // Arrange
            var calculator = new ChaikinAdLineCalculator(new Dictionary<string, string>(), nameof(CalculatorNameEnum.CHAIKINADLINE));
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
            var calculator = new ChaikinAdLineCalculator(parameters, nameof(CalculatorNameEnum.CHAIKINADLINE));
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
            Assert.Equal(nameof(CalculatorNameEnum.CHAIKINADLINE), result.Name);
            Assert.Equal(10, result.Results.Count);

            foreach (var timestamp in timestamps)
            {
                Assert.Contains(timestamp, result.Results.Keys);
            }

            // Check structure of results
            foreach (var entry in result.Results)
            {
                Assert.Single(entry.Value);
                Assert.Equal(nameof(TechnicalNamesEnum.CHAIKINADLINE), entry.Value[0].Key);
                Assert.IsType<double>(entry.Value[0].Value);
            }
        }

        [Fact]
        public void Calculate_ShouldHandleEmptyPricesArray()
        {
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.NA), "" } };
            // Arrange
            var calculator = new ChaikinAdLineCalculator(parameters, nameof(CalculatorNameEnum.CHAIKINADLINE));
            double[,] emptyPrices = new double[0, 6];

            // Act
            var result = calculator.Calculate(emptyPrices, true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(nameof(CalculatorNameEnum.CHAIKINADLINE), result.Name);
            Assert.Empty(result.Results);
        }

        [Fact]
        public void Calculate_ShouldAccumulateLine_WhenVolumeIncreases()
        {
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.NA), "" } };
            // Arrange
            var calculator = new ChaikinAdLineCalculator(parameters, nameof(CalculatorNameEnum.CHAIKINADLINE));

            // Create custom price data with increasing buying pressure
            double[,] prices = new double[5, 6];

            // Create price data where price closes higher in the range with increasing volume
            for (int i = 0; i < 5; i++)
            {
                prices[i, 0] = 1000000 + i; // timestamp
                prices[i, 1] = 100; // open
                prices[i, 2] = 110; // high
                prices[i, 3] = 90; // low
                prices[i, 4] = 108; // close (near high - buying pressure)
                prices[i, 5] = 1000 * (i + 1); // increasing volume
            }

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert
            // Chaikin AD Line should be accumulating (increasing)
            double previousValue = result.Results[1000000][0].Value;
            bool foundIncreasingValue = false;

            for (int i = 1; i < 5; i++)
            {
                double currentValue = result.Results[1000000 + i][0].Value;
                if (currentValue > previousValue)
                {
                    foundIncreasingValue = true;
                    break;
                }
                previousValue = currentValue;
            }

            Assert.True(foundIncreasingValue, "Chaikin AD Line should increase with buying pressure and increasing volume");
        }

        [Fact]
        public void Calculate_ShouldDecumulateLine_WhenSellingPressureIncreases()
        {
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.NA), "" } };
            // Arrange
            var calculator = new ChaikinAdLineCalculator(parameters, nameof(CalculatorNameEnum.CHAIKINADLINE));

            // Create custom price data with increasing selling pressure
            double[,] prices = new double[5, 6];

            // Create price data where price closes lower in the range with increasing volume
            for (int i = 0; i < 5; i++)
            {
                prices[i, 0] = 1000000 + i; // timestamp
                prices[i, 1] = 100; // open
                prices[i, 2] = 110; // high
                prices[i, 3] = 90; // low
                prices[i, 4] = 92; // close (near low - selling pressure)
                prices[i, 5] = 1000 * (i + 1); // increasing volume
            }

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert
            // Chaikin AD Line should be decreasing with selling pressure
            double previousValue = result.Results[1000000][0].Value;
            bool foundDecreasingValue = false;

            for (int i = 1; i < 5; i++)
            {
                double currentValue = result.Results[1000000 + i][0].Value;
                if (currentValue < previousValue)
                {
                    foundDecreasingValue = true;
                    break;
                }
                previousValue = currentValue;
            }

            Assert.True(foundDecreasingValue, "Chaikin AD Line should decrease with selling pressure and increasing volume");
        }

        [Fact]
        public void Calculate_ShouldBeSensitiveToVolumeChanges()
        {
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.NA), "" } };
            // Arrange
            var calculator = new ChaikinAdLineCalculator(parameters, nameof(CalculatorNameEnum.CHAIKINADLINE));

            // Create two price datasets with identical price action but different volumes
            double[,] lowVolumeData = new double[5, 6];
            double[,] highVolumeData = new double[5, 6];

            for (int i = 0; i < 5; i++)
            {
                // Same price action
                lowVolumeData[i, 0] = highVolumeData[i, 0] = 1000000 + i; // timestamp
                lowVolumeData[i, 1] = highVolumeData[i, 1] = 100; // open
                lowVolumeData[i, 2] = highVolumeData[i, 2] = 110; // high
                lowVolumeData[i, 3] = highVolumeData[i, 3] = 90; // low
                lowVolumeData[i, 4] = highVolumeData[i, 4] = 105; // close

                // Different volumes
                lowVolumeData[i, 5] = 1000; // low volume
                highVolumeData[i, 5] = 10000; // high volume
            }

            // Act
            var lowVolumeResult = calculator.Calculate(lowVolumeData, true);
            var highVolumeResult = calculator.Calculate(highVolumeData, true);

            // Assert
            // The magnitude of changes should be proportional to volume
            double lowVolumeDelta = Math.Abs(lowVolumeResult.Results[1000004][0].Value - lowVolumeResult.Results[1000000][0].Value);
            double highVolumeDelta = Math.Abs(highVolumeResult.Results[1000004][0].Value - highVolumeResult.Results[1000000][0].Value);

            Assert.True(highVolumeDelta > lowVolumeDelta,
                $"Higher volume should create larger changes in AD Line (high: {highVolumeDelta}, low: {lowVolumeDelta})");
        }

        [Fact]
        public void Calculate_ShouldBeUnaffectedByZeroVolume()
        {
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.NA), "" } };
            // Arrange
            var calculator = new ChaikinAdLineCalculator(parameters, nameof(CalculatorNameEnum.CHAIKINADLINE));

            // Create price data with some zero volume entries
            double[,] prices = new double[5, 6];

            for (int i = 0; i < 5; i++)
            {
                prices[i, 0] = 1000000 + i; // timestamp
                prices[i, 1] = 100; // open
                prices[i, 2] = 110; // high
                prices[i, 3] = 90; // low
                prices[i, 4] = 105; // close
                prices[i, 5] = (i % 2 == 0) ? 0 : 1000; // alternate between 0 and normal volume
            }

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert
            // Verify days with zero volume don't change the AD Line
            for (int i = 1; i < 5; i++)
            {
                if (prices[i, 5] == 0) // Zero volume day
                {
                    Assert.Equal(
                        result.Results[1000000 + i - 1][0].Value,
                        result.Results[1000000 + i][0].Value,
                        8); // Compare with precision
                }
            }
        }

        [Fact]
        public void Calculate_ShouldProduceConsistentResults_WhenCalledMultipleTimes()
        {
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.NA), "" } };
            // Arrange
            var calculator = new ChaikinAdLineCalculator(parameters, nameof(CalculatorNameEnum.CHAIKINADLINE));
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
            var calculator = new ChaikinAdLineCalculator(parameters, nameof(CalculatorNameEnum.CHAIKINADLINE));
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
        public void Calculate_ShouldHandleIdenticalHighLowCloseValues()
        {
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.NA), "" } };
            // Arrange
            var calculator = new ChaikinAdLineCalculator(parameters, nameof(CalculatorNameEnum.CHAIKINADLINE));

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
            // With identical high/low/close, money flow multiplier is 0
            // So AD Line should remain flat after the first value
            double firstValue = result.Results[1000000][0].Value;
            for (int i = 1; i < 5; i++)
            {
                Assert.Equal(firstValue, result.Results[1000000 + i][0].Value, 8);
            }
        }

        [Fact]
        public void Calculate_ShouldHandleExtremeValues()
        {
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.NA), "" } };
            // Arrange
            var calculator = new ChaikinAdLineCalculator(parameters, nameof(CalculatorNameEnum.CHAIKINADLINE));

            // Create price data with extreme values
            double[,] prices = new double[5, 6];

            // First entry - normal values
            prices[0, 0] = 1000000; // timestamp
            prices[0, 1] = 100; // open
            prices[0, 2] = 110; // high
            prices[0, 3] = 90; // low
            prices[0, 4] = 105; // close
            prices[0, 5] = 1000; // volume

            // Second entry - very high values
            prices[1, 0] = 1000001; // timestamp
            prices[1, 1] = 10000; // open
            prices[1, 2] = 11000; // high
            prices[1, 3] = 9000; // low
            prices[1, 4] = 10500; // close
            prices[1, 5] = 100000; // volume

            // Third entry - very low values
            prices[2, 0] = 1000002; // timestamp
            prices[2, 1] = 0.001; // open
            prices[2, 2] = 0.0011; // high
            prices[2, 3] = 0.0009; // low
            prices[2, 4] = 0.00105; // close
            prices[2, 5] = 1000; // volume

            // Fourth entry - mix of positive and negative values
            prices[3, 0] = 1000003; // timestamp
            prices[3, 1] = -100; // open (negative value)
            prices[3, 2] = 100; // high
            prices[3, 3] = -200; // low (negative value)
            prices[3, 4] = -50; // close (negative value)
            prices[3, 5] = 1000; // volume

            // Fifth entry - extreme volume
            prices[4, 0] = 1000004; // timestamp
            prices[4, 1] = 100; // open
            prices[4, 2] = 110; // high
            prices[4, 3] = 90; // low
            prices[4, 4] = 105; // close
            prices[4, 5] = 1000000000; // extreme volume

            // Act & Assert
            // Just make sure it doesn't throw exceptions
            var result = calculator.Calculate(prices, true);
            Assert.NotNull(result);
            Assert.Equal(5, result.Results.Count);
        }
    }
}
