using HC.TechnicalCalculators.Src.Models;
using TALib;

namespace HC.TechnicalCalculators.Src.Calculators.Overlap
{
    /// <summary>
    /// Bollinger Bands Calculator
    /// Time Complexity: O(n) where n is the number of price data points
    /// Space Complexity: O(n) for output storage, O(1) for algorithm computation
    /// </summary>
    public class BollingerBandsCalculator : BaseCalculator
    {
        private const int MIN_PERIOD = 2;
        private const int MAX_PERIOD = 200;
        private const int DEFAULT_PERIOD = 20;

        private const double MIN_MULTIPLIER = 0.1;
        private const double MAX_MULTIPLIER = 5.0;
        private const double DEFAULT_MULTIPLIER = 2.0;

        public BollingerBandsCalculator(Dictionary<string, string> para, string name) : base(name, para)
        {
            if (!parameters.ContainsKey(nameof(ParameterNamesEnum.Period)) || !parameters.ContainsKey(nameof(ParameterNamesEnum.Multiplier)))
            {
                throw new ArgumentException($"Parameters {nameof(ParameterNamesEnum.Period)} and {nameof(ParameterNamesEnum.Multiplier)} are required.");
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

            if (!double.TryParse(parameters[nameof(ParameterNamesEnum.Multiplier)], out double multiplier) ||
                multiplier < MIN_MULTIPLIER || multiplier > MAX_MULTIPLIER)
            {
                throw new ArgumentException($"Multiplier must be between {MIN_MULTIPLIER} and {MAX_MULTIPLIER}.");
            }
        }

        protected override CalculatorResults CalculateInternal(double[,] prices)
        {
            var period = int.Parse(parameters[nameof(ParameterNamesEnum.Period)]);
            var multiplier = double.Parse(parameters[nameof(ParameterNamesEnum.Multiplier)]);

            var length = Close.Length;

            int outBegIdx, outNbElement;
            double[] upperBand = new double[length];
            double[] middleBand = new double[length];
            double[] lowerBand = new double[length];

            var retCode = Core.Bbands(Close, 0, length - 1, upperBand, middleBand, lowerBand, out outBegIdx, out outNbElement);
            ValidateTALibResult(retCode, nameof(CalculatorNameEnum.BBANDS));

            var results = new Dictionary<long, KeyValuePair<string, double>[]>();

            for (int i = outBegIdx; i < outBegIdx + outNbElement; i++)
            {
                long timestamp = (long)prices[i, 0];
                results[timestamp] = new KeyValuePair<string, double>[]
                {
                    new KeyValuePair<string, double>(nameof(TechnicalNamesEnum.MIDDLEBAND), middleBand[i - outBegIdx]),
                    new KeyValuePair<string, double>(nameof(TechnicalNamesEnum.UPPERBAND), upperBand[i - outBegIdx]),
                    new KeyValuePair<string, double>(nameof(TechnicalNamesEnum.LOWERBAND), lowerBand[i - outBegIdx])
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
                nameof(TechnicalNamesEnum.MIDDLEBAND),
                nameof(TechnicalNamesEnum.UPPERBAND),
                nameof(TechnicalNamesEnum.LOWERBAND)};
        }
        public static Dictionary<string, (double Min, double Max, ParameterValueTypeEnum valueType)> GetParameterConstraints()
        {
            return new Dictionary<string, (double Min, double Max, ParameterValueTypeEnum valueType)>
            {
                { nameof(ParameterNamesEnum.Period), (MIN_PERIOD, MAX_PERIOD, ParameterValueTypeEnum.INT) },
                { nameof(ParameterNamesEnum.Multiplier), (MIN_MULTIPLIER, MAX_MULTIPLIER, ParameterValueTypeEnum.DOUBLE) }
            };
        }
    }
}

