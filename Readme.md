# SMT Technical Calculators
## Summary
The SMT Technical Calculators library provides a set of calculators for performing various technical analysis calculations on financial data. The library is designed to be extensible, allowing new calculators to be added easily.

Library uses Core.TALibrary for technical analysis calculations.

## Creating a New Calculator from the BaseCalculator Class

To create a new calculator by extending the `BaseCalculator` class, follow these steps:

1. **Create a New Calculator Class**:
   Create a new class that inherits from `BaseCalculator`. Implement the required methods and properties.

2. **Register the New Calculator**:
   The new calculator will be automatically registered with the `CalculatorFactory` when the class is instantiated, thanks to the `BaseCalculator` constructor.

3. **Implement the Calculation Logic**:
   Override the `CalculateInternal` method to implement the specific calculation logic for your new calculator. This method should return a `CalculatorResults` object containing the calculation results.

4. **Add Unit Tests**:
   Create unit tests to verify the functionality of your new calculator. Use the existing test classes as a reference.

## Calculator Factory
The `TechnicalCalculatorFactory` class is a static factory class responsible for registering and retrieving instances of technical calculators. It provides a centralized mechanism to manage different types of calculators by their names.
## List of Calculators
### Momentum Calculator
1. **UltOscCalculator**:
   Calculates the Ultimate Oscillator for a given set of price data.
2. **StochCalculator**:
   Calculates the Stochastic Oscillator for a given set of price data.
3. **RsiCalculator**:
   Calculates the Relative Strength Index (RSI) for a given set of price data.
4. **RocCalculator**:
   Calculates the Rate of Change (ROC) for a given set of price data.
5. **PpoCalculator**:
   Calculates the Percentage Price Oscillator (PPO) for a given set of price data.
6. **MomCalculator**:
   Calculates the Momentum for a given set of price data.
7. **MfiCalculator**:
   Calculates the Money Flow Index (MFI) for a given set of price data.
8. **MacdCalculator**:
   Calculates the Moving Average Convergence Divergence (MACD) for a given set of price data.
9. **DxCalculator**:
   Calculates the Directional Movement Index (DX) for a given set of price data.
10. **DmiCalculator**:
   Calculates the Directional Movement Indicator (DMI) for a given set of price data.
11. **DmCalculator**:
   Calculates the Directional Movement (DM) for a given set of price data.
12. **AroonCalculator**:
   Calculates the Aroon Indicator for a given set of price data.
13. **ApoCalculator**:
   Calculates the Absolute Price Oscillator (APO) for a given set of price data.
14. **AdxrCalculator**:
   Calculates the Average Directional Movement Index Rating (ADXR) for a given set of price data.
15. **AdxCalculator**:
   Calculates the Average Directional Movement Index (ADX) for a given set of price data.
### Overlap Calculator
1. **WeightedMovingAverage**:
   Calculates the Weighted Moving Average for a given set of price data.
2. **TripleExponentialMovingAverageCalculator**:
   Calculates the Triple Exponential Moving Average (TEMA) for a given set of price data.
3. **TriangularMovingAverageCalculator**:
   Calculates the Triangular Moving Average for a given set of price data.
3. **SimpleMovingAverageCalculator**:
   Calculates the Simple Moving Average for a given set of price data.
4. **ParabolicSarCalculator**:
   Calculates the Parabolic SAR for a given set of price data.
5. **MovingAverageCalculator**:
   Calculates the Moving Average for a given set of price data.
6. **MesaAdaptiveMovingAverageCalculator**:
   Calculates the Mesa Adaptive Moving Average (MAMA) for a given set of price data.
7. **KaufmanAdaptiveMovingAverageCalculator**:
   Calculates the Kaufman Adaptive Moving Average (KAMA) for a given set of price data.
8. **ExponentialMovingAverageCalculator**:
   Calculates the Exponential Moving Average for a given set of price data.
9. **DemaCalculator**:
   Calculates the Double Exponential Moving Average (DEMA) for a given set of price data.
10. **BollingerBandsCalculator**:
	Calculates the Bollinger Bands for a given set of price data.
### Price Calculator
1. **AvgPriceCalculator**:
   Calculates the Average Price for a given set of price data.
2. **WclPriceCalculator**:
   Calculates the Weighted Close Price for a given set of price data.
### Statistics Calculator
1. **BetaCalculator**:
   Calculates the Beta for a given set of price data.
### Volatility Calculator
1. **ATRCalculator**:
   Calculates the Average True Range (ATR) for a given set of price data.
2. **NATRCalculator**:
   Calculates the Normalized Average True Range (NATR) for a given set of price data.
3. **TrangeCalculator**:
   Calculates the True Range (TR) for a given set of price data.
### Volume Calculator
1. **ChaikinAdLineCalculator**:
   Calculates the Chaikin Accumulation/Distribution Line for a given set of price data.
2. **ChaikinOscillatorCalculator**:
   Calculates the Chaikin Oscillator for a given set of price data.
3. **ObvCalculator**:
   Calculates the On-Balance Volume (OBV) for a given set of price data.
	