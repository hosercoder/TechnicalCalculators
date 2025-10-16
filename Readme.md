# HC Technical Calculators

## Purpose
The HC Technical Calculators library provides a comprehensive set of technical analysis calculators for financial data analysis. The library is designed to be extensible and modular, allowing new calculators to be added easily while maintaining a consistent interface.

This package is part of a NuGet suite designed to be used as components of a larger financial analysis product.

## Repository
🔗 **GitHub**: [https://github.com/hosercoder/TechnicalCalculators](https://github.com/hosercoder/TechnicalCalculators)

## Summary
The HC Technical Calculators library offers a wide range of calculators for performing various technical analysis calculations on financial market data. Built on top of the TALib.NETCore library, it provides a modern .NET 8 wrapper with enhanced features including:

- Dependency injection support
- Comprehensive parameter validation with default values
- Extensible factory pattern for calculator creation
- Consistent result formatting
- Built-in security features for data handling

## Architecture

### Factory Pattern
The library uses a factory pattern through the `ITechnicalCalculatorFactory` interface to create calculator instances. This allows for:
- Centralized calculator creation
- Parameter validation and constraint checking
- Discovery of available calculators and their capabilities

### Base Calculator Class
All calculators inherit from the `BaseCalculator` abstract class, which provides:
- Common price data parsing (Open, High, Low, Close, Volume)
- Parameter validation framework
- Consistent result formatting
- Error handling and logging support

## Creating a New Calculator

To create a new calculator by extending the `BaseCalculator` class, follow these steps:

1. **Create a New Calculator Class**:
   Create a new class that inherits from `BaseCalculator`. Implement the required methods and properties.
```csharp
public class MyCustomCalculator : BaseCalculator
{
    private const int MIN_PERIOD = 2;
    private const int MAX_PERIOD = 100;
    private const int DEFAULT_PERIOD = 14;

    public MyCustomCalculator(Dictionary<string, string> parameters, string name) 
        : base(name, parameters)
    {
        ValidateParameters();
    }

    private void ValidateParameters()
    {
        // Set default values and validate parameters
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
        // Implement your calculation logic here
        // Return CalculatorResults object
    }
}
```

2. **Register with Factory**:
   Add the new calculator to the `CalculatorFactory` and update the `CalculatorNameEnum`.

3. **Add Static Methods**:
   Implement static methods for metadata:
```csharp
public static IReadOnlyList<string> GetTechnicalIndicatorNames()
{
    return new string[] { "MyIndicator" };
}

public static Dictionary<string, (double Min, double Max, double Default)> GetParameterConstraints()
{
    return new Dictionary<string, (double Min, double Max, double Default)>
    {
        { nameof(ParameterNamesEnum.Period), (MIN_PERIOD, MAX_PERIOD, DEFAULT_PERIOD) }
    };
}
```

4. **Add Unit Tests**:
   Create comprehensive unit tests following the existing test patterns.

## Available Calculators

### Momentum Calculators
- **UltOscCalculator**: Ultimate Oscillator - measures momentum using multiple timeframes
- **StochCalculator**: Stochastic Oscillator - compares closing price to price range over time
- **RsiCalculator**: Relative Strength Index - measures overbought/oversold conditions
- **RocCalculator**: Rate of Change - measures percentage change over time
- **PpoCalculator**: Percentage Price Oscillator - shows relationship between two moving averages
- **MomCalculator**: Momentum - measures rate of price change
- **MfiCalculator**: Money Flow Index - volume-weighted RSI
- **MacdCalculator**: Moving Average Convergence Divergence - trend following momentum indicator
- **DxCalculator**: Directional Movement Index - measures trend strength
- **DmiCalculator**: Directional Movement Indicator - shows trend direction
- **DmCalculator**: Directional Movement - raw directional movement values
- **AroonCalculator**: Aroon Indicator - identifies trend changes and strength
- **ApoCalculator**: Absolute Price Oscillator - difference between two moving averages
- **AdxrCalculator**: Average Directional Movement Index Rating - smoothed ADX
- **AdxCalculator**: Average Directional Movement Index - measures trend strength

### Overlap Studies (Moving Averages & Price)
- **WeightedMovingAverage**: Weighted Moving Average - gives more weight to recent prices
- **TripleExponentialMovingAverageCalculator**: TEMA - reduces lag of traditional moving averages
- **TriangularMovingAverageCalculator**: Triangular Moving Average - double-smoothed moving average
- **SimpleMovingAverage**: Simple Moving Average - arithmetic mean of prices
- **ParabolicSarCalculator**: Parabolic SAR - stop and reverse system
- **MovingAverageCalculator**: Generic Moving Average calculator
- **MesaAdaptiveMovingAverageCalculator**: MAMA - adaptive moving average system
- **KaufmanAdaptiveMovingAverageCalculator**: KAMA - efficiency ratio based adaptive MA
- **ExponentialMovingAverage**: Exponential Moving Average - gives more weight to recent data
- **DemaCalculator**: Double Exponential Moving Average - faster EMA variant
- **BollingerBandsCalculator**: Bollinger Bands - volatility bands around moving average

### Price Transform
- **AvgPriceCalculator**: Average Price - (High + Low + Close) / 3
- **WclPriceCalculator**: Weighted Close Price - (High + Low + 2*Close) / 4

### Statistical Functions
- **BetaCalculator**: Beta - measures correlation with market movements

### Volatility Indicators
- **ATRCalculator**: Average True Range - measures market volatility
- **NATRCalculator**: Normalized Average True Range - ATR as percentage
- **TrangeCalculator**: True Range - measures daily volatility

### Volume Indicators
- **ChaikinAdLineCalculator**: Chaikin Accumulation/Distribution Line - volume flow indicator
- **ChaikinOscillatorCalculator**: Chaikin Oscillator - momentum of A/D line
- **ObvCalculator**: On-Balance Volume - relates volume to price change

## Installation

### NuGet Package
```bash
dotnet add package TechnicalCalculators
```

### From Source
```bash
git https://github.com/hosercoder/TechnicalCalculators.git
cd TechnicalCalculators
dotnet build
```

## Usage Example

```csharp
// Using dependency injection
services.AddTechnicalCalculators();

// Create calculator through factory
var factory = serviceProvider.GetRequiredService<ITechnicalCalculatorFactory>();

var parameters = new Dictionary<string, string>
{
    { "Period", "14" }
};

var rsiCalculator = factory.CreateCalculator(
    CalculatorNameEnum.RSI, 
    parameters, 
    "RSI_14"
);

// Calculate results
double[,] priceData = GetPriceData(); // Your price data
var results = rsiCalculator.Calculate(priceData);

// Access results
foreach (var result in results.Results)
{
    long timestamp = result.Key;
    var indicators = result.Value;
    
    foreach (var indicator in indicators)
    {
        Console.WriteLine($"{timestamp}: {indicator.Key} = {indicator.Value}");
    }
}
```

## Contributing

We welcome contributions! Please see our [Contributing Guidelines](CONTRIBUTING.md) for details.

### Development Setup
1. Clone the repository
2. Ensure you have .NET 8 SDK installed
3. Run `dotnet restore` to restore dependencies
4. Run `dotnet build` to build the solution
5. Run `dotnet test` to execute tests

## Dependencies
- **.NET 8.0**: Target framework
- **TALib.NETCore**: Core technical analysis calculations
- **Microsoft.Extensions.DependencyInjection**: Dependency injection support
- **Microsoft.Extensions.Options**: Configuration options pattern
- **Microsoft.Extensions.Http**: HTTP client factory support

## License
This library is part of the HC financial analysis suite.

## Support

For issues and questions:
- 🐛 **Bug Reports**: [GitHub Issues](https://github.com/hosercoder/TechnicalCalculators/issues)
- 📚 **Documentation**: [Wiki](https://github.com/hosercoder/TechnicalCalculators/wiki)