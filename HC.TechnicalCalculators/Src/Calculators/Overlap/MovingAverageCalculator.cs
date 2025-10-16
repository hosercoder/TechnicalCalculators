using HC.TechnicalCalculators.Src.Calculators;
using HC.TechnicalCalculators.Src.Models;

namespace HC.TechnicalCalculators.Src.Calculators.Overlap
{
    public class MovingAverageCalculator : BaseCalculator
    {
        private const int MIN_PERIOD = 2;
        private const int MAX_PERIOD = 200;
        private const int DEFAULT_PERIOD = 20;

        public MovingAverageCalculator(Dictionary<string, string> para, string name) : base(name, para)
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

            var movingAverages = CalculateSimpleMovingAverage(Close, period);
            var results = new Dictionary<long, KeyValuePair<string, double>[]>();

            for (int i = 0; i < movingAverages.Count; i++)
            {
                long timestamp = (long)prices[i + period - 1, 0];
                results[timestamp] = new KeyValuePair<string, double>[]
                {
                    new KeyValuePair<string, double>(nameof(TechnicalNamesEnum.MOVINGAVERAGE), movingAverages[i])
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
            return new string[] { nameof(TechnicalNamesEnum.MOVINGAVERAGE) };
        }
        public static Dictionary<string, (double Min, double Max, ParameterValueTypeEnum valueType)> GetParameterConstraints()
        {
            return new Dictionary<string, (double Min, double Max, ParameterValueTypeEnum valueType)>
            {
                { nameof(ParameterNamesEnum.Period), (MIN_PERIOD, MAX_PERIOD, ParameterValueTypeEnum.INT) }
            };
        }
        private List<double> CalculateSimpleMovingAverage(double[] prices, int period)
        {
            List<double> movingAverages = new List<double>();

            for (int i = 0; i <= prices.Length - period; i++)
            {
                double sum = 0;
                for (int j = 0; j < period; j++)
                {
                    sum += prices[i + j];
                }
                movingAverages.Add(sum / period);
            }

            return movingAverages;
        }
    }
}

