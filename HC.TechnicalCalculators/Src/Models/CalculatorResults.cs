namespace HC.TechnicalCalculators.Src.Models
{
    public class CalculatorResults
    {
        public required string Name { get; set; }
        public required Dictionary<long, KeyValuePair<string, double>[]> Results { get; set; }
    }
}
