using HC.TechnicalCalculators.Src.Interfaces;
using HC.TechnicalCalculators.Src.Models;
using HC.TechnicalCalculators.Src.Security;
using TALib;

namespace HC.TechnicalCalculators.Src.Calculators
{
    /// <summary>
    /// Abstract base class for technical calculators.
    /// Provides common functionality for price data processing and validation.
    /// Time Complexity: O(n) for data processing where n is the number of price data points
    /// Space Complexity: O(n) for storing price arrays and output results
    /// </summary>
    public abstract class BaseCalculator : ITechnicalCalculator
    {
        protected string _name;
        protected Dictionary<string, string> parameters;
        protected readonly IInputValidationService ValidationService;

        private const int MAX_ARRAY_SIZE = 1000000; // 1M elements
        private const int MAX_PARAMETER_LENGTH = 100;

        /// <summary>
        /// High prices array.
        /// </summary>
        public double[] High = Array.Empty<double>();

        /// <summary>
        /// Low prices array.
        /// </summary>
        public double[] Low = Array.Empty<double>();

        /// <summary>
        /// Close prices array.
        /// </summary>
        public double[] Close = Array.Empty<double>();

        /// <summary>
        /// Volume array.
        /// </summary>
        public double[] Volume = Array.Empty<double>();

        /// <summary>
        /// Open prices array.
        /// </summary>
        public double[] Open = Array.Empty<double>();

        /// <summary>
        /// Timestamp array.
        /// </summary>
        public double[] Timestamp = Array.Empty<double>();

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseCalculator"/> class.
        /// Registers the calculator with the specified name.
        /// </summary>
        /// <param name="name">The name of the calculator.</param>
        /// <param name="para">Calculator parameters</param>
        /// <param name="validationService">Service for input validation</param>
        protected BaseCalculator(string name, Dictionary<string, string> para, IInputValidationService validationService)
        {
            ValidationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
            parameters = para ?? throw new ArgumentNullException(nameof(para), "Parameters dictionary cannot be null.");

            // Validate calculator name
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Calculator name cannot be null or empty.", nameof(name));
            }
            ValidationService.ValidateStringLength(name, MAX_PARAMETER_LENGTH, nameof(name));

            foreach (var kvp in parameters)
            {
                if (kvp.Key == nameof(ParameterNamesEnum.NA))
                {
                    continue;
                }

                // Enhanced parameter validation
                if (string.IsNullOrEmpty(kvp.Key))
                {
                    throw new ArgumentException("Parameter key cannot be null or empty.");
                }

                // Validate parameter values are not null or empty
                if (string.IsNullOrEmpty(kvp.Value))
                {
                    throw new ArgumentException($"Parameter '{kvp.Key}' cannot be null or empty.", kvp.Key);
                }

                // Validate parameter lengths to prevent buffer overflow attacks
                ValidationService.ValidateStringLength(kvp.Key, MAX_PARAMETER_LENGTH, "parameter key");
                ValidationService.ValidateStringLength(kvp.Value, MAX_PARAMETER_LENGTH, "parameter value");
            }

            _name = name;
        }

        /// <summary>
        /// Legacy constructor for backward compatibility during migration
        /// Uses a default validation service
        /// </summary>
        /// <param name="name">The name of the calculator.</param>
        /// <param name="para">Calculator parameters</param>
        protected BaseCalculator(string name, Dictionary<string, string> para)
            : this(name, para, CreateDefaultValidationService())
        {
        }

        private static IInputValidationService CreateDefaultValidationService()
        {
            return new InputValidationService(
                Microsoft.Extensions.Logging.Abstractions.NullLogger<InputValidationService>.Instance);
        }

        /// <summary>
        /// Calculates the results based on the provided prices and parameters.
        /// </summary>
        /// <param name="prices">The prices array.</param>
        /// <param name="parameters">The parameters dictionary.</param>
        /// <returns>The calculation results.</returns>
        public CalculatorResults Calculate(double[,] prices, bool skipValidation = false)
        {
            if (!skipValidation)
            {
                // Check for null specifically to maintain backward compatibility with existing tests
                if (prices == null)
                {
                    throw new ArgumentNullException(nameof(prices), "Prices array cannot be null.");
                }

                // Check columns before general validation for more specific error messages
                if (prices.GetLength(1) != 6)
                {
                    throw new ArgumentException("Prices array must have 6 columns: timestamp, open, high, low, close, volume.");
                }

                // Enhanced security validation
                if (!ValidationService.IsValidPriceData(prices))
                {
                    throw new ArgumentException("Invalid price data provided.");
                }

                ValidationService.ValidateArraySize(prices, MAX_ARRAY_SIZE);
            }
            else
            {
                // Basic null check even when skipping validation
                if (prices == null)
                {
                    throw new ArgumentNullException(nameof(prices), "Prices array cannot be null.");
                }
            }

            ParsePrices(prices);
            return CalculateInternal(prices);
        }

        /// <summary>
        /// Calculates the results based on the provided prices and parameters.
        /// Must be implemented by derived classes.
        /// </summary>
        /// <param name="prices">The prices array must have 6 columns: start, open, high, low, close, volume..</param>
        /// <param name="parameters">The parameters dictionary.</param>
        /// <returns>The calculation results.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the prices array or parameters dictionary is null.</exception>
        /// <exception cref="ArgumentException">Thrown when the prices array does not have 6 columns or the period is invalid.</exception>
        protected abstract CalculatorResults CalculateInternal(double[,] prices);

        private void ParsePrices(double[,] prices)
        {
            int length = prices.GetLength(0);

            // Initialize arrays with proper size
            High = new double[length];
            Low = new double[length];
            Close = new double[length];
            Volume = new double[length];
            Open = new double[length];
            Timestamp = new double[length];

            // Securely copy data to prevent external modification
            for (int i = 0; i < length; i++)
            {
                Timestamp[i] = prices[i, 0];
                Open[i] = prices[i, 1];
                High[i] = prices[i, 2];
                Low[i] = prices[i, 3];
                Close[i] = prices[i, 4];
                Volume[i] = prices[i, 5];
            }
        }

        protected void ValidatePeriod(Dictionary<string, string> parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters), "Parameters dictionary cannot be null.");
            }
            foreach (var kvp in parameters)
            {
                if (parameters.ContainsKey("period"))
                {
                    var period = int.Parse(parameters["period"]);
                    if (period <= 0)
                    {
                        throw new ArgumentException("Period must be greater than zero.");
                    }
                    if (High.Length < period)
                    {
                        throw new ArgumentException("Price array is too short for the given period.");
                    }
                }
            }
        }

        /// <summary>
        /// Validates the TALib RetCode and throws an exception if the calculation failed.
        /// This method provides robust error handling for all TALib function calls.
        /// </summary>
        /// <param name="retCode">The return code from TALib function</param>
        /// <param name="functionName">The name of the TALib function that was called</param>
        /// <exception cref="InvalidOperationException">Thrown when TALib calculation fails</exception>
        protected static void ValidateTALibResult(Core.RetCode retCode, string functionName)
        {
            if (retCode != Core.RetCode.Success)
            {
                throw new InvalidOperationException($"TALib {functionName} calculation failed with return code: {retCode}. " +
                    $"This may indicate insufficient data, invalid parameters, or internal calculation error.");
            }
        }
    }
}
