using HC.TechnicalCalculators.Src.Models;
using TALib;

namespace HC.TechnicalCalculators.Src.Calculators.Overlap
{
    /// <summary>
    /// Double Exponential Moving Average (DEMA) Calculator
    /// Time Complexity: O(n) where n is the number of price data points
    /// Space Complexity: O(n) for output storage, O(1) for algorithm computation
    /// </summary>
    public class DemaCalculator : BaseCalculator
    {
        private const int MIN_PERIOD = 2;
        private const int MAX_PERIOD = 200;
        private const int DEFAULT_PERIOD = 21;

        public DemaCalculator(Dictionary<string, string> para, string name) : base(name, para)
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

            var length = Close.Length;
            int outBegIdx, outNbElement;
            int outBegIdx2, outNbElement2;
            double[] ema1 = new double[length];
            double[] ema2 = new double[length];
            double[] dema = new double[length];

            var retCode1 = Core.Ema(Close, 0, Close.Length - 1, ema1, out outBegIdx, out outNbElement, period);
            ValidateTALibResult(retCode1, nameof(CalculatorNameEnum.EMA));
            var retCode2 = Core.Ema(ema1, 0, ema1.Length - 1, ema2, out outBegIdx2, out outNbElement2, period);
            ValidateTALibResult(retCode2, nameof(CalculatorNameEnum.EMA));

            for (int i = 0; i < length; i++)
            {
                dema[i] = 2 * ema1[i] - ema2[i];
            }

            var results = new Dictionary<long, KeyValuePair<string, double>[]>();

            for (int i = outBegIdx2; i < outBegIdx2 + outNbElement2; i++)
            {
                long timestamp = (long)prices[i, 0];
                results[timestamp] = new KeyValuePair<string, double>[]
                {
                    new KeyValuePair<string, double>(nameof(TechnicalNamesEnum.DEMA), dema[i])
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
            return new string[] { nameof(TechnicalNamesEnum.DEMA) };
        }
        public static Dictionary<string, (double Min, double Max, ParameterValueTypeEnum valueType)> GetParameterConstraints()
        {
            return new Dictionary<string, (double Min, double Max, ParameterValueTypeEnum valueType)>
            {
                { nameof(ParameterNamesEnum.Period), (MIN_PERIOD, MAX_PERIOD, ParameterValueTypeEnum.INT) }
            };
        }
    }
}

