using HC.TechnicalCalculators.Src.Calculators;
using HC.TechnicalCalculators.Src.Models;
using TALib;

namespace HC.TechnicalCalculators.Src.Calculators.Momentum
{
    /// <summary>
    /// Moving Average Convergence Divergence (MACD) Calculator
    /// Time Complexity: O(n) where n is the number of price data points
    /// Space Complexity: O(n) for output storage, O(1) for algorithm computation
    /// </summary>
    public class MacdCalculator : BaseCalculator
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

        public MacdCalculator(Dictionary<string, string> para, string name) : base(name, para)
        {
            if (!parameters.ContainsKey(nameof(ParameterNamesEnum.FastPeriod)) ||
                !parameters.ContainsKey(nameof(ParameterNamesEnum.SlowPeriod)) ||
                !parameters.ContainsKey(nameof(ParameterNamesEnum.SignalPeriod)))
            {
                throw new ArgumentException($"Parameters {nameof(ParameterNamesEnum.FastPeriod)}, {nameof(ParameterNamesEnum.SlowPeriod)}, and {nameof(ParameterNamesEnum.SignalPeriod)} are required.");
            }

            ValidateParameters();
        }

        private void ValidateParameters()
        {
            var fastPeriod = int.Parse(parameters[nameof(ParameterNamesEnum.FastPeriod)]);
            var slowPeriod = int.Parse(parameters[nameof(ParameterNamesEnum.SlowPeriod)]);
            var signalPeriod = int.Parse(parameters[nameof(ParameterNamesEnum.SignalPeriod)]);

            if (fastPeriod < MIN_FAST_PERIOD || fastPeriod > MAX_FAST_PERIOD)
            {
                throw new ArgumentException($"FastPeriod must be between {MIN_FAST_PERIOD} and {MAX_FAST_PERIOD}.");
            }

            if (slowPeriod < MIN_SLOW_PERIOD || slowPeriod > MAX_SLOW_PERIOD)
            {
                throw new ArgumentException($"SlowPeriod must be between {MIN_SLOW_PERIOD} and {MAX_SLOW_PERIOD}.");
            }

            if (signalPeriod < MIN_SIGNAL_PERIOD || signalPeriod > MAX_SIGNAL_PERIOD)
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

            var macd = new double[length];
            var macdSignal = new double[length];
            var macdHist = new double[length];
            CalculateMACD(Close, fastPeriod, slowPeriod, signalPeriod, out macd, out macdSignal, out macdHist);

            var results = new Dictionary<long, KeyValuePair<string, double>[]>();

            for (int i = 0; i < macd.Length; i++)
            {
                long timestamp = (long)prices[i, 0];
                results[timestamp] = new KeyValuePair<string, double>[]
                {
                    new KeyValuePair<string, double>(nameof(TechnicalNamesEnum.MACD), macd[i]),
                    new KeyValuePair<string, double>(nameof(TechnicalNamesEnum.MACDSIGNAL), macdSignal[i]),
                    new KeyValuePair<string, double>(nameof(TechnicalNamesEnum.MACDHIST), macdHist[i])
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
                nameof(TechnicalNamesEnum.MACD),
                nameof(TechnicalNamesEnum.MACDSIGNAL),
                nameof(TechnicalNamesEnum.MACDHIST)};
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
        private void CalculateMACD(double[] prices, int fastPeriod, int slowPeriod, int signalPeriod, out double[] macd, out double[] macdSignal, out double[] macdHist)
        {
            int outBegIdx, outNbElement;
            macd = new double[prices.Length];
            macdSignal = new double[prices.Length];
            macdHist = new double[prices.Length];
            var retCode = Core.Macd(prices, 0, prices.Length - 1, macd, macdSignal, macdHist, out outBegIdx, out outNbElement, fastPeriod, slowPeriod, signalPeriod);
            ValidateTALibResult(retCode, nameof(CalculatorNameEnum.MACD));
        }
    }
}

