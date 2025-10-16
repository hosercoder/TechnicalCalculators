using HC.TechnicalCalculators.Src.Models;

namespace HC.TechnicalCalculators.Src.Interfaces
{
    public interface ITechnicalCalculator
    {
        CalculatorResults Calculate(double[,] prices, bool skipValidation = false);
    }
}
