using HC.TechnicalCalculators.Src.Interfaces;
using HC.TechnicalCalculators.Src.Models;
using HC.TechnicalCalculators.Src.Security;

namespace HC.TechnicalCalculators.Src.Calculators.News
{
    /// <summary>
    /// News Sentiment Calculator
    /// Time Complexity: O(n + m) where n is the number of price data points and m is the number of news items
    /// Space Complexity: O(n + m) for storing price data and news sentiment results
    /// </summary>
    public class NewsSentimentCalculator : BaseCalculator
    {
        private readonly INewsFeedService _newsFeedService;
        private string? _symbol;
        private int _lookbackPeriod = 24; // Default hours to look back

        public NewsSentimentCalculator(INewsFeedService newsFeedService, Dictionary<string, string> para, string name, IInputValidationService validationService)
            : base(name, para, validationService)
        {
            _newsFeedService = newsFeedService ?? throw new ArgumentNullException(nameof(newsFeedService));
            _symbol = parameters.ContainsKey(nameof(ParameterNamesEnum.Symbol)) ? parameters[nameof(ParameterNamesEnum.Symbol)] : null;
        }

        protected override CalculatorResults CalculateInternal(double[,] data)
        {
            if (string.IsNullOrEmpty(_symbol))
                throw new InvalidOperationException("Symbol parameter is required");

            var results = new Dictionary<long, KeyValuePair<string, double>[]>();
            // Get timestamp for the current candle
            for (int i = 0; i < data.Length; i++)
            {
                long ts = (long)data[i, 0];
                DateTime candleTime = DateTimeOffset.FromUnixTimeSeconds(ts).UtcDateTime;
                var r = _newsFeedService.GetSentimentScore(_symbol, candleTime.AddHours(-_lookbackPeriod), candleTime);
                results[ts] = new KeyValuePair<string, double>[]
                {

                    new KeyValuePair<string, double>(nameof(TechnicalNamesEnum.NEWSSENTIMENT), r)
                };
            }
            // Calculate the sentiment score for the given symbol and time range


            CalculatorResults calculatorResults = new CalculatorResults
            {
                Name = _name,
                Results = results.OrderByDescending(x => x.Key).ToDictionary(x => x.Key, x => x.Value)
            };

            return calculatorResults;
        }

        public static IReadOnlyList<string> GetTechnicalIndicatorNames()
        {
            return new string[] { nameof(TechnicalNamesEnum.NEWSSENTIMENT) };
        }
        public static IReadOnlyList<string> GetRequiredParamNames()
        {
            return new string[] { nameof(ParameterNamesEnum.Symbol) };
        }

        public static Dictionary<string, (double Min, double Max, ParameterValueTypeEnum valueType)> GetParameterConstraints()
        {
            return new Dictionary<string, (double Min, double Max, ParameterValueTypeEnum valueType)>
            {
                // No numeric parameter constraints for news sentiment calculator
                // Symbol is a string parameter
            };
        }
    }
}

