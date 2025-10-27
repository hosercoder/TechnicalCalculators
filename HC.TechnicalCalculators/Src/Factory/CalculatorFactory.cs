using HC.TechnicalCalculators.Src.Calculators.Momentum;
using HC.TechnicalCalculators.Src.Calculators.News;
using HC.TechnicalCalculators.Src.Calculators.Overlap;
using HC.TechnicalCalculators.Src.Calculators.Price;
using HC.TechnicalCalculators.Src.Calculators.Volatility;
using HC.TechnicalCalculators.Src.Calculators.Volume;
using HC.TechnicalCalculators.Src.Interfaces;
using HC.TechnicalCalculators.Src.Models;
using HC.TechnicalCalculators.Src.Security;

namespace HC.TechnicalCalculators.Src.Factory
{
    /// <summary>
    /// Static Factory class to create instances of technical calculators based on the specified type and parameters.
    /// </summary>
    public static class CalculatorFactory
    {
        public static ITechnicalCalculator CreateCalculator(CalculatorNameEnum calculatorType, Dictionary<string, string> parameters,
            string name, IInputValidationService validationService)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Calculator name cannot be null or empty", nameof(name));

            // Validate parameters using the validation service
            foreach (var param in parameters)
            {
                validationService.ValidateStringLength(param.Key, 50, "parameter key");
                validationService.ValidateStringLength(param.Value, 100, "parameter value");
            }

            return GetCalculator(calculatorType, parameters, name);
        }

        public static IReadOnlyList<string> GetTechnicalIndicators(CalculatorNameEnum calculatorType)
        {
            return calculatorType switch
            {
                CalculatorNameEnum.SMA => SimpleMovingAverage.GetTechnicalIndicatorNames(),
                CalculatorNameEnum.EMA => ExponentialMovingAverage.GetTechnicalIndicatorNames(),
                CalculatorNameEnum.RSI => RsiCalculator.GetTechnicalIndicatorNames(),
                CalculatorNameEnum.ADX => AdxCalculator.GetTechnicalIndicatorNames(),
                CalculatorNameEnum.ADXR => AdxrCalculator.GetTechnicalIndicatorNames(),
                CalculatorNameEnum.APO => ApoCalculator.GetTechnicalIndicatorNames(),
                CalculatorNameEnum.AROON => AroonCalculator.GetTechnicalIndicatorNames(),
                CalculatorNameEnum.ATR => AtrCalculator.GetTechnicalIndicatorNames(),
                CalculatorNameEnum.AVGPRICE => AvgPriceCalculator.GetTechnicalIndicatorNames(),
                CalculatorNameEnum.BBANDS => BollingerBandsCalculator.GetTechnicalIndicatorNames(),
                CalculatorNameEnum.CHAIKINADLINE => ChaikinAdLineCalculator.GetTechnicalIndicatorNames(),
                CalculatorNameEnum.CHAIKINADOSCILLATOR => ChaikinAdOscillatorCalculator.GetTechnicalIndicatorNames(),
                CalculatorNameEnum.DEMA => DemaCalculator.GetTechnicalIndicatorNames(),
                CalculatorNameEnum.DM => DmCalculator.GetTechnicalIndicatorNames(),
                CalculatorNameEnum.DMI => DmiCalculator.GetTechnicalIndicatorNames(),
                CalculatorNameEnum.DX => DxCalculator.GetTechnicalIndicatorNames(),
                CalculatorNameEnum.KAMA => KaufmanAdaptiveMovingAverageCalculator.GetTechnicalIndicatorNames(),
                CalculatorNameEnum.MA => MovingAverageCalculator.GetTechnicalIndicatorNames(),
                CalculatorNameEnum.MACD => MacdCalculator.GetTechnicalIndicatorNames(),
                CalculatorNameEnum.MAMA => MesaAdaptiveMovingAverageCalculator.GetTechnicalIndicatorNames(),
                CalculatorNameEnum.MFI => MfiCalculator.GetTechnicalIndicatorNames(),
                CalculatorNameEnum.MOM => MomCalculator.GetTechnicalIndicatorNames(),
                CalculatorNameEnum.NATR => NatrCalculator.GetTechnicalIndicatorNames(),
                CalculatorNameEnum.OBV => ObvCalculator.GetTechnicalIndicatorNames(),
                CalculatorNameEnum.PPO => PpoCalculator.GetTechnicalIndicatorNames(),
                CalculatorNameEnum.PSAR => ParabolicSarCalculator.GetTechnicalIndicatorNames(),
                CalculatorNameEnum.ROC => RocCalculator.GetTechnicalIndicatorNames(),
                CalculatorNameEnum.STOCH => StochCalculator.GetTechnicalIndicatorNames(),
                CalculatorNameEnum.TEMA => TripleExponentialMovingAverageCalculator.GetTechnicalIndicatorNames(),
                CalculatorNameEnum.TMA => TriangularMovingAverageCalculator.GetTechnicalIndicatorNames(),
                CalculatorNameEnum.TRANGE => TrangeCalculator.GetTechnicalIndicatorNames(),
                CalculatorNameEnum.ULTOSC => UltOscCalculator.GetTechnicalIndicatorNames(),
                CalculatorNameEnum.WCLPRICE => WclPriceCalculator.GetTechnicalIndicatorNames(),
                CalculatorNameEnum.WMA => WeightedMovingAverage.GetTechnicalIndicatorNames(),
                CalculatorNameEnum.NEWSSENTIMENT => NewsSentimentCalculator.GetTechnicalIndicatorNames(),
                _ => throw new NotSupportedException($"Calculator '{calculatorType}' is not supported.")
            };
        }

