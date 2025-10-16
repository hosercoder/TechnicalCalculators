using HC.TechnicalCalculators.Src.Calculators;
using HC.TechnicalCalculators.Src.Models;
using TALib;

namespace HC.TechnicalCalculators.Src.Calculators.Overlap
{
    public class KaufmanAdaptiveMovingAverageCalculator : BaseCalculator
    {
        private const int MIN_PERIOD = 2;
        private const int MAX_PERIOD = 100;
        private const int DEFAULT_PERIOD = 14;

        public KaufmanAdaptiveMovingAverageCalculator(Dictionary<string, string> para, string name) : base(name, para)
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

            var kama = CalculateKAMA(Close, period);
            var results = new Dictionary<long, KeyValuePair<string, double>[]>();

            for (int i = 0; i < kama.Length; i++)
            {
                long timestamp = (long)prices[i, 0];
                results[timestamp] = new KeyValuePair<string, double>[]
                {
                    new KeyValuePair<string, double>(nameof(TechnicalNamesEnum.KAMA), kama[i])
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
            return new string[] { nameof(TechnicalNamesEnum.KAMA) };
        }
        public static Dictionary<string, (double Min, double Max, ParameterValueTypeEnum valueType)> GetParameterConstraints()
        {
            return new Dictionary<string, (double Min, double Max, ParameterValueTypeEnum valueType)>
            {
                { nameof(ParameterNamesEnum.Period), (MIN_PERIOD, MAX_PERIOD, ParameterValueTypeEnum.INT) }
            };
        }
        private double[] CalculateKAMA(double[] prices, int period)
        {
            int outBegIdx, outNbElement;
            double[] kama = new double[prices.Length];
            Core.Kama(prices, 0, prices.Length - 1, kama, out outBegIdx, out outNbElement, period);
            return kama;
        }
    }
}

