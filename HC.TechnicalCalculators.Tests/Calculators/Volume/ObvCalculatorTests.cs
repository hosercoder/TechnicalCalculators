using HC.TechnicalCalculators.Src.Calculators.Volume;
using HC.TechnicalCalculators.Src.Models;
using HC.TechnicalCalculators.Tests.Helpers;

namespace HC.TechnicalCalculators.Tests.Calculators.Volume
{
    public class ObvCalculatorTests
    {
        [Fact]
        public void Constructor_ShouldCreateInstance_WithParameters()
        {
            // Arrange
            var parameters = new Dictionary<string, string>();

            // Act
            var calculator = new ObvCalculator(parameters, nameof(CalculatorNameEnum.OBV));

            // Assert
            Assert.NotNull(calculator);
        }

        [Fact]
        public void Calculate_ShouldThrowArgumentNullException_WhenPricesIsNull()
        {
            // Arrange
            var parameters = new Dictionary<string, string>();
            var calculator = new ObvCalculator(parameters, nameof(CalculatorNameEnum.OBV));
            double[,] prices = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => calculator.Calculate(prices));
            Assert.Contains("prices", exception.ParamName);
        }

        [Fact]
        public void Calculate_ShouldThrowArgumentException_WhenPricesDoesNotHaveCorrectColumns()
        {
            // Arrange
            var parameters = new Dictionary<string, string>();
            var calculator = new ObvCalculator(parameters, nameof(CalculatorNameEnum.OBV));
            double[,] prices = new double[3, 4]; // Invalid number of columns

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => calculator.Calculate(prices));
            Assert.Contains("columns", exception.Message);
        }

        [Fact]
        public void Calculate_ShouldReturnCorrectResults_WhenPricesAreValid()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.NA), "" } };
            var calculator = new ObvCalculator(parameters, nameof(CalculatorNameEnum.OBV));
            double[,] prices = Pricedata.GetPrices(5); // Using 5 data points

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
            Assert.Equal(nameof(CalculatorNameEnum.OBV), result.Name);
            Assert.Equal(5, result.Results.Count);

            // Verify each timestamp has results
            foreach (var timestamp in timestamps)
            {
                Assert.Contains(timestamp, result.Results.Keys);
                Assert.Single(result.Results[timestamp]);
                Assert.Equal(nameof(TechnicalNamesEnum.OBV), result.Results[timestamp][0].Key);
            }
        }

        [Fact]
        public void Calculate_ShouldHandleEmptyPricesArray()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.NA), "" } };
            var calculator = new ObvCalculator(parameters, nameof(CalculatorNameEnum.OBV));
            double[,] prices = new double[0, 6];

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => calculator.Calculate(prices, true));
            Assert.Contains("TALib OBV calculation failed with return code:", exception.Message);
        }

        [Fact]
        public void Calculate_ShouldCalculateCorrectOBVValues()
        {
            // Arrange
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.NA), "" } };
            var calculator = new ObvCalculator(parameters, nameof(CalculatorNameEnum.OBV));

            // Create a custom price array with known values
            double[,] prices = new double[3, 6];
            // timestamp, open, high, low, close, volume
            prices[0, 0] = 1625097600; // 2021-07-01
            prices[0, 1] = 100; // open
            prices[0, 2] = 105; // high
            prices[0, 3] = 95;  // low
            prices[0, 4] = 102; // close
            prices[0, 5] = 1000; // volume

            prices[1, 0] = 1625184000; // 2021-07-02
            prices[1, 1] = 102; // open
            prices[1, 2] = 110; // high
            prices[1, 3] = 100; // low
            prices[1, 4] = 108; // close (higher than previous close)
            prices[1, 5] = 1500; // volume

            prices[2, 0] = 1625270400; // 2021-07-03
            prices[2, 1] = 108; // open
            prices[2, 2] = 112; // high
            prices[2, 3] = 98;  // low
            prices[2, 4] = 100; // close (lower than previous close)
            prices[2, 5] = 2000; // volume

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert
            Assert.NotNull(result);

            // First OBV value should be the first volume
            Assert.Equal(1000, result.Results[(long)prices[0, 0]][0].Value, 6);

            // Second OBV should be first + second volume (because close price increased)
            Assert.Equal(2500, result.Results[(long)prices[1, 0]][0].Value, 6);

            // Third OBV should be second - third volume (because close price decreased)
            Assert.Equal(500, result.Results[(long)prices[2, 0]][0].Value, 6);
        }

        [Fact]
        public void CalculateOBV_ShouldHandleEqualClosePrices()
        {
            var parameters = new Dictionary<string, string> { { nameof(ParameterNamesEnum.NA), "" } };

            var calculator = new ObvCalculator(parameters, nameof(CalculatorNameEnum.OBV));

            // Create a custom price array with equal close prices
            double[,] prices = new double[3, 6];
            // timestamp, open, high, low, close, volume
            prices[0, 0] = 1625097600; // 2021-07-01
            prices[0, 1] = 100; // open
            prices[0, 2] = 105; // high
            prices[0, 3] = 95;  // low
            prices[0, 4] = 100; // close
            prices[0, 5] = 1000; // volume

            prices[1, 0] = 1625184000; // 2021-07-02
            prices[1, 1] = 100; // open
            prices[1, 2] = 110; // high
            prices[1, 3] = 90;  // low
            prices[1, 4] = 100; // close (same as previous close)
            prices[1, 5] = 1500; // volume

            prices[2, 0] = 1625270400; // 2021-07-03
            prices[2, 1] = 100; // open
            prices[2, 2] = 112; // high
            prices[2, 3] = 98;  // low
            prices[2, 4] = 105; // close (higher than previous close)
            prices[2, 5] = 2000; // volume

            // Act
            var result = calculator.Calculate(prices, true);

            // Assert
            Assert.NotNull(result);

            // First OBV value should be the first volume
            Assert.Equal(1000, result.Results[(long)prices[0, 0]][0].Value, 6);

            // Second OBV should remain same as first (because close prices are equal)
            Assert.Equal(1000, result.Results[(long)prices[1, 0]][0].Value, 6);

            // Third OBV should be second + third volume (because close price increased)
            Assert.Equal(3000, result.Results[(long)prices[2, 0]][0].Value, 6);
        }
    }
}
