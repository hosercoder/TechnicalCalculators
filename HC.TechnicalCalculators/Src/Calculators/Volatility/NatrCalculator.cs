using HC.TechnicalCalculators.Src.Calculators;
using HC.TechnicalCalculators.Src.Models;
using TALib;

namespace HC.TechnicalCalculators.Src.Calculators.Volatility
{
    public class NatrCalculator : BaseCalculator
    {
        private const int MIN_PERIOD = 1;
        private const int MAX_PERIOD = 100;
        private const int DEFAULT_PERIOD = 14;

        public NatrCalculator(Dictionary<string, string> para, string name) : base(name, para)
        {
            ValidateParameters();
        }

        private void ValidateParameters()
        {
            if (!parameters.ContainsKey(nameof(ParameterNamesEnum.Period)))
            {
                parameters[nameof(ParameterNamesEnum.Period)] = DEFAULT_PERIOD.ToString();
            }

            // Only validate range if the value can be parsed as an integer
            // Let FormatException be thrown during calculation for invalid formats
            if (int.TryParse(parameters[nameof(ParameterNamesEnum.Period)], out int period))
            {
                if (period < MIN_PERIOD || period > MAX_PERIOD)
                {
                    throw new ArgumentException($"Period must be between {MIN_PERIOD} and {MAX_PERIOD}.");
                }
            }
        }
        protected override CalculatorResults CalculateInternal(double[,] prices)
        {
            var period = int.Parse(parameters[nameof(ParameterNamesEnum.Period)]);

            if (prices.GetLength(0) < period)
            {
                throw new ArgumentException($"Not enough data points for NATR calculation. Minimum required: {period}");
            }

            var natr = CalculateNATR(High, Low, Close, period);
            var results = new Dictionary<long, KeyValuePair<string, double>[]>();

            for (int i = 0; i < natr.Length; i++)
            {
                long timestamp = (long)prices[i, 0];
                results[timestamp] = new KeyValuePair<string, double>[]
                {
                    new KeyValuePair<string, double>(nameof(TechnicalNamesEnum.NATR), natr[i])
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
            return new string[] { nameof(TechnicalNamesEnum.NATR) };
        }
        public static Dictionary<string, (double Min, double Max, ParameterValueTypeEnum valueType)> GetParameterConstraints()
        {
            return new Dictionary<string, (double Min, double Max, ParameterValueTypeEnum valueType)>
            {
                { nameof(ParameterNamesEnum.Period), (MIN_PERIOD, MAX_PERIOD, ParameterValueTypeEnum.INT) }
            };
        }
        private double[] CalculateNATR(double[] high, double[] low, double[] close, int period)
        {
            int outBegIdx, outNbElement;
            double[] natr = new double[high.Length];
            Core.Natr(high, low, close, 0, high.Length - 1, natr, out outBegIdx, out outNbElement, period);
            return natr;
        }
    }
}

