using HC.TechnicalCalculators.Src.Models;
using TALib;

namespace HC.TechnicalCalculators.Src.Calculators.Momentum
{
    public class ApoCalculator : BaseCalculator
    {
        private const int MIN_FAST_PERIOD = 2;
        private const int MAX_FAST_PERIOD = 50;
        private const int DEFAULT_FAST_PERIOD = 12;

        private const int MIN_SLOW_PERIOD = 3;
        private const int MAX_SLOW_PERIOD = 100;
        private const int DEFAULT_SLOW_PERIOD = 26;

        public ApoCalculator(Dictionary<string, string> para, string name) : base(name, para)
        {
            ValidateParameters();
        }

        private void ValidateParameters()
        {
            if (!parameters.ContainsKey(nameof(ParameterNamesEnum.FastPeriod)))
            {
                parameters[nameof(ParameterNamesEnum.FastPeriod)] = DEFAULT_FAST_PERIOD.ToString();
            }

            if (!parameters.ContainsKey(nameof(ParameterNamesEnum.SlowPeriod)))
            {
                parameters[nameof(ParameterNamesEnum.SlowPeriod)] = DEFAULT_SLOW_PERIOD.ToString();
            }

            if (!int.TryParse(parameters[nameof(ParameterNamesEnum.FastPeriod)], out int fastPeriod) ||
                fastPeriod < MIN_FAST_PERIOD || fastPeriod > MAX_FAST_PERIOD)
            {
                throw new ArgumentException($"FastPeriod must be between {MIN_FAST_PERIOD} and {MAX_FAST_PERIOD}.");
            }

            if (!int.TryParse(parameters[nameof(ParameterNamesEnum.SlowPeriod)], out int slowPeriod) ||
                slowPeriod < MIN_SLOW_PERIOD || slowPeriod > MAX_SLOW_PERIOD)
            {
                throw new ArgumentException($"SlowPeriod must be between {MIN_SLOW_PERIOD} and {MAX_SLOW_PERIOD}.");
            }

            if (fastPeriod >= slowPeriod)
            {
                throw new ArgumentException("FastPeriod must be less than SlowPeriod.");
            }
        }

        protected override CalculatorResults CalculateInternal(double[,] prices)
        {

            var fastPeriod = int.Parse(parameters[nameof(ParameterNamesEnum.FastPeriod)]);
            var slowPeriod = int.Parse(parameters[nameof(ParameterNamesEnum.SlowPeriod)]);

            if (fastPeriod <= 0 || slowPeriod <= 0)
            {
                throw new ArgumentException($"{nameof(ParameterNamesEnum.FastPeriod)} and {nameof(ParameterNamesEnum.SlowPeriod)} must be greater than zero.");
            }

            var apo = CalculateAPO(Close, fastPeriod, slowPeriod);
            var results = new Dictionary<long, KeyValuePair<string, double>[]>();

            for (int i = 0; i < apo.Length; i++)
            {
                long timestamp = (long)prices[i, 0];
                results[timestamp] = new KeyValuePair<string, double>[]
                {
                    new KeyValuePair<string, double>(nameof(TechnicalNamesEnum.APO), apo[i])
                };
            }

            return new CalculatorResults
            {
                Name = _name,
                Results = results.OrderByDescending(x => x.Key).ToDictionary(x => x.Key, x => x.Value)
            };
        }
        private double[] CalculateAPO(double[] prices, int fastPeriod, int slowPeriod)
        {
            int outBegIdx, outNbElement;
            double[] apo = new double[prices.Length];
            Core.Apo(prices, 0, prices.Length - 1, apo, out outBegIdx, out outNbElement, Core.MAType.Sma, fastPeriod, slowPeriod);
            return apo;
        }
        public static IReadOnlyList<string> GetTechnicalIndicatorNames()
        {
            return new string[] { nameof(TechnicalNamesEnum.APO) };
        }
        public static Dictionary<string, (double Min, double Max, ParameterValueTypeEnum valueType)> GetParameterConstraints()
        {
            return new Dictionary<string, (double Min, double Max, ParameterValueTypeEnum valueType)>
            {
                { nameof(ParameterNamesEnum.FastPeriod), (MIN_FAST_PERIOD, MAX_FAST_PERIOD, ParameterValueTypeEnum.INT) },
                { nameof(ParameterNamesEnum.SlowPeriod), (MIN_SLOW_PERIOD, MAX_SLOW_PERIOD, ParameterValueTypeEnum.INT) }
            };
        }
    }
}
