using HC.TechnicalCalculators.Src.Calculators.Price;
using HC.TechnicalCalculators.Src.Models;
using HC.TechnicalCalculators.Tests.Helpers;

namespace HC.TechnicalCalculators.Tests.Calculators.Price
{
    public class AvgPriceCalculatorTests
    {

        [Fact]
        public void Constructor_ShouldCreateInstance()
        {
            // Arrange
            var parameters = new Dictionary<string, string>() { { nameof(ParameterNamesEnum.NA), "" } };

            // Act
            var calculator = new AvgPriceCalculator(parameters, nameof(CalculatorNameEnum.AVGPRICE));

            // Assert
            Assert.NotNull(calculator);
        }

        [Fact]
        public void Calculate_ShouldReturnValidResults()
        {
            var parameters = new Dictionary<string, string>() { { nameof(ParameterNamesEnum.NA), "" } };
            // Arrange
            var calculator = new AvgPriceCalculator(parameters, nameof(CalculatorNameEnum.AVGPRICE));
            var prices = Pricedata.GetPrices(10); // Using helper to get test prices

            // Act
            var results = calculator.Calculate(prices, true);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(nameof(CalculatorNameEnum.AVGPRICE), results.Name);
            Assert.NotEmpty(results.Results);

            // Verify structure and values of results
            foreach (var result in results.Results)
            {
                Assert.Single(result.Value); // Each timestamp should have one value
                Assert.Equal(nameof(TechnicalNamesEnum.AVGPRICE), result.Value[0].Key);

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

                // AVGPRICE formula = (open + high + low + close) / 4
                double expectedAvgPrice = (prices[rowIndex, 1] + prices[rowIndex, 2] +
                                           prices[rowIndex, 3] + prices[rowIndex, 4]) / 4.0;

                Assert.Equal(expectedAvgPrice, result.Value[0].Value, 6); // 6 decimal precision
            }
        }

        [Fact]
        public void Calculate_ShouldThrowArgumentNullException_WhenPricesIsNull()
        {
            var parameters = new Dictionary<string, string>() { { nameof(ParameterNamesEnum.NA), "" } };
            // Arrange
            var calculator = new AvgPriceCalculator(parameters, nameof(CalculatorNameEnum.AVGPRICE));

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => calculator.Calculate(null));
            Assert.Contains("prices", exception.ParamName);
        }

        [Fact]
        public void Calculate_ShouldThrowArgumentException_WhenPricesHasInvalidColumns()
        {
            var parameters = new Dictionary<string, string>() { { nameof(ParameterNamesEnum.NA), "" } };
            // Arrange
            var calculator = new AvgPriceCalculator(parameters, nameof(CalculatorNameEnum.AVGPRICE));
            double[,] invalidPrices = new double[5, 3]; // Only 3 columns instead of required 6

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => calculator.Calculate(invalidPrices));
            Assert.Contains("columns", exception.Message);
        }

        [Fact]
        public void Calculate_ShouldHandleEmptyPricesArray()
        {
            var parameters = new Dictionary<string, string>() { { nameof(ParameterNamesEnum.NA), "" } };
            // Arrange
            var calculator = new AvgPriceCalculator(parameters, nameof(CalculatorNameEnum.AVGPRICE));
            double[,] emptyPrices = new double[0, 6];

            // Act
            var results = calculator.Calculate(emptyPrices, true);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(nameof(CalculatorNameEnum.AVGPRICE), results.Name);
            Assert.Empty(results.Results);
        }

        [Fact]
        public void Calculate_ShouldProduceConsistentResults_WhenCalledMultipleTimes()
        {
            var parameters = new Dictionary<string, string>() { { nameof(ParameterNamesEnum.NA), "" } };
            // Arrange
            var calculator = new AvgPriceCalculator(parameters, nameof(CalculatorNameEnum.AVGPRICE));
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
            var calculator = new AvgPriceCalculator(parameters, nameof(CalculatorNameEnum.AVGPRICE));
            double[,] extremePrices = new double[2, 6]
            {
                { 1625097600, 1000000, 1000000, 1000000, 1000000, 1000 }, // Very high prices
                { 1625184000, 0.00001, 0.00002, 0.00001, 0.00002, 1000 }  // Very low prices
            };

            // Act
            var results = calculator.Calculate(extremePrices, true);

            // Assert
            Assert.NotNull(results);
            Assert.Equal(2, results.Results.Count);

            // High price case
            Assert.Equal(1000000, results.Results[1625097600][0].Value);

            // Low price case
            Assert.Equal(0.000015, results.Results[1625184000][0].Value, 6);
        }

    }
}
