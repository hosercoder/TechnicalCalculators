using HC.TechnicalCalculators.Src.Models;
using TALib;

namespace HC.TechnicalCalculators.Src.Calculators.Momentum
{
    /// <summary>
    /// Average Directional Index (ADX) Calculator
    /// Time Complexity: O(n) where n is the number of price data points
    /// Space Complexity: O(n) for output storage, O(1) for algorithm computation
    /// </summary>
    public class AdxCalculator : BaseCalculator
    {
        // ADX period constraints based on common financial analysis practices
        private const int MIN_PERIOD = 2;
        private const int MAX_PERIOD = 100;
        private const int DEFAULT_PERIOD = 14;
        public AdxCalculator(Dictionary<string, string> para, string name) : base(name, para)
        {
            if (!parameters.ContainsKey(nameof(ParameterNamesEnum.Period)))
            {
                throw new ArgumentException($"Parameter {nameof(ParameterNamesEnum.Period)} is required.");
            }

            ValidateParameters();
        }

        private void ValidateParameters()
        {
            if (!int.TryParse(parameters[nameof(ParameterNamesEnum.Period)], out int period) ||
                period < MIN_PERIOD || period > MAX_PERIOD)
            {
                throw new ArgumentException($"Period must be between {MIN_PERIOD} and {MAX_PERIOD}.");
            }
        }

        protected override CalculatorResults CalculateInternal(double[,] prices)
        {

            var period = int.Parse(parameters[nameof(ParameterNamesEnum.Period)]);

            var adx = CalculateADX(High, Low, Close, period);
            var results = new Dictionary<long, KeyValuePair<string, double>[]>();

            for (int i = 0; i < adx.Length; i++)
            {
                long timestamp = (long)prices[i, 0];
                results[timestamp] = new KeyValuePair<string, double>[]
                {
                    new KeyValuePair<string, double>(nameof(TechnicalNamesEnum.ADX), adx[i])
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
            return new string[] { nameof(TechnicalNamesEnum.ADX) };
        }
        public static Dictionary<string, (double Min, double Max, ParameterValueTypeEnum valueType)> GetParameterConstraints()
        {
            return new Dictionary<string, (double Min, double Max, ParameterValueTypeEnum valueType)>
            {
                { nameof(ParameterNamesEnum.Period), (MIN_PERIOD, MAX_PERIOD, ParameterValueTypeEnum.INT) }
            };
        }
        private double[] CalculateADX(double[] high, double[] low, double[] close, int period)
        {
            int outBegIdx, outNbElement;
            double[] adx = new double[high.Length];
            var retCode = Core.Adx(high, low, close, 0, high.Length - 1, adx, out outBegIdx, out outNbElement, period);
            ValidateTALibResult(retCode, nameof(CalculatorNameEnum.ADX));
            return adx;
        }


    }
}
