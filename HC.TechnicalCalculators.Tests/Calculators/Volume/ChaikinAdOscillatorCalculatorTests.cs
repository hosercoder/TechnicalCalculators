using HC.TechnicalCalculators.Src.Calculators.Volume;
using HC.TechnicalCalculators.Src.Models;
using HC.TechnicalCalculators.Tests.Helpers;

namespace HC.TechnicalCalculators.Tests.Calculators.Volume
{
    public class ChaikinAdOscillatorCalculatorTests
    {
        [Fact]
        public void Constructor_ShouldCreateInstance()
        {
            // Arrange
            var parameters = new Dictionary<string, string>();

            // Act
            var calculator = new ChaikinAdOscillatorCalculator(parameters, nameof(CalculatorNameEnum.CHAIKINADOSCILLATOR));

            // Assert
            Assert.NotNull(calculator);
        }

        [Fact]
        public void Calculate_ShouldThrowArgumentNullException_WhenPricesIsNull()
        {
            // Arrange
            var calculator = new ChaikinAdOscillatorCalculator(new Dictionary<string, string>(), nameof(CalculatorNameEnum.CHAIKINADOSCILLATOR));

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => calculator.Calculate(null));
            Assert.Contains("prices", exception.ParamName);
        }

        [Fact]
        public void Calculate_ShouldThrowArgumentException_WhenPricesHasInvalidColumns()
        {
            // Arrange
            var calculator = new ChaikinAdOscillatorCalculator(new Dictionary<string, string>(), nameof(CalculatorNameEnum.CHAIKINADOSCILLATOR));
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
            var calculator = new ChaikinAdOscillatorCalculator(parameters, nameof(CalculatorNameEnum.CHAIKINADOSCILLATOR));
            var prices = Pricedata.GetPrices(20); // Need more data for meaningful EMA calculations

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
            Assert.Equal(nameof(CalculatorNameEnum.CHAIKINADOSCILLATOR), result.Name);
            Assert.Equal(prices.GetLength(0), result.Results.Count);

            foreach (var timestamp in timestamps)
            {
                Assert.Contains(timestamp, result.Results.Keys);
            }

            // Check structure of results
            foreach (var entry in result.Results)
            {
                Assert.Single(entry.Value);
                Assert.Equal(nameof(TechnicalNamesEnum.CHAIKINADOSCILLATOR), entry.Value[0].Key);
                Assert.IsType<double>(entry.Value[0].Value);
            }
        }

