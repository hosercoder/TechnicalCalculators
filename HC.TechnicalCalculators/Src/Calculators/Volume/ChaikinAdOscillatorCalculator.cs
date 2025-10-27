using HC.TechnicalCalculators.Src.Models;
using TALib;

namespace HC.TechnicalCalculators.Src.Calculators.Volume
{
    public class ChaikinAdOscillatorCalculator : BaseCalculator
    {
        public ChaikinAdOscillatorCalculator(Dictionary<string, string> para, string name) : base(name, para)
        {

        }
        protected override CalculatorResults CalculateInternal(double[,] prices)
        {
            var adLine = CalculateChaikinAdLine(High, Low, Close, Volume);
            var chaikinOscillator = CalculateChaikinAdOscillator(adLine);
            var results = new Dictionary<long, KeyValuePair<string, double>[]>();

            for (int i = 0; i < chaikinOscillator.Length; i++)
            {
                long timestamp = (long)prices[i, 0];
                results[timestamp] = new KeyValuePair<string, double>[]
                {
                    new KeyValuePair<string, double>(nameof(TechnicalNamesEnum.CHAIKINADOSCILLATOR), chaikinOscillator[i])
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
            return new string[] { nameof(TechnicalNamesEnum.CHAIKINADOSCILLATOR) };
        }
        public static Dictionary<string, (double Min, double Max, ParameterValueTypeEnum valueType)> GetParameterConstraints()
        {
            return new Dictionary<string, (double Min, double Max, ParameterValueTypeEnum valueType)>
            {
                { nameof(ParameterNamesEnum.NA), (0, 0, ParameterValueTypeEnum.INT) }
            };
        }
        private double[] CalculateChaikinAdLine(double[] high, double[] low, double[] close, double[] volume)
        {
            int outBegIdx, outNbElement;
            double[] adLine = new double[high.Length];
            var retCode = Core.Ad(high, low, close, volume, 0, high.Length - 1, adLine, out outBegIdx, out outNbElement);
            ValidateTALibResult(retCode, nameof(CalculatorNameEnum.CHAIKINADLINE));
            return adLine;
        }

        private double[] CalculateChaikinAdOscillator(double[] adLine)
        {
            int outBegIdx, outNbElement;
            double[] ema3 = new double[adLine.Length];
            double[] ema10 = new double[adLine.Length];
            var retCode1 = Core.Ema(adLine, 0, adLine.Length - 1, ema3, out outBegIdx, out outNbElement, 3);
            ValidateTALibResult(retCode1, nameof(CalculatorNameEnum.EMA));
            var retCode2 = Core.Ema(adLine, 0, adLine.Length - 1, ema10, out outBegIdx, out outNbElement, 10);
            ValidateTALibResult(retCode2, nameof(CalculatorNameEnum.EMA));

            double[] chaikinOscillator = new double[adLine.Length];
            for (int i = 0; i < adLine.Length; i++)
            {
                chaikinOscillator[i] = ema3[i] - ema10[i];
            }

            return chaikinOscillator;
        }
    }
}

