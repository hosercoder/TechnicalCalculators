using HC.TechnicalCalculators.Src.Models;
using TALib;

namespace HC.TechnicalCalculators.Src.Calculators.Statistics
{
    /// <summary>
    /// Beta Coefficient Calculator
    /// Time Complexity: O(n) where n is the number of price data points
    /// Space Complexity: O(n) for output storage, O(1) for algorithm computation
    /// </summary>
    public class BetaCalculator
    {

        public CalculatorResults Calculate(double[,] prices, double[,] marketData, Dictionary<string, string> parameters)
        {
            if (!parameters.ContainsKey("marketPrices"))
            {
                throw new ArgumentException("Parameter 'marketPrices' is required.");
            }
            if (!parameters.ContainsKey("period"))
            {
                throw new ArgumentException("Parameter 'period' is required.");
            }

            var period = int.Parse(parameters["period"]);
            var marketPrices = marketData;

            if (period <= 0)
            {
                throw new ArgumentException("Period must be greater than zero.");
            }

            if (prices == null || marketPrices == null)
            {
                throw new ArgumentNullException(nameof(prices), "Prices array cannot be null.");
            }

            if (prices.GetLength(0) < period || marketPrices.GetLength(0) < period)
            {
                throw new ArgumentException("Price array is too short for the given period.");
            }

            var length = prices.GetLength(0);
            double[] stockClose = new double[length];
            double[] marketClose = new double[length];
            for (int i = 0; i < length; i++)
            {
                stockClose[i] = prices[i, 3];
                marketClose[i] = marketPrices[i, 3];
            }

            var beta = CalculateBETA(stockClose, marketClose, period);
            var results = new Dictionary<long, KeyValuePair<string, double>[]>();

            for (int i = 0; i < beta.Length; i++)
            {
                long timestamp = (long)prices[i, 0];
                results[timestamp] = new KeyValuePair<string, double>[]
                {
                    new KeyValuePair<string, double>(nameof(TechnicalNamesEnum.BETA), beta[i])
                };
            }

            return new CalculatorResults
            {
                Name = "BETA",
                Results = results.OrderByDescending(x => x.Key).ToDictionary(x => x.Key, x => x.Value)
            };
        }

        private double[] CalculateBETA(double[] stockClose, double[] marketClose, int period)
        {
            int outBegIdx, outNbElement;
            double[] beta = new double[stockClose.Length];
            Core.Beta(stockClose, marketClose, 0, stockClose.Length - 1, beta, out outBegIdx, out outNbElement, period);
            return beta;
        }
    }
}