        [Fact]
        public void Calculate_ShouldHandleEmptyPricesArray()
        {
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.NA), "" } };
            // Arrange
            var calculator = new ChaikinAdOscillatorCalculator(parameters, nameof(CalculatorNameEnum.CHAIKINADOSCILLATOR));
            double[,] emptyPrices = new double[0, 6];

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => calculator.Calculate(emptyPrices, true));
            Assert.Contains("TALib CHAIKINADLINE calculation failed with return code:", exception.Message);

        }

        [Fact]
        public void Calculate_ShouldProduceConsistentResults_WhenCalledMultipleTimes()
        {
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.NA), "" } };
            // Arrange
            var calculator = new ChaikinAdOscillatorCalculator(parameters, nameof(CalculatorNameEnum.CHAIKINADOSCILLATOR));
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
            var calculator = new ChaikinAdOscillatorCalculator(parameters, nameof(CalculatorNameEnum.CHAIKINADOSCILLATOR));
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
        public void Calculate_OscillatorShouldRespondToMarketChanges()
        {
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.NA), "" } };
            // Arrange
            var calculator = new ChaikinAdOscillatorCalculator(parameters, nameof(CalculatorNameEnum.CHAIKINADOSCILLATOR));

            // Create price data with clear trend changes
            double[,] prices = new double[30, 6];

            // First 10 periods - buying pressure (closes near high)
            for (int i = 0; i < 10; i++)
            {
                prices[i, 0] = 1000000 + i; // timestamp
                prices[i, 1] = 100; // open
                prices[i, 2] = 110; // high
                prices[i, 3] = 90; // low
                prices[i, 4] = 108; // close (near high - buying pressure)
                prices[i, 5] = 1000; // volume
            }

            // Next 10 periods - selling pressure (closes near low)
            for (int i = 10; i < 20; i++)
            {
                prices[i, 0] = 1000000 + i; // timestamp
                prices[i, 1] = 100; // open
                prices[i, 2] = 110; // high
                prices[i, 3] = 90; // low
                prices[i, 4] = 92; // close (near low - selling pressure)
                prices[i, 5] = 1000; // volume
            }

            // Last 10 periods - buying pressure again
            for (int i = 20; i < 30; i++)
            {
                prices[i, 0] = 1000000 + i; // timestamp
                prices[i, 1] = 100; // open
                prices[i, 2] = 110; // high
                prices[i, 3] = 90; // low
                prices[i, 4] = 108; // close (near high - buying pressure)
                prices[i, 5] = 1000; // volume
            }

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert
            // Extract values from the middle of each segment to observe trends
            double buyingPhase1 = result.Results[1000005][0].Value;
            double sellingPhase = result.Results[1000015][0].Value;
            double buyingPhase2 = result.Results[1000025][0].Value;

            // Oscillator should be more positive during buying pressure
            // and more negative during selling pressure
            Assert.True(buyingPhase1 > sellingPhase,
                $"Oscillator should be higher during buying pressure ({buyingPhase1}) than selling pressure ({sellingPhase})");

            Assert.True(buyingPhase2 > sellingPhase,
                $"Oscillator should be higher during buying pressure ({buyingPhase2}) than selling pressure ({sellingPhase})");
        }

        [Fact]
        public void Calculate_ShouldCrossZeroLine_WithChangingMarketConditions()
        {
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.NA), "" } };
            // Arrange
            var calculator = new ChaikinAdOscillatorCalculator(parameters, nameof(CalculatorNameEnum.CHAIKINADOSCILLATOR));

            // Create price data with strong trend reversals
            double[,] prices = new double[40, 6];

            // First 20 periods - strong buying pressure
            for (int i = 0; i < 20; i++)
            {
                prices[i, 0] = 1000000 + i; // timestamp
                prices[i, 1] = 100 + i; // open (uptrend)
                prices[i, 2] = 110 + i; // high
                prices[i, 3] = 95 + i; // low
                prices[i, 4] = 108 + i; // close (near high - buying pressure)
                prices[i, 5] = 1000 + (i * 100); // increasing volume
            }

            // Next 20 periods - strong selling pressure
            for (int i = 20; i < 40; i++)
            {
                prices[i, 0] = 1000000 + i; // timestamp
                prices[i, 1] = 120 - (i - 20); // open (downtrend)
                prices[i, 2] = 125 - (i - 20); // high
                prices[i, 3] = 115 - (i - 20); // low
                prices[i, 4] = 116 - (i - 20); // close (near low - selling pressure)
                prices[i, 5] = 3000 - ((i - 20) * 50); // decreasing volume
            }

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert
            // Find if the oscillator crosses the zero line during trend reversal
            bool foundPositiveValue = false;
            bool foundNegativeValue = false;
            bool crossedZero = false;

            foreach (var timestamp in result.Results.Keys.OrderBy(k => k))
            {
                double value = result.Results[timestamp][0].Value;

                if (value > 0)
                    foundPositiveValue = true;
                else if (value < 0)
                    foundNegativeValue = true;

                // If we've seen both positive and negative values, a zero crossing occurred
                if (foundPositiveValue && foundNegativeValue)
                {
                    crossedZero = true;
                    break;
                }
            }

            Assert.True(crossedZero, "Oscillator should cross the zero line during significant trend reversal");
        }

        [Fact(Skip = "Work in progress")]
        public void Calculate_ShouldBeAbleToDetectDivergence()
        {
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.NA), "" } };
            // Arrange
            var calculator = new ChaikinAdOscillatorCalculator(parameters, nameof(CalculatorNameEnum.CHAIKINADOSCILLATOR));

            // Create price data with price-oscillator divergence
            double[,] prices = new double[30, 6];

            // First 10 periods - prices and volume rising together
            for (int i = 0; i < 10; i++)
            {
                prices[i, 0] = 1000000 + i; // timestamp
                prices[i, 1] = 100 + i; // open (uptrend)
                prices[i, 2] = 110 + i; // high
                prices[i, 3] = 95 + i; // low
                prices[i, 4] = 108 + i; // close 
                prices[i, 5] = 1000 + (i * 100); // increasing volume
            }

            // Next 10 periods - prices rising but volume declining (bearish divergence)
            for (int i = 10; i < 20; i++)
            {
                prices[i, 0] = 1000000 + i; // timestamp
                prices[i, 1] = 110 + (i - 10); // open (still uptrend)
                prices[i, 2] = 120 + (i - 10); // high
                prices[i, 3] = 105 + (i - 10); // low
                prices[i, 4] = 118 + (i - 10); // close
                prices[i, 5] = 2000 - ((i - 10) * 100); // decreasing volume
            }

            // Last 10 periods - both prices and volume declining
            for (int i = 20; i < 30; i++)
            {
                prices[i, 0] = 1000000 + i; // timestamp
                prices[i, 1] = 120 - (i - 20); // open (downtrend)
                prices[i, 2] = 125 - (i - 20); // high
                prices[i, 3] = 115 - (i - 20); // low
                prices[i, 4] = 116 - (i - 20); // close
                prices[i, 5] = 1000 - ((i - 20) * 50); // decreasing volume
            }

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert
            // Check that oscillator peaks before price does (classic divergence pattern)
            double oscMidPhase1 = result.Results[1000005][0].Value;
            double oscMidPhase2 = result.Results[1000015][0].Value;
            double oscMidPhase3 = result.Results[1000025][0].Value;

            // In a divergence, the oscillator should start declining before price
            // Second phase has higher prices but the oscillator should be weakening
            Assert.True(oscMidPhase1 > oscMidPhase2,
                $"Oscillator should weaken in phase 2 ({oscMidPhase2}) compared to phase 1 ({oscMidPhase1})");

            // By phase 3, both price and oscillator should be declining
            Assert.True(oscMidPhase3 < oscMidPhase2,
                $"Oscillator should continue to decline in phase 3 ({oscMidPhase3}) compared to phase 2 ({oscMidPhase2})");
        }

        [Fact]
        public void Calculate_ShouldHandleExtremeValues()
        {
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.NA), "" } };
            // Arrange
            var calculator = new ChaikinAdOscillatorCalculator(parameters, nameof(CalculatorNameEnum.CHAIKINADOSCILLATOR));

            // Create price data with extreme values
            double[,] prices = new double[15, 6];

            // First 5 entries - normal values
            for (int i = 0; i < 5; i++)
            {
                prices[i, 0] = 1000000 + i; // timestamp
                prices[i, 1] = 100; // open
                prices[i, 2] = 110; // high
                prices[i, 3] = 90; // low
                prices[i, 4] = 105; // close
                prices[i, 5] = 1000; // volume
            }

            // Next 5 entries - very high values
            for (int i = 5; i < 10; i++)
            {
                prices[i, 0] = 1000000 + i; // timestamp
                prices[i, 1] = 10000; // open
                prices[i, 2] = 11000; // high
                prices[i, 3] = 9000; // low
                prices[i, 4] = 10500; // close
                prices[i, 5] = 100000; // volume
            }

            // Last 5 entries - very low values
            for (int i = 10; i < 15; i++)
            {
                prices[i, 0] = 1000000 + i; // timestamp
                prices[i, 1] = 0.001; // open
                prices[i, 2] = 0.0011; // high
                prices[i, 3] = 0.0009; // low
                prices[i, 4] = 0.00105; // close
                prices[i, 5] = 1000; // volume
            }

            // Act & Assert
            // Just make sure it doesn't throw exceptions
            var result = calculator.Calculate(prices, true);
            Assert.NotNull(result);
            Assert.Equal(15, result.Results.Count);
        }

        [Fact]
        public void Calculate_ShouldHandleZeroVolume()
        {
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.NA), "" } };
            // Arrange
            var calculator = new ChaikinAdOscillatorCalculator(parameters, nameof(CalculatorNameEnum.CHAIKINADOSCILLATOR));

            // Create price data with some zero volume entries
            double[,] prices = new double[15, 6];

            for (int i = 0; i < 15; i++)
            {
                prices[i, 0] = 1000000 + i; // timestamp
                prices[i, 1] = 100; // open
                prices[i, 2] = 110; // high
                prices[i, 3] = 90; // low
                prices[i, 4] = 105; // close
                prices[i, 5] = (i % 3 == 0) ? 0 : 1000; // alternate zero volume days
            }

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert - Just verify it doesn't throw and returns all timestamps
            Assert.NotNull(result);
            Assert.Equal(15, result.Results.Count);
        }
    }
}
