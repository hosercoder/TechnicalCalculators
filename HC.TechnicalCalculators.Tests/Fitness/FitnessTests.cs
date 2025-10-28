using FitnessTests.CodeAnalysis;
using FitnessTests.Library.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace HC.TechnicalCalculators.Tests.Fitness
{
    public class FitnessTests
    {

        [Fact]
        public void RunFitnessTestsOnAllCodeFiles()
        {
            // Arrange
            var projectRoot = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;
            var libraryPath = Path.Combine(projectRoot, "HC.TechnicalCalculators");

            var sourceFiles = Directory.GetFiles(libraryPath, "*.cs", SearchOption.AllDirectories);

            var results = new List<FitnessResult>();
            var fitnessAnalyzer = new FitnessAnalyzer();
            Console.WriteLine($"Solution Directory: {projectRoot}");
            Console.WriteLine($"Found {sourceFiles.Length} calculator files to analyze:");
            foreach (var file in sourceFiles)
            {
                Console.WriteLine($"  - {Path.GetFileName(file)}");
            }

            Assert.True(sourceFiles.Any(), "No calculator files found for analysis");

            // Act - Analyze each calculator
            foreach (var filePath in sourceFiles)
            {
                try
                {
                    var fileName = Path.GetFileNameWithoutExtension(filePath);
                    var syntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(filePath));
                    var compilation = CSharpCompilation.Create("Test").AddSyntaxTrees(syntaxTree);
                    var model = compilation.GetSemanticModel(syntaxTree);
                    var result = fitnessAnalyzer.AnalyzeCode(fileName, File.ReadAllText(filePath), 10, 3);

                    // Set the calculator name on the result if it has that property

                    results.AddRange(result);
                    Console.WriteLine($"✓ Analyzed: {fileName}");
                }
                catch (Exception ex)
                {
                    var calculatorName = Path.GetFileNameWithoutExtension(filePath);
                    Console.WriteLine($"✗ Failed to analyze {calculatorName}: {ex.Message}");
                }
            }

            // Assert
            Assert.True(results.Any(), "No Files were successfully analyzed");

            // Analyze results
            var failedAnalyses = results.Where(r => !r.Passed).ToList();
            var totalAnalyzed = results.Count;

            Console.WriteLine($"\n=== Big O Analysis Results ===");
            Console.WriteLine($"Total Files Analyzed: {totalAnalyzed}");
            Console.WriteLine($"Acceptable Complexity: {totalAnalyzed - failedAnalyses.Count}");
            Console.WriteLine($"Concerning Complexity: {failedAnalyses.Count}");
            Console.WriteLine();

            foreach (var result in results.OrderBy(r => r.TestName))
            {
                Console.WriteLine($"{result.TestName}: {result.Passed} - {result.Details}");
            }

            // Assert that most calculators have acceptable complexity
            var acceptableRatio = (double)(totalAnalyzed - failedAnalyses.Count) / totalAnalyzed;
            Assert.True(acceptableRatio >= 0.7,
                $"Only {acceptableRatio:P0} of Files have acceptable complexity. Expected at least 70%.");
        }
    }
}
