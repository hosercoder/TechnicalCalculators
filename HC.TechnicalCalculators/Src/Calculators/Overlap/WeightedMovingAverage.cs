using HC.TechnicalCalculators.Src.Calculators;
using HC.TechnicalCalculators.Src.Models;
using TALib;

namespace HC.TechnicalCalculators.Src.Calculators.Overlap
{
    public class WeightedMovingAverage : BaseCalculator
    {
        private const int MIN_PERIOD = 2;
        private const int MAX_PERIOD = 200;
        private const int DEFAULT_PERIOD = 20;

        public WeightedMovingAverage(Dictionary<string, string> para, string name) : base(name, para)
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

            int outBegIdx, outNbElement;
            double[] movingAvg = new double[Close.Length];

            Core.Sma(Close, 0, Close.Length - 1, movingAvg, out outBegIdx, out outNbElement, period);

            var results = new Dictionary<long, KeyValuePair<string, double>[]>();

            for (int i = outBegIdx; i < outBegIdx + outNbElement; i++)
            {
                long timestamp = (long)prices[i, 0];
                results[timestamp] = new KeyValuePair<string, double>[]
                {
                    new KeyValuePair<string, double>(nameof(TechnicalNamesEnum.MOVINGAVERAGE), movingAvg[i - outBegIdx])
                };
            }

            return new CalculatorResults
            {
                Name = _name,
                Results = results.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value)
            };
        }
        public static IReadOnlyList<string> GetTechnicalIndicatorNames()
        {
            return new string[] { nameof(TechnicalNamesEnum.MOVINGAVERAGE) };
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

