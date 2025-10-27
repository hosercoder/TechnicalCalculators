using HC.TechnicalCalculators.Src.Models;
using TALib;

namespace HC.TechnicalCalculators.Src.Calculators.Volume
{
    /// <summary>
    /// Chaikin Accumulation/Distribution Line Calculator
    /// Time Complexity: O(n) where n is the number of price data points
    /// Space Complexity: O(n) for output storage, O(1) for algorithm computation
    /// </summary>
    public class ChaikinAdLineCalculator : BaseCalculator
    {
        public ChaikinAdLineCalculator(Dictionary<string, string> para, string name) : base(name, para)
        {
        }
        protected override CalculatorResults CalculateInternal(double[,] prices)
        {
            var adLine = CalculateChaikinAdLine(High, Low, Close, Volume);
            var results = new Dictionary<long, KeyValuePair<string, double>[]>();

            for (int i = 0; i < adLine.Length; i++)
            {
                long timestamp = (long)prices[i, 0];
                results[timestamp] = new KeyValuePair<string, double>[]
                {
                    new KeyValuePair<string, double>(nameof(TechnicalNamesEnum.CHAIKINADLINE), adLine[i])
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
            return new string[] { nameof(TechnicalNamesEnum.CHAIKINADLINE) };
        }
        public static Dictionary<string, (double Min, double Max, ParameterValueTypeEnum valueType)> GetParameterConstraints()
        {
            return new Dictionary<string, (double Min, double Max, ParameterValueTypeEnum valueType)>
            {
                { nameof(ParameterNamesEnum.NA), (0, 0, ParameterValueTypeEnum.INT) }
            };
        }
        private double[] CalculateChaikinAdLine(double[] high, double[] low, double[] close, double[] volume)
        {
            int outBegIdx, outNbElement;
            double[] adLine = new double[high.Length];
            Core.Ad(high, low, close, volume, 0, high.Length - 1, adLine, out outBegIdx, out outNbElement);
            return adLine;
        }
    }
}

