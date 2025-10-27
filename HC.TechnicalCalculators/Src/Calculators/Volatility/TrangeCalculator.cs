using HC.TechnicalCalculators.Src.Models;
using TALib;

namespace HC.TechnicalCalculators.Src.Calculators.Volatility
{
    /// <summary>
    /// True Range Calculator
    /// Time Complexity: O(n) where n is the number of price data points
    /// Space Complexity: O(n) for output storage, O(1) for algorithm computation
    /// </summary>
    public class TrangeCalculator : BaseCalculator
    {
        public TrangeCalculator(Dictionary<string, string> para, string name) : base(name, para)
        {
        }
        protected override CalculatorResults CalculateInternal(double[,] prices)
        {
            var trange = CalculateTRANGE(High, Low, Close);
            var results = new Dictionary<long, KeyValuePair<string, double>[]>();

            for (int i = 0; i < trange.Length; i++)
            {
                long timestamp = (long)prices[i, 0];
                results[timestamp] = new KeyValuePair<string, double>[]
                {
                    new KeyValuePair<string, double>(nameof(TechnicalNamesEnum.TRANGE), trange[i])
                };
            }

            return new CalculatorResults
            {
                Name = _name,
                Results = results.OrderByDescending(x => x.Key).ToDictionary(x => x.Key, x => x.Value)
            };
        }
        public static IReadOnlyList<string> GetTechnicalIndicatorNames()
        {
            return new string[] { nameof(TechnicalNamesEnum.TRANGE) };
        }
        public static Dictionary<string, (double Min, double Max, ParameterValueTypeEnum valueType)> GetParameterConstraints()
        {
            return new Dictionary<string, (double Min, double Max, ParameterValueTypeEnum valueType)>
            {
                { nameof(ParameterNamesEnum.NA), (0, 0, ParameterValueTypeEnum.INT) }
            };
        }
        private double[] CalculateTRANGE(double[] high, double[] low, double[] close)
        {
            int outBegIdx, outNbElement;
            double[] trange = new double[high.Length];
            Core.TRange(high, low, close, 0, high.Length - 1, trange, out outBegIdx, out outNbElement);
            return trange;
        }
    }
}

