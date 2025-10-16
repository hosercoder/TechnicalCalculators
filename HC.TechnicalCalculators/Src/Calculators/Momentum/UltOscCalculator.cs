using HC.TechnicalCalculators.Src.Calculators;
using HC.TechnicalCalculators.Src.Models;
using TALib;

namespace HC.TechnicalCalculators.Src.Calculators.Momentum
{
    public class UltOscCalculator : BaseCalculator
    {
        private const int MIN_PERIOD = 1;
        private const int MAX_PERIOD = 100;
        private const int DEFAULT_SHORT_PERIOD = 7;
        private const int DEFAULT_MEDIUM_PERIOD = 14;
        private const int DEFAULT_LONG_PERIOD = 28;

        public UltOscCalculator(Dictionary<string, string> para, string name) : base(name, para)
        {
            ValidateParameters();
        }

        private void ValidateParameters()
        {
            if (!parameters.ContainsKey(nameof(ParameterNamesEnum.ShortPeriod)))
            {
                parameters[nameof(ParameterNamesEnum.ShortPeriod)] = DEFAULT_SHORT_PERIOD.ToString();
            }

            if (!parameters.ContainsKey(nameof(ParameterNamesEnum.MediumPeriod)))
            {
                parameters[nameof(ParameterNamesEnum.MediumPeriod)] = DEFAULT_MEDIUM_PERIOD.ToString();
            }

            if (!parameters.ContainsKey(nameof(ParameterNamesEnum.LongPeriod)))
            {
                parameters[nameof(ParameterNamesEnum.LongPeriod)] = DEFAULT_LONG_PERIOD.ToString();
            }

            if (!int.TryParse(parameters[nameof(ParameterNamesEnum.ShortPeriod)], out int shortPeriod) ||
                shortPeriod < MIN_PERIOD || shortPeriod > MAX_PERIOD)
            {
                throw new ArgumentException($"ShortPeriod must be between {MIN_PERIOD} and {MAX_PERIOD}.");
            }

            if (!int.TryParse(parameters[nameof(ParameterNamesEnum.MediumPeriod)], out int mediumPeriod) ||
                mediumPeriod < MIN_PERIOD || mediumPeriod > MAX_PERIOD)
            {
                throw new ArgumentException($"MediumPeriod must be between {MIN_PERIOD} and {MAX_PERIOD}.");
            }

            if (!int.TryParse(parameters[nameof(ParameterNamesEnum.LongPeriod)], out int longPeriod) ||
                longPeriod < MIN_PERIOD || longPeriod > MAX_PERIOD)
            {
                throw new ArgumentException($"LongPeriod must be between {MIN_PERIOD} and {MAX_PERIOD}.");
            }

            if (shortPeriod >= mediumPeriod || mediumPeriod >= longPeriod)
            {
                throw new ArgumentException("Periods must be in ascending order: ShortPeriod < MediumPeriod < LongPeriod.");
            }
        }
        protected override CalculatorResults CalculateInternal(double[,] prices)
        {
            var shortPeriod = int.Parse(parameters[nameof(ParameterNamesEnum.ShortPeriod)]);
            var mediumPeriod = int.Parse(parameters[nameof(ParameterNamesEnum.MediumPeriod)]);
            var longPeriod = int.Parse(parameters[nameof(ParameterNamesEnum.LongPeriod)]);

            if (shortPeriod <= 0 || mediumPeriod <= 0 || longPeriod <= 0)
            {
                throw new ArgumentException($"{nameof(ParameterNamesEnum.ShortPeriod)}, {nameof(ParameterNamesEnum.MediumPeriod)}, and {nameof(ParameterNamesEnum.LongPeriod)} must be greater than zero.");
            }

            var ultOsc = CalculateULTOSC(High, Low, Close, shortPeriod, mediumPeriod, longPeriod);
            var results = new Dictionary<long, KeyValuePair<string, double>[]>();

            for (int i = 0; i < ultOsc.Length; i++)
            {
                long timestamp = (long)prices[i, 0];
                results[timestamp] = new KeyValuePair<string, double>[]
                {
                    new KeyValuePair<string, double>(nameof(TechnicalNamesEnum.ULTOSC), ultOsc[i])
                };
            }

            return new CalculatorResults
            {
                Name = _name,
                Results = results.OrderByDescending(x => x.Key).ToDictionary(x => x.Key, x => x.Value)
            };
        }
        public static string[] GetTechnicalIndicatorNames()
        {
            return new string[] { nameof(TechnicalNamesEnum.ULTOSC) };
        }
        public static Dictionary<string, (double Min, double Max, ParameterValueTypeEnum valueType)> GetParameterConstraints()
        {
            return new Dictionary<string, (double Min, double Max, ParameterValueTypeEnum valueType)>
            {
                { nameof(ParameterNamesEnum.ShortPeriod), (MIN_PERIOD, MAX_PERIOD, ParameterValueTypeEnum.INT) },
                { nameof(ParameterNamesEnum.MediumPeriod), (MIN_PERIOD, MAX_PERIOD, ParameterValueTypeEnum.INT) },
                { nameof(ParameterNamesEnum.LongPeriod), (MIN_PERIOD, MAX_PERIOD, ParameterValueTypeEnum.INT) }
            };
        }
        private double[] CalculateULTOSC(double[] high, double[] low, double[] close, int shortPeriod, int mediumPeriod, int longPeriod)
        {
            int outBegIdx, outNbElement;
            double[] ultOsc = new double[high.Length];
            Core.UltOsc(high, low, close, 0, high.Length - 1, ultOsc, out outBegIdx, out outNbElement, shortPeriod, mediumPeriod, longPeriod);
            return ultOsc;
        }
    }
}

