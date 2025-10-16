using HC.TechnicalCalculators.Src.Calculators;
using HC.TechnicalCalculators.Src.Models;
using TALib;

namespace HC.TechnicalCalculators.Src.Calculators.Momentum
{
    public class DmCalculator : BaseCalculator
    {
        private const int MIN_PERIOD = 2;
        private const int MAX_PERIOD = 100;
        private const int DEFAULT_PERIOD = 14;

        public DmCalculator(Dictionary<string, string> para, string name) : base(name, para)
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

            var plusDM = CalculatePlusDM(High, Low, period);
            var minusDM = CalculateMinusDM(High, Low, period);
            var results = new Dictionary<long, KeyValuePair<string, double>[]>();

            for (int i = 0; i < plusDM.Length; i++)
            {
                long timestamp = (long)prices[i, 0];
                results[timestamp] = new KeyValuePair<string, double>[]
                {
                    new KeyValuePair<string, double>(nameof(TechnicalNamesEnum.PLUSDM), plusDM[i]),
                    new KeyValuePair<string, double>(nameof(TechnicalNamesEnum.MINUSDM), minusDM[i])
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
                nameof(TechnicalNamesEnum.PLUSDM), 
                nameof(TechnicalNamesEnum.MINUSDM) 
            };
        }
        public static Dictionary<string, (double Min, double Max, ParameterValueTypeEnum valueType)> GetParameterConstraints()
        {
            return new Dictionary<string, (double Min, double Max, ParameterValueTypeEnum valueType)>
            {
                { nameof(ParameterNamesEnum.Period), (MIN_PERIOD, MAX_PERIOD, ParameterValueTypeEnum.INT) }
            };
        }
        private double[] CalculatePlusDM(double[] high, double[] low, int period)
        {
            int outBegIdx, outNbElement;
            double[] plusDM = new double[high.Length];
            Core.PlusDM(high, low, 0, high.Length - 1, plusDM, out outBegIdx, out outNbElement, period);
            return plusDM;
        }

        private double[] CalculateMinusDM(double[] high, double[] low, int period)
        {
            int outBegIdx, outNbElement;
            double[] minusDM = new double[high.Length];
            Core.MinusDM(high, low, 0, high.Length - 1, minusDM, out outBegIdx, out outNbElement, period);
            return minusDM;
        }
    }
}
