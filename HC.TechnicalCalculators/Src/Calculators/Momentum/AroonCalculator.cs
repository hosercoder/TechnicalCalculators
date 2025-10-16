using HC.TechnicalCalculators.Src.Calculators;
using HC.TechnicalCalculators.Src.Models;
using TALib;

namespace HC.TechnicalCalculators.Src.Calculators.Momentum
{
    public class AroonCalculator : BaseCalculator
    {
        private const int MIN_PERIOD = 2;
        private const int MAX_PERIOD = 100;
        private const int DEFAULT_PERIOD = 14;

        public AroonCalculator(Dictionary<string, string> para, string name) : base(name, para)
        {
            ValidateParameters();
        }

        private void ValidateParameters()
        {
            if (!parameters.ContainsKey(nameof(ParameterNamesEnum.Period)))
            {
                parameters[nameof(ParameterNamesEnum.Period)] = DEFAULT_PERIOD.ToString();
            }

            if (!int.TryParse(parameters[nameof(ParameterNamesEnum.Period)], out int period) ||
                period < MIN_PERIOD || period > MAX_PERIOD)
            {
                throw new ArgumentException($"Period must be between {MIN_PERIOD} and {MAX_PERIOD}.");
            }
        }
        protected override CalculatorResults CalculateInternal(double[,] prices)
        {

            var period = int.Parse(parameters[nameof(ParameterNamesEnum.Period)]);

            var length = High.Length;

            var aroonUp = new double[length];
            var aroonDown = new double[length];
            CalculateAROON(High, Low, period, out aroonUp, out aroonDown);

            var results = new Dictionary<long, KeyValuePair<string, double>[]>();

            for (int i = 0; i < aroonUp.Length; i++)
            {
                long timestamp = (long)prices[i, 0];
                results[timestamp] = new KeyValuePair<string, double>[]
                {
                    new KeyValuePair<string, double>(nameof(TechnicalNamesEnum.AROONUP), aroonUp[i]),
                    new KeyValuePair<string, double>(nameof(TechnicalNamesEnum.AROONDOWN), aroonDown[i])
                };
            }

            return new CalculatorResults
            {
                Name = _name,
                Results = results.OrderByDescending(x => x.Key).ToDictionary(x => x.Key, x => x.Value)
            };
        }

        private void CalculateAROON(double[] high, double[] low, int period, out double[] aroonUp, out double[] aroonDown)
        {
            int outBegIdx, outNbElement;
            var aroonUpt = new double[high.Length];
            var aroonDownt = new double[low.Length];
            var retCode = Core.Aroon(high, low, 0, high.Length - 1, aroonUpt, aroonDownt, out outBegIdx, out outNbElement, period);
            ValidateTALibResult(retCode, nameof(CalculatorNameEnum.AROON));

            aroonUp = aroonUpt;
            aroonDown = aroonDownt;
        }
        public static IReadOnlyList<string> GetTechnicalIndicatorNames()
        {
            return new string[] { 
                nameof(TechnicalNamesEnum.AROONUP), 
                nameof(TechnicalNamesEnum.AROONDOWN) 
            };
        }
        public static Dictionary<string, (double Min, double Max, ParameterValueTypeEnum valueType)> GetParameterConstraints()
        {
            return new Dictionary<string, (double Min, double Max, ParameterValueTypeEnum valueType)>
            {
                { nameof(ParameterNamesEnum.Period), (MIN_PERIOD, MAX_PERIOD, ParameterValueTypeEnum.INT) }
            };
        }
    }
}

