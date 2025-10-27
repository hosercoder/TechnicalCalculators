using HC.TechnicalCalculators.Src.Models;
using TALib;

namespace HC.TechnicalCalculators.Src.Calculators.Volume
{
    /// <summary>
    /// On-Balance Volume (OBV) Calculator
    /// Time Complexity: O(n) where n is the number of price data points
    /// Space Complexity: O(n) for output storage, O(1) for algorithm computation
    /// </summary>
    public class ObvCalculator : BaseCalculator
    {
        public ObvCalculator(Dictionary<string, string> para, string name) : base(name, para)
        {
        }
        protected override CalculatorResults CalculateInternal(double[,] prices)
        {
            var obv = CalculateOBV(Close, Volume);
            var results = new Dictionary<long, KeyValuePair<string, double>[]>();

            for (int i = 0; i < obv.Length; i++)
            {
                long timestamp = (long)prices[i, 0];
                results[timestamp] = new KeyValuePair<string, double>[]
                {
                    new KeyValuePair<string, double>(nameof(TechnicalNamesEnum.OBV), obv[i])
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
            return new string[] { nameof(TechnicalNamesEnum.OBV) };
        }
        public static Dictionary<string, (double Min, double Max, ParameterValueTypeEnum valueType)> GetParameterConstraints()
        {
            return new Dictionary<string, (double Min, double Max, ParameterValueTypeEnum valueType)>
            {
                { nameof(ParameterNamesEnum.NA), (0, 0, ParameterValueTypeEnum.INT) }
            };
        }
        private double[] CalculateOBV(double[] close, double[] volume)
        {
            int outBegIdx, outNbElement;
            double[] obv = new double[close.Length];
            var retCode = Core.Obv(close, volume, 0, close.Length - 1, obv, out outBegIdx, out outNbElement);
            ValidateTALibResult(retCode, nameof(CalculatorNameEnum.OBV));
            return obv;
        }
    }
}

