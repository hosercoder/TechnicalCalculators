using HC.TechnicalCalculators.Src.Models;

namespace HC.TechnicalCalculators.Src.Calculators.Overlap
{
    public class TriangularMovingAverageCalculator : BaseCalculator
    {
        private const int MIN_PERIOD = 2;
        private const int MAX_PERIOD = 200;
        private const int DEFAULT_PERIOD = 20;

        public TriangularMovingAverageCalculator(Dictionary<string, string> para, string name) : base(name, para)
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

            var tma = CalculateTMA(Close, period);
            var results = new Dictionary<long, KeyValuePair<string, double>[]>();

            for (int i = 0; i < tma.Length; i++)
            {
                long timestamp = (long)prices[i, 0];
                results[timestamp] = new KeyValuePair<string, double>[]
                {
                    new KeyValuePair<string, double>(nameof(TechnicalNamesEnum.MOVINGAVERAGE), tma[i])
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

        private double[] CalculateTMA(double[] prices, int period)
        {
            int length = prices.Length;
            double[] sma1 = new double[length];
            double[] sma2 = new double[length];
            double[] tma = new double[length];

            // Calculate the first Simple Moving Average (SMA)
            for (int i = 0; i < length; i++)
            {
                if (i < period - 1)
                {
                    sma1[i] = double.NaN;
                }
                else
                {
                    double sum = 0;
                    for (int j = 0; j < period; j++)
                    {
                        sum += prices[i - j];
                    }
                    sma1[i] = sum / period;
                }
            }

            // Calculate the second SMA on the first SMA
            for (int i = 0; i < length; i++)
            {
                if (i < period - 1)
                {
                    sma2[i] = double.NaN;
                }
                else
                {
                    double sum = 0;
                    int validCount = 0;
                    for (int j = 0; j < period; j++)
                    {
                        if (!double.IsNaN(sma1[i - j]))
                        {
                            sum += sma1[i - j];
                            validCount++;
                        }
                    }
                    sma2[i] = validCount > 0 ? sum / validCount : double.NaN;
                }
            }

            // Calculate the TMA
            for (int i = 0; i < length; i++)
            {
                tma[i] = sma2[i];
            }

            return tma;
        }
    }
}

