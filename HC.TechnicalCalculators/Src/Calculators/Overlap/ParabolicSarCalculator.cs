using HC.TechnicalCalculators.Src.Models;
using TALib;

namespace HC.TechnicalCalculators.Src.Calculators.Overlap
{
    /// <summary>
    /// Parabolic SAR (Stop and Reverse) Calculator
    /// Time Complexity: O(n) where n is the number of price data points
    /// Space Complexity: O(n) for output storage, O(1) for algorithm computation
    /// </summary>
    public class ParabolicSarCalculator : BaseCalculator
    {
        private const double MIN_ACCELERATION = 0.001;
        private const double MAX_ACCELERATION = 0.5;
        private const double DEFAULT_ACCELERATION = 0.02;

        private const double MIN_MAXIMUM = 0.01;
        private const double MAX_MAXIMUM = 1.0;
        private const double DEFAULT_MAXIMUM = 0.2;

        public ParabolicSarCalculator(Dictionary<string, string> para, string name) : base(name, para)
        {
            if (!parameters.ContainsKey(nameof(ParameterNamesEnum.Acceleration)) || !parameters.ContainsKey(nameof(ParameterNamesEnum.Maximum)))
            {
                throw new ArgumentException($"Parameters {nameof(ParameterNamesEnum.Acceleration)} and {nameof(ParameterNamesEnum.Maximum)} are required.");
            }

            ValidateParameters();
        }

        private void ValidateParameters()
        {
            if (!double.TryParse(parameters[nameof(ParameterNamesEnum.Acceleration)], out double acceleration) ||
                acceleration < MIN_ACCELERATION || acceleration > MAX_ACCELERATION)
            {
                throw new ArgumentException($"Acceleration must be between {MIN_ACCELERATION} and {MAX_ACCELERATION}.");
            }

            if (!double.TryParse(parameters[nameof(ParameterNamesEnum.Maximum)], out double maximum) ||
                maximum < MIN_MAXIMUM || maximum > MAX_MAXIMUM)
            {
                throw new ArgumentException($"Maximum must be between {MIN_MAXIMUM} and {MAX_MAXIMUM}.");
            }

            if (acceleration >= maximum)
            {
                throw new ArgumentException("Acceleration must be less than Maximum.");
            }
        }

        protected override CalculatorResults CalculateInternal(double[,] prices)
        {
            var acceleration = double.Parse(parameters[nameof(ParameterNamesEnum.Acceleration)]);
            var maximum = double.Parse(parameters[nameof(ParameterNamesEnum.Maximum)]);
            if (acceleration <= 0 || maximum <= 0)
            {
                throw new ArgumentException($"{nameof(ParameterNamesEnum.Acceleration)} must be greater than zero.");
            }
            if (prices.GetLength(0) < 2)
            {
                throw new ArgumentException("Price array is too short for the calculation.");
            }

            int outBegIdx, outNbElement;
            double[] sar = new double[High.Length];

            Core.Sar(High, Low, 0, High.Length - 1, sar, out outBegIdx, out outNbElement, acceleration, maximum);
            var results = new Dictionary<long, KeyValuePair<string, double>[]>();
            bool isUptrend = true;

            for (int i = outBegIdx; i < outBegIdx + outNbElement; i++)
            {
                long timestamp = (long)prices[i, 0];
                bool isReversal = false;

                if (isUptrend && prices[i, 3] < sar[i - outBegIdx])
                {
                    isUptrend = false;
                    isReversal = true;
                }
                else if (!isUptrend && prices[i, 3] > sar[i - outBegIdx])
                {
                    isUptrend = true;
                    isReversal = true;
                }

                results[timestamp] = new KeyValuePair<string, double>[]
                {
                    new KeyValuePair<string, double>(nameof(TechnicalNamesEnum.SAR), sar[i - outBegIdx]),
                    new KeyValuePair<string, double>(nameof(TechnicalNamesEnum.ISREVERSAL), isReversal ? 1.0 : 0.0)
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
                nameof(TechnicalNamesEnum.SAR),
                nameof(TechnicalNamesEnum.ISREVERSAL)};
        }

        public static Dictionary<string, (double Min, double Max, ParameterValueTypeEnum valueType)> GetParameterConstraints()
        {
            return new Dictionary<string, (double Min, double Max, ParameterValueTypeEnum valueType)>
            {
                { nameof(ParameterNamesEnum.Acceleration), (MIN_ACCELERATION, MAX_ACCELERATION, ParameterValueTypeEnum.DOUBLE) },
                { nameof(ParameterNamesEnum.Maximum), (MIN_MAXIMUM, MAX_MAXIMUM, ParameterValueTypeEnum.DOUBLE) }
            };
        }
    }
}