        public static Dictionary<string, (double Min, double Max, ParameterValueTypeEnum valueType)> GetParameterConstraints(CalculatorNameEnum calculatorType)
        {
            return calculatorType switch
            {
                CalculatorNameEnum.SMA => SimpleMovingAverage.GetParameterConstraints(),
                CalculatorNameEnum.EMA => ExponentialMovingAverage.GetParameterConstraints(),
                CalculatorNameEnum.RSI => RsiCalculator.GetParameterConstraints(),
                CalculatorNameEnum.ADX => AdxCalculator.GetParameterConstraints(),
                CalculatorNameEnum.ADXR => AdxrCalculator.GetParameterConstraints(),
                CalculatorNameEnum.APO => ApoCalculator.GetParameterConstraints(),
                CalculatorNameEnum.AROON => AroonCalculator.GetParameterConstraints(),
                CalculatorNameEnum.ATR => AtrCalculator.GetParameterConstraints(),
                CalculatorNameEnum.AVGPRICE => AvgPriceCalculator.GetParameterConstraints(),
                CalculatorNameEnum.BBANDS => BollingerBandsCalculator.GetParameterConstraints(),
                CalculatorNameEnum.CHAIKINADLINE => ChaikinAdLineCalculator.GetParameterConstraints(),
                CalculatorNameEnum.CHAIKINADOSCILLATOR => ChaikinAdOscillatorCalculator.GetParameterConstraints(),
                CalculatorNameEnum.DEMA => DemaCalculator.GetParameterConstraints(),
                CalculatorNameEnum.DM => DmCalculator.GetParameterConstraints(),
                CalculatorNameEnum.DMI => DmiCalculator.GetParameterConstraints(),
                CalculatorNameEnum.DX => DxCalculator.GetParameterConstraints(),
                CalculatorNameEnum.KAMA => KaufmanAdaptiveMovingAverageCalculator.GetParameterConstraints(),
                CalculatorNameEnum.MA => MovingAverageCalculator.GetParameterConstraints(),
                CalculatorNameEnum.MACD => MacdCalculator.GetParameterConstraints(),
                CalculatorNameEnum.MAMA => MesaAdaptiveMovingAverageCalculator.GetParameterConstraints(),
                CalculatorNameEnum.MFI => MfiCalculator.GetParameterConstraints(),
                CalculatorNameEnum.MOM => MomCalculator.GetParameterConstraints(),
                CalculatorNameEnum.NATR => NatrCalculator.GetParameterConstraints(),
                CalculatorNameEnum.OBV => ObvCalculator.GetParameterConstraints(),
                CalculatorNameEnum.PPO => PpoCalculator.GetParameterConstraints(),
                CalculatorNameEnum.PSAR => ParabolicSarCalculator.GetParameterConstraints(),
                CalculatorNameEnum.ROC => RocCalculator.GetParameterConstraints(),
                CalculatorNameEnum.STOCH => StochCalculator.GetParameterConstraints(),
                CalculatorNameEnum.TEMA => TripleExponentialMovingAverageCalculator.GetParameterConstraints(),
                CalculatorNameEnum.TMA => TriangularMovingAverageCalculator.GetParameterConstraints(),
                CalculatorNameEnum.TRANGE => TrangeCalculator.GetParameterConstraints(),
                CalculatorNameEnum.ULTOSC => UltOscCalculator.GetParameterConstraints(),
                CalculatorNameEnum.WCLPRICE => WclPriceCalculator.GetParameterConstraints(),
                CalculatorNameEnum.WMA => WeightedMovingAverage.GetParameterConstraints(),
                CalculatorNameEnum.NEWSSENTIMENT => NewsSentimentCalculator.GetParameterConstraints(),
                _ => throw new NotSupportedException($"Calculator '{calculatorType}' is not supported.")
            };
        }

