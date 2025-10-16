using HC.TechnicalCalculators.Src.Calculators.Price;
using HC.TechnicalCalculators.Src.Models;
using HC.TechnicalCalculators.Tests.Helpers;

namespace HC.TechnicalCalculators.Tests.Calculators.Price
{
    public class WclPriceCalculatorTests
    {

        [Fact]
        public void Constructor_ShouldCreateInstance()
        {
            // Arrange
            var parameters = new Dictionary<string, string>() { { nameof(ParameterNamesEnum.NA), "" } };

            // Act
            var calculator = new WclPriceCalculator(parameters, nameof(CalculatorNameEnum.WCLPRICE));

            // Assert
            Assert.NotNull(calculator);
        }

        [Fact]
        public void Calculate_ShouldThrowArgumentNullException_WhenPricesIsNull()
        {
            var parameters = new Dictionary<string, string>() { { nameof(ParameterNamesEnum.NA), "" } };
            // Arrange
            var calculator = new WclPriceCalculator(parameters, nameof(CalculatorNameEnum.WCLPRICE));

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => calculator.Calculate(null));
            Assert.Contains("prices", exception.ParamName);
        }

        [Fact]
        public void Calculate_ShouldThrowArgumentException_WhenPricesHasInvalidColumns()
        {
            var parameters = new Dictionary<string, string>() { { nameof(ParameterNamesEnum.NA), "" } };
            // Arrange
            var calculator = new WclPriceCalculator(parameters, nameof(CalculatorNameEnum.WCLPRICE));
            double[,] invalidPrices = new double[5, 3]; // Only 3 columns instead of required 6

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => calculator.Calculate(invalidPrices));
            Assert.Contains("columns", exception.Message);
        }

        [Fact]
        public void Calculate_ShouldReturnValidResults()
        {
            var parameters = new Dictionary<string, string>() { { nameof(ParameterNamesEnum.NA), "" } };
            // Arrange
            var calculator = new WclPriceCalculator(parameters, nameof(CalculatorNameEnum.WCLPRICE));
            var prices = Pricedata.GetPrices(10); // Using helper to get test prices

            // Act
            var results = calculator.Calculate(prices, true);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(nameof(CalculatorNameEnum.WCLPRICE), results.Name);
            Assert.NotEmpty(results.Results);

            // Verify structure and values of results
            foreach (var result in results.Results)
            {
                Assert.Single(result.Value); // Each timestamp should have one value
                Assert.Equal(nameof(TechnicalNamesEnum.WCLPRICE), result.Value[0].Key);

                // Get original price data for this timestamp to verify calculation
                int rowIndex = -1;
                for (int i = 0; i < prices.GetLength(0); i++)
                {
                    if ((long)prices[i, 0] == result.Key)
                    {
                        rowIndex = i;
                        break;
                    }
                }

                Assert.True(rowIndex >= 0, "Timestamp in results should match an input timestamp");

                // WCLPRICE formula = (High + Low + Close*2) / 4
                double expectedWclPrice = (prices[rowIndex, 2] + prices[rowIndex, 3] +
                                          (prices[rowIndex, 4] * 2)) / 4.0;

                Assert.Equal(expectedWclPrice, result.Value[0].Value, 6); // 6 decimal precision
            }
        }

        [Fact]
        public void Calculate_ShouldHandleEmptyPricesArray()
        {
            var parameters = new Dictionary<string, string>() { { nameof(ParameterNamesEnum.NA), "" } };
            // Arrange
            var calculator = new WclPriceCalculator(parameters, nameof(CalculatorNameEnum.WCLPRICE));
            double[,] emptyPrices = new double[0, 6];

            // Act
            var results = calculator.Calculate(emptyPrices, true);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(nameof(CalculatorNameEnum.WCLPRICE), results.Name);
            Assert.Empty(results.Results);
        }

        [Fact]
        public void Calculate_ShouldProduceConsistentResults_WhenCalledMultipleTimes()
        {
            var parameters = new Dictionary<string, string>() { { nameof(ParameterNamesEnum.NA), "" } };
            // Arrange
            var calculator = new WclPriceCalculator(parameters, nameof(CalculatorNameEnum.WCLPRICE));
            var prices = Pricedata.GetPrices(10);

            // Act
            var results1 = calculator.Calculate(prices, true);
            var results2 = calculator.Calculate(prices, true);

            // Assert
            Assert.Equal(results1.Results.Count, results2.Results.Count);

            foreach (var key in results1.Results.Keys)
            {
                Assert.Contains(key, results2.Results.Keys);
                Assert.Equal(results1.Results[key][0].Value, results2.Results[key][0].Value);
            }
        }

        [Fact]
        public void Calculate_ShouldHandleExtremePriceValues()
        {
            var parameters = new Dictionary<string, string>() { { nameof(ParameterNamesEnum.NA), "" } };
            // Arrange
            var calculator = new WclPriceCalculator(parameters, nameof(CalculatorNameEnum.WCLPRICE));
            double[,] extremePrices = new double[2, 6]
            {
                { 1625097600, 10000, 10000, 10000, 10000, 1000 }, // Very high prices
                { 1625184000, 0.00001, 0.00002, 0.00001, 0.00002, 1000 }  // Very low prices
            };

            // Act
            var results = calculator.Calculate(extremePrices, true);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(2, results.Results.Count);

            // High price case
            Assert.Equal(10000, results.Results[1625097600][0].Value);

            // Low price case - WCLPRICE = (0.00002 + 0.00001 + 0.00002*2) / 4 = 0.0000175
            Assert.Equal(0.0000175, results.Results[1625184000][0].Value, 8);
        }

        [Fact]
        public void Calculate_ShouldWeightClosePriceMoreHeavily()
        {
            var parameters = new Dictionary<string, string>() { { nameof(ParameterNamesEnum.NA), "" } };
            // Arrange
            var calculator = new WclPriceCalculator(parameters, nameof(CalculatorNameEnum.WCLPRICE));
            double[,] testPrices = new double[1, 6]
            {
                { 1625097600, 100, 200, 300, 400, 1000 } // High, Low, Close values are all different
            };

            // Act
            var results = calculator.Calculate(testPrices, true);

            // Assert
            Assert.NotNull(results);
            Assert.Single(results.Results);

            // WCLPRICE = (200 + 300 + 400*2) / 4 = 325
            Assert.Equal(325, results.Results[1625097600][0].Value);

            // Check that it's closer to close price (400) than simple average
            double simpleAverage = (200 + 300 + 400) / 3.0; // = 300
            double wclPrice = results.Results[1625097600][0].Value;

            Assert.True(Math.Abs(wclPrice - 400) < Math.Abs(simpleAverage - 400),
                "WCL price should be weighted more toward close price than simple average");
        }

        [Fact]
        public void Calculate_ShouldNotUseOpenPrice()
        {
            var parameters = new Dictionary<string, string>() { { nameof(ParameterNamesEnum.NA), "" } };
            // Arrange - Create two price sets identical except for Open price
            var calculator = new WclPriceCalculator(parameters, nameof(CalculatorNameEnum.WCLPRICE));

            double[,] prices1 = new double[1, 6]
            {
                { 1625097600, 100, 200, 300, 400, 1000 } // Open = 100
            };

            double[,] prices2 = new double[1, 6]
            {
                { 1625097600, 999, 200, 300, 400, 1000 } // Open = 999 (very different)
            };

            // Act
            var results1 = calculator.Calculate(prices1, true);
            var results2 = calculator.Calculate(prices2, true);

            // Assert - Results should be identical despite different Open prices
            Assert.Equal(
                results1.Results[1625097600][0].Value,
                results2.Results[1625097600][0].Value
            );
        }

        [Fact]
        public void Calculate_AllEqualPrices_ShouldReturnSameValue()
        {
            var parameters = new Dictionary<string, string>() { { nameof(ParameterNamesEnum.NA), "" } };
            // Arrange
            var calculator = new WclPriceCalculator(parameters, nameof(CalculatorNameEnum.WCLPRICE));
            double constantPrice = 100.0;
            double[,] constantPrices = new double[1, 6]
            {
                { 1625097600, constantPrice, constantPrice, constantPrice, constantPrice, 1000 }
            };

            // Act
            var results = calculator.Calculate(constantPrices, true);

            // Assert
            Assert.Equal(constantPrice, results.Results[1625097600][0].Value);
        }
    }
}
