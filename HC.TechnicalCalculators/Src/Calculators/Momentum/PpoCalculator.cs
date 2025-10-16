using HC.TechnicalCalculators.Src.Calculators;
using HC.TechnicalCalculators.Src.Models;
using TALib;

namespace HC.TechnicalCalculators.Src.Calculators.Momentum
{
    public class PpoCalculator : BaseCalculator
    {
        private const int MIN_FAST_PERIOD = 2;
        private const int MAX_FAST_PERIOD = 50;
        private const int DEFAULT_FAST_PERIOD = 12;

        private const int MIN_SLOW_PERIOD = 2;
        private const int MAX_SLOW_PERIOD = 100;
        private const int DEFAULT_SLOW_PERIOD = 26;

        private const int MIN_SIGNAL_PERIOD = 2;
        private const int MAX_SIGNAL_PERIOD = 50;
        private const int DEFAULT_SIGNAL_PERIOD = 9;

        public PpoCalculator(Dictionary<string, string> para, string name) : base(name, para)
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

            if (!parameters.ContainsKey(nameof(ParameterNamesEnum.SignalPeriod)))
            {
                parameters[nameof(ParameterNamesEnum.SignalPeriod)] = DEFAULT_SIGNAL_PERIOD.ToString();
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

            if (!int.TryParse(parameters[nameof(ParameterNamesEnum.SignalPeriod)], out int signalPeriod) ||
                signalPeriod < MIN_SIGNAL_PERIOD || signalPeriod > MAX_SIGNAL_PERIOD)
            {
                throw new ArgumentException($"SignalPeriod must be between {MIN_SIGNAL_PERIOD} and {MAX_SIGNAL_PERIOD}.");
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
            var signalPeriod = int.Parse(parameters[nameof(ParameterNamesEnum.SignalPeriod)]);

            if (fastPeriod <= 0 || slowPeriod <= 0 || signalPeriod <= 0)
            {
                throw new ArgumentException($"{nameof(ParameterNamesEnum.FastPeriod)}, {nameof(ParameterNamesEnum.SlowPeriod)}, and {nameof(ParameterNamesEnum.SignalPeriod)} must be greater than zero.");
            }

            var length = Close.Length;
            var ppo = new double[length];

            CalculatePPO(Close, fastPeriod, slowPeriod, signalPeriod, out ppo);

            var results = new Dictionary<long, KeyValuePair<string, double>[]>();

            for (int i = 0; i < ppo.Length; i++)
            {
                long timestamp = (long)prices[i, 0];
                results[timestamp] = new KeyValuePair<string, double>[]
                {
                    new KeyValuePair<string, double>(nameof(TechnicalNamesEnum.PPO), ppo[i]),
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
            return new string[] { 
                nameof(TechnicalNamesEnum.PPO),
                nameof(TechnicalNamesEnum.PPOHIST),
                nameof(TechnicalNamesEnum.PPOSIGNAL)
            };
        }
        public static Dictionary<string, (double Min, double Max, ParameterValueTypeEnum valueType)> GetParameterConstraints()
        {
            return new Dictionary<string, (double Min, double Max, ParameterValueTypeEnum valueType)>
            {
                { nameof(ParameterNamesEnum.FastPeriod), (MIN_FAST_PERIOD, MAX_FAST_PERIOD, ParameterValueTypeEnum.INT) },
                { nameof(ParameterNamesEnum.SlowPeriod), (MIN_SLOW_PERIOD, MAX_SLOW_PERIOD, ParameterValueTypeEnum.INT) },
                { nameof(ParameterNamesEnum.SignalPeriod), (MIN_SIGNAL_PERIOD, MAX_SIGNAL_PERIOD, ParameterValueTypeEnum.INT) }
            };
        }
        private void CalculatePPO(double[] prices, int fastPeriod, int slowPeriod, int signalPeriod, out double[] ppo)
        {
            int outBegIdx, outNbElement;
            ppo = new double[prices.Length];
            Core.Ppo(prices, 0, prices.Length - 1, ppo, out outBegIdx, out outNbElement, Core.MAType.Sma, fastPeriod, slowPeriod);
        }
    }
}