        public static ITechnicalCalculator GetCalculator(CalculatorNameEnum CalculatorNameEnum, Dictionary<string, string> parameters, string name)
        {
            return CalculatorNameEnum switch
            {
                CalculatorNameEnum.SMA => new SimpleMovingAverage(parameters, name),
                CalculatorNameEnum.EMA => new ExponentialMovingAverage(parameters, name),
                CalculatorNameEnum.RSI => new RsiCalculator(parameters, name),
                CalculatorNameEnum.ADX => new AdxCalculator(parameters, name),
                CalculatorNameEnum.ADXR => new AdxrCalculator(parameters, name),
                CalculatorNameEnum.APO => new ApoCalculator(parameters, name),
                CalculatorNameEnum.AROON => new AroonCalculator(parameters, name),
                CalculatorNameEnum.ATR => new AtrCalculator(parameters, name),
                CalculatorNameEnum.AVGPRICE => new AvgPriceCalculator(parameters, name),
                CalculatorNameEnum.BBANDS => new BollingerBandsCalculator(parameters, name),
                CalculatorNameEnum.CHAIKINADLINE => new ChaikinAdLineCalculator(parameters, name),
                CalculatorNameEnum.CHAIKINADOSCILLATOR => new ChaikinAdOscillatorCalculator(parameters, name),
                CalculatorNameEnum.DEMA => new DemaCalculator(parameters, name),
                CalculatorNameEnum.DM => new DmCalculator(parameters, name),
                CalculatorNameEnum.DMI => new DmiCalculator(parameters, name),
                CalculatorNameEnum.DX => new DxCalculator(parameters, name),
                CalculatorNameEnum.KAMA => new KaufmanAdaptiveMovingAverageCalculator(parameters, name),
                CalculatorNameEnum.MA => new MovingAverageCalculator(parameters, name),
                CalculatorNameEnum.MACD => new MacdCalculator(parameters, name),
                CalculatorNameEnum.MAMA => new MesaAdaptiveMovingAverageCalculator(parameters, name),
                CalculatorNameEnum.MFI => new MfiCalculator(parameters, name),
                CalculatorNameEnum.MOM => new MomCalculator(parameters, name),
                CalculatorNameEnum.NATR => new NatrCalculator(parameters, name),
                CalculatorNameEnum.OBV => new ObvCalculator(parameters, name),
                CalculatorNameEnum.PPO => new PpoCalculator(parameters, name),
                CalculatorNameEnum.PSAR => new ParabolicSarCalculator(parameters, name),
                CalculatorNameEnum.ROC => new RocCalculator(parameters, name),
                CalculatorNameEnum.STOCH => new StochCalculator(parameters, name),
                CalculatorNameEnum.TEMA => new TripleExponentialMovingAverageCalculator(parameters, name),
                CalculatorNameEnum.TMA => new TriangularMovingAverageCalculator(parameters, name),
                CalculatorNameEnum.TRANGE => new TrangeCalculator(parameters, name),
                CalculatorNameEnum.ULTOSC => new UltOscCalculator(parameters, name),
                CalculatorNameEnum.WCLPRICE => new WclPriceCalculator(parameters, name),
                CalculatorNameEnum.WMA => new WeightedMovingAverage(parameters, name),
            };
        }
    }
}
