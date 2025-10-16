using HC.TechnicalCalculators.Src.Calculators.Overlap;
using HC.TechnicalCalculators.Src.Models;
using HC.TechnicalCalculators.Tests.Helpers;

namespace HC.TechnicalCalculators.Tests.Calculators.Overlap
{
    public class ParabolicSarCalculatorTests
    {
        [Fact]
        public void Constructor_ShouldThrowArgumentException_WhenAccelerationParameterIsMissing()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Maximum), "0.2" } };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new ParabolicSarCalculator(parameters, nameof(CalculatorNameEnum.PSAR)));
            Assert.Equal("Parameters Acceleration and Maximum are required.", exception.Message);
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentException_WhenMaximumParameterIsMissing()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.Acceleration), "0.02" } };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new ParabolicSarCalculator(parameters, nameof(CalculatorNameEnum.PSAR)));
            Assert.Equal("Parameters Acceleration and Maximum are required.", exception.Message);
        }

        [Fact]
        public void Constructor_ShouldCreateInstance_WhenAllRequiredParametersAreProvided()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.Acceleration), "0.02" },
                { nameof(ParameterNamesEnum.Maximum), "0.2" }
            };

            // Act
            var calculator = new ParabolicSarCalculator(parameters, nameof(CalculatorNameEnum.PSAR));

            // Assert
            Assert.NotNull(calculator);
        }

        [Fact]
        public void Calculate_ShouldThrowFormatException_WhenAccelerationIsNotANumber()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.Acceleration), "invalid" },
                { nameof(ParameterNamesEnum.Maximum), "0.2" }
            };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new ParabolicSarCalculator(parameters, nameof(CalculatorNameEnum.PSAR)));
            Assert.Equal("Acceleration must be between 0.001 and 0.5.", exception.Message);
        }

        [Fact]
        public void Calculate_ShouldThrowFormatException_WhenMaximumIsNotANumber()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.Acceleration), "0.02" },
                { nameof(ParameterNamesEnum.Maximum), "invalid" }
            };

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => new ParabolicSarCalculator(parameters, nameof(CalculatorNameEnum.PSAR)));
            Assert.Equal("Maximum must be between 0.01 and 1.", exception.Message);
        }

        [Fact]
        public void Calculate_ShouldThrowArgumentException_WhenPricesArrayIsTooShort()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.Acceleration), "0.02" },
                { nameof(ParameterNamesEnum.Maximum), "0.2" }
            };

            var calculator = new ParabolicSarCalculator(parameters, nameof(CalculatorNameEnum.PSAR));
            double[,] prices = Pricedata.GetPrices(1); // Only 1 price point, need at least 2

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => calculator.Calculate(prices, true));
            Assert.Equal("Price array is too short for the calculation.", exception.Message);
        }

        [Fact]
        public void Calculate_ShouldReturnCorrectResults_WhenParametersAreValid()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.Acceleration), "0.02" },
                { nameof(ParameterNamesEnum.Maximum), "0.2" }
            };

            var calculator = new ParabolicSarCalculator(parameters, nameof(CalculatorNameEnum.PSAR));
            double[,] prices = Pricedata.GetPrices(20); // Use enough data points for meaningful SAR calculation

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(nameof(CalculatorNameEnum.PSAR), result.Name);
            Assert.True(result.Results.Count > 0);

            // Check the structure of results
            foreach (var timestamp in result.Results.Keys)
            {
                var values = result.Results[timestamp];
                Assert.Equal(2, values.Length);
                Assert.Contains(values, kvp => kvp.Key == nameof(TechnicalNamesEnum.SAR));
                Assert.Contains(values, kvp => kvp.Key == nameof(TechnicalNamesEnum.ISREVERSAL));

                // ISREVERSAL should be either 0 or 1
                var isReversal = values.First(kvp => kvp.Key == nameof(TechnicalNamesEnum.ISREVERSAL)).Value;
                Assert.True(isReversal == 0.0 || isReversal == 1.0);
            }
        }

        [Fact]
        public void Calculate_ShouldReturnOrderedResults()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.Acceleration), "0.02" },
                { nameof(ParameterNamesEnum.Maximum), "0.2" }
            };

            var calculator = new ParabolicSarCalculator(parameters, nameof(CalculatorNameEnum.PSAR));
            double[,] prices = Pricedata.GetPrices(20);

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert
            var timestamps = result.Results.Keys.ToList();
            var orderedTimestamps = timestamps.OrderByDescending(t => t).ToList();
            Assert.Equal(orderedTimestamps, timestamps);
        }

        [Fact]
        public void Calculate_ShouldThrowArgumentNullException_WhenPricesIsNull()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.Acceleration), "0.02" },
                { nameof(ParameterNamesEnum.Maximum), "0.2" }
            };

            var calculator = new ParabolicSarCalculator(parameters, nameof(CalculatorNameEnum.PSAR));
            double[,] prices = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => calculator.Calculate(prices));
            Assert.Contains("prices", exception.Message);
        }

        [Fact]
        public void Calculate_DifferentParameters_ShouldProduceDifferentResults()
        {
            // Arrange
            var parameters1 = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.Acceleration), "0.02" },
                { nameof(ParameterNamesEnum.Maximum), "0.2" }
            };

            var parameters2 = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.Acceleration), "0.05" },
                { nameof(ParameterNamesEnum.Maximum), "0.2" }
            };

            var calculator1 = new ParabolicSarCalculator(parameters1, nameof(CalculatorNameEnum.PSAR));
            var calculator2 = new ParabolicSarCalculator(parameters2, nameof(CalculatorNameEnum.PSAR));

            double[,] prices = Pricedata.GetPrices(30); // Need enough data points

            // Act
            var result1 = calculator1.Calculate(prices, true);
            var result2 = calculator2.Calculate(prices, true);

            // Assert
            // Find a common timestamp that exists in both results
            var commonTimestamps = result1.Results.Keys.Intersect(result2.Results.Keys);
            Assert.NotEmpty(commonTimestamps);

            bool foundDifference = false;
            foreach (var timestamp in commonTimestamps)
            {
                var sar1 = result1.Results[timestamp].First(kvp => kvp.Key == nameof(TechnicalNamesEnum.SAR)).Value;
                var sar2 = result2.Results[timestamp].First(kvp => kvp.Key == nameof(TechnicalNamesEnum.SAR)).Value;

                if (sar1 != sar2)
                {
                    foundDifference = true;
                    break;
                }
            }

            // Different acceleration should produce at least some different SAR values
            Assert.True(foundDifference);
        }

        [Fact]
        public void Calculate_ShouldHaveReversals()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.Acceleration), "0.02" },
                { nameof(ParameterNamesEnum.Maximum), "0.2" }
            };

            var calculator = new ParabolicSarCalculator(parameters, nameof(CalculatorNameEnum.PSAR));
            // Use a longer price series to increase chance of seeing reversals
            double[,] prices = Pricedata.GetPrices(50);

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert
            // Count how many reversals are detected
            int reversalCount = 0;
            foreach (var timestamp in result.Results.Keys)
            {
                var isReversal = result.Results[timestamp].First(kvp => kvp.Key == nameof(TechnicalNamesEnum.ISREVERSAL)).Value;
                if (isReversal == 1.0)
                {
                    reversalCount++;
                }
            }

            // In a sufficiently long price series, there should be at least one trend reversal
            // This test might be flaky depending on the test data, but with realistic price data
            // there should be some reversals
            Assert.True(reversalCount > 0, "No trend reversals detected in the SAR calculation");
        }

        [Fact]
        public void Calculate_WithLargeDataSet_ShouldCompleteSuccessfully()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.Acceleration), "0.02" },
                { nameof(ParameterNamesEnum.Maximum), "0.2" }
            };

            var calculator = new ParabolicSarCalculator(parameters, nameof(CalculatorNameEnum.PSAR));
            double[,] prices = Pricedata.GetPrices(100); // Large dataset

            // Act
            var result = calculator.Calculate(prices);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Results.Count > 0);
        }

        [Fact]
        public void Calculate_ShouldNotModifyInputPrices()
        {
            // Arrange
            var parameters = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.Acceleration), "0.02" },
                { nameof(ParameterNamesEnum.Maximum), "0.2" }
            };

            var calculator = new ParabolicSarCalculator(parameters, nameof(CalculatorNameEnum.PSAR));
            double[,] prices = Pricedata.GetPrices(20);

            // Make a copy of the prices array for comparison
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
        public void Calculate_WithHigherAcceleration_ShouldAdaptFaster()
        {
            // Arrange - Higher acceleration should make SAR adapt more quickly to price changes
            var parameters1 = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.Acceleration), "0.02" }, // Lower acceleration
                { nameof(ParameterNamesEnum.Maximum), "0.2" }
            };

            var parameters2 = new Dictionary<string, string>
            {
                { nameof(ParameterNamesEnum.Acceleration), "0.1" }, // Higher acceleration
                { nameof(ParameterNamesEnum.Maximum), "0.2" }
            };

            var calculator1 = new ParabolicSarCalculator(parameters1, nameof(CalculatorNameEnum.PSAR));
            var calculator2 = new ParabolicSarCalculator(parameters2, nameof(CalculatorNameEnum.PSAR));

            // Create a price series with a sharp reversal
            double[,] prices = Pricedata.GetPrices(30);

            // Act
            var result1 = calculator1.Calculate(prices, true);
            var result2 = calculator2.Calculate(prices, true);

            // Assert
            // Higher acceleration should generally result in more reversals detected
            int reversalCount1 = result1.Results.Values.Count(v => v.First(kvp => kvp.Key == nameof(TechnicalNamesEnum.ISREVERSAL)).Value == 1.0);
            int reversalCount2 = result2.Results.Values.Count(v => v.First(kvp => kvp.Key == nameof(TechnicalNamesEnum.ISREVERSAL)).Value == 1.0);

            // This is a probabilistic test; higher acceleration should usually result in more reversals,
            // but it's not guaranteed. If this test is flaky, consider removing it.
            Assert.True(reversalCount2 >= reversalCount1,
                $"Higher acceleration ({parameters2[nameof(ParameterNamesEnum.Acceleration)]}) should result in at least as many reversals as lower acceleration ({parameters1[nameof(ParameterNamesEnum.Acceleration)]}), " +
                $"but got {reversalCount2} vs {reversalCount1}");
        }
    }
}
