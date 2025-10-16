using HC.TechnicalCalculators.Src.Calculators;
using HC.TechnicalCalculators.Src.Models;
using TALib;

namespace HC.TechnicalCalculators.Src.Calculators.Momentum
{
    /// <summary>
    /// Stochastic Oscillator Calculator
    /// Time Complexity: O(n) where n is the number of price data points
    /// Space Complexity: O(n) for output storage, O(1) for algorithm computation
    /// </summary>
    public class StochCalculator : BaseCalculator
    {
        private const int MIN_K_PERIOD = 1;
        private const int MAX_K_PERIOD = 100;
        private const int DEFAULT_K_PERIOD = 14;

        private const int MIN_D_PERIOD = 1;
        private const int MAX_D_PERIOD = 100;
        private const int DEFAULT_D_PERIOD = 3;

        public StochCalculator(Dictionary<string, string> para, string name) : base(name, para)
        {
            ValidateParameters();
        }

        private void ValidateParameters()
        {
            if (!parameters.ContainsKey(nameof(ParameterNamesEnum.FastKPeriod)))
            {
                parameters[nameof(ParameterNamesEnum.FastKPeriod)] = DEFAULT_K_PERIOD.ToString();
            }

            if (!parameters.ContainsKey(nameof(ParameterNamesEnum.SlowDPeriod)))
            {
                parameters[nameof(ParameterNamesEnum.SlowDPeriod)] = DEFAULT_D_PERIOD.ToString();
            }

            if (!parameters.ContainsKey(nameof(ParameterNamesEnum.SlowKPeriod)))
            {
                parameters[nameof(ParameterNamesEnum.SlowKPeriod)] = DEFAULT_K_PERIOD.ToString();
            }

            if (!int.TryParse(parameters[nameof(ParameterNamesEnum.FastKPeriod)], out int kPeriod) ||
                kPeriod < MIN_K_PERIOD || kPeriod > MAX_K_PERIOD)
            {
                throw new ArgumentException($"FastKPeriod must be between {MIN_K_PERIOD} and {MAX_K_PERIOD}.");
            }

            if (!int.TryParse(parameters[nameof(ParameterNamesEnum.SlowDPeriod)], out int dPeriod) ||
                dPeriod < MIN_D_PERIOD || dPeriod > MAX_D_PERIOD)
            {
                throw new ArgumentException($"SlowDPeriod must be between {MIN_D_PERIOD} and {MAX_D_PERIOD}.");
            }

            if (!int.TryParse(parameters[nameof(ParameterNamesEnum.SlowKPeriod)], out int slowKPeriod) ||
                slowKPeriod < MIN_K_PERIOD || slowKPeriod > MAX_K_PERIOD)
            {
                throw new ArgumentException($"SlowKPeriod must be between {MIN_K_PERIOD} and {MAX_K_PERIOD}.");
            }
        }
        protected override CalculatorResults CalculateInternal(double[,] prices)
        {
            var fastKPeriod = int.Parse(parameters[nameof(ParameterNamesEnum.FastKPeriod)]);
            var slowKPeriod = int.Parse(parameters[nameof(ParameterNamesEnum.SlowKPeriod)]);
            var slowDPeriod = int.Parse(parameters[nameof(ParameterNamesEnum.SlowDPeriod)]);

            if (fastKPeriod <= 0 || slowKPeriod <= 0 || slowDPeriod <= 0)
            {
                throw new ArgumentException($"{nameof(ParameterNamesEnum.FastKPeriod)}, {nameof(ParameterNamesEnum.SlowKPeriod)}, and {nameof(ParameterNamesEnum.SlowDPeriod)} must be greater than zero.");
            }
            var length = High.Length;

            var stochK = new double[length];
            var stochD = new double[length];
            CalculateSTOCH(High, Low, Close, fastKPeriod, slowKPeriod, slowDPeriod, out stochK, out stochD);

            var results = new Dictionary<long, KeyValuePair<string, double>[]>();

            for (int i = 0; i < stochK.Length; i++)
            {
                long timestamp = (long)prices[i, 0];
                results[timestamp] = new KeyValuePair<string, double>[]
                {
                    new KeyValuePair<string, double>(nameof(TechnicalNamesEnum.STOCHK), stochK[i]),
                    new KeyValuePair<string, double>(nameof(TechnicalNamesEnum.STOCHD), stochD[i])
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
            return new string[] { nameof(TechnicalNamesEnum.STOCHK), nameof(TechnicalNamesEnum.STOCHD) };
        }
        public static Dictionary<string, (double Min, double Max, ParameterValueTypeEnum valueType)> GetParameterConstraints()
        {
            return new Dictionary<string, (double Min, double Max, ParameterValueTypeEnum valueType)>
            {
                { nameof(ParameterNamesEnum.FastKPeriod), (MIN_K_PERIOD, MAX_K_PERIOD, ParameterValueTypeEnum.INT) },
                { nameof(ParameterNamesEnum.SlowKPeriod), (MIN_K_PERIOD, MAX_K_PERIOD, ParameterValueTypeEnum.INT) },
                { nameof(ParameterNamesEnum.SlowDPeriod), (MIN_D_PERIOD, MAX_D_PERIOD, ParameterValueTypeEnum.INT) }
            };
        }
        private void CalculateSTOCH(double[] high, double[] low, double[] close, int fastKPeriod, int slowKPeriod, int slowDPeriod, out double[] stochK, out double[] stochD)
        {
            int outBegIdx, outNbElement;
            stochK = new double[high.Length];
            stochD = new double[high.Length];
            var retCode = Core.Stoch(high, low, close, 0, high.Length - 1, stochK, stochD, out outBegIdx, out outNbElement,
                Core.MAType.Sma, Core.MAType.Sma, fastKPeriod, slowKPeriod, slowDPeriod);
            ValidateTALibResult(retCode, nameof(CalculatorNameEnum.STOCH));
        }
    }
}

