using HC.TechnicalCalculators.Src.Calculators;
using HC.TechnicalCalculators.Src.Models;
using TALib;

namespace HC.TechnicalCalculators.Src.Calculators.Overlap
{
    public class MesaAdaptiveMovingAverageCalculator : BaseCalculator
    {
        private const double MIN_FAST_LIMIT = 0.01;
        private const double MAX_FAST_LIMIT = 0.99;
        private const double DEFAULT_FAST_LIMIT = 0.5;

        private const double MIN_SLOW_LIMIT = 0.01;
        private const double MAX_SLOW_LIMIT = 0.99;
        private const double DEFAULT_SLOW_LIMIT = 0.05;

        public MesaAdaptiveMovingAverageCalculator(Dictionary<string, string> para, string name) : base(name, para)
        {
            ValidateParameters();
        }

        private void ValidateParameters()
        {
            if (!parameters.ContainsKey(nameof(ParameterNamesEnum.FastLimit)))
            {
                parameters[nameof(ParameterNamesEnum.FastLimit)] = DEFAULT_FAST_LIMIT.ToString();
            }

            if (!parameters.ContainsKey(nameof(ParameterNamesEnum.SlowLimit)))
            {
                parameters[nameof(ParameterNamesEnum.SlowLimit)] = DEFAULT_SLOW_LIMIT.ToString();
            }

            if (!double.TryParse(parameters[nameof(ParameterNamesEnum.FastLimit)], out double fastLimit) ||
                fastLimit < MIN_FAST_LIMIT || fastLimit > MAX_FAST_LIMIT)
            {
                throw new ArgumentException($"FastLimit must be between {MIN_FAST_LIMIT} and {MAX_FAST_LIMIT}.");
            }

            if (!double.TryParse(parameters[nameof(ParameterNamesEnum.SlowLimit)], out double slowLimit) ||
                slowLimit < MIN_SLOW_LIMIT || slowLimit > MAX_SLOW_LIMIT)
            {
                throw new ArgumentException($"SlowLimit must be between {MIN_SLOW_LIMIT} and {MAX_SLOW_LIMIT}.");
            }

            if (fastLimit <= slowLimit)
            {
                throw new ArgumentException("FastLimit must be greater than SlowLimit.");
            }
        }

        protected override CalculatorResults CalculateInternal(double[,] prices)
        {
            var fastLimit = double.Parse(parameters[nameof(ParameterNamesEnum.FastLimit)]);
            var slowLimit = double.Parse(parameters[nameof(ParameterNamesEnum.SlowLimit)]);

            if (fastLimit <= 0 || slowLimit <= 0)
            {
                throw new ArgumentException($"{nameof(ParameterNamesEnum.FastLimit)} and {nameof(ParameterNamesEnum.SlowLimit)} must be greater than zero.");
            }

            var mama = CalculateMAMA(Close, fastLimit, slowLimit);
            var results = new Dictionary<long, KeyValuePair<string, double>[]>();

            for (int i = 0; i < mama.Length; i++)
            {
                long timestamp = (long)prices[i, 0];
                results[timestamp] = new KeyValuePair<string, double>[]
                {
                    new KeyValuePair<string, double>(nameof(TechnicalNamesEnum.MAMA), mama[i])
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
            return new string[] { nameof(TechnicalNamesEnum.MAMA) };
        }
        public static Dictionary<string, (double Min, double Max, ParameterValueTypeEnum valueType)> GetParameterConstraints()
        {
            return new Dictionary<string, (double Min, double Max, ParameterValueTypeEnum valueType)>
            {
                { nameof(ParameterNamesEnum.FastLimit), (MIN_FAST_LIMIT, MAX_FAST_LIMIT, ParameterValueTypeEnum.DOUBLE) },
                { nameof(ParameterNamesEnum.SlowLimit), (MIN_SLOW_LIMIT, MAX_SLOW_LIMIT, ParameterValueTypeEnum.DOUBLE) }
            };
        }
        private double[] CalculateMAMA(double[] prices, double fastLimit, double slowLimit)
        {
            int outBegIdx, outNbElement;
            double[] mama = new double[prices.Length];
            double[] fama = new double[prices.Length];
            Core.Mama(prices, 0, prices.Length - 1, mama, fama, out outBegIdx, out outNbElement, fastLimit, slowLimit);
            return mama;
        }
    }
}

