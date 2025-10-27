using FitnessTests.CodeAnalysis;
using FitnessTests.Library.Models;
using HC.TechnicalCalculators.Src.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Reflection;

namespace HC.TechnicalCalculators.Tests.Fitness
{
    public class FitnessTests
    {
        private readonly string _solutionDirectory;
        private readonly string[] _calculatorDirectories;

        public FitnessTests()
        {
            // Find the solution directory by walking up from the current test assembly location
            _solutionDirectory = FindSolutionDirectory();

            _calculatorDirectories = new[]
            {
                Path.Combine(_solutionDirectory, "HC.TechnicalCalculators", "Src", "Calculators", "Momentum"),
                Path.Combine(_solutionDirectory, "HC.TechnicalCalculators", "Src", "Calculators", "Overlap"),
                Path.Combine(_solutionDirectory, "HC.TechnicalCalculators", "Src", "Calculators", "Volume"),
                Path.Combine(_solutionDirectory, "HC.TechnicalCalculators", "Src", "Calculators", "Volatility"),
                Path.Combine(_solutionDirectory, "HC.TechnicalCalculators", "Src", "Calculators", "Statistics"),
                Path.Combine(_solutionDirectory, "HC.TechnicalCalculators", "Src", "Calculators", "Price"),
                Path.Combine(_solutionDirectory, "HC.TechnicalCalculators", "Src", "Calculators", "News")
            };
        }

        [Fact]
        public void Test_BigO_Performance_AnalyzeAllCalculators()
        {
            // Arrange
            var calculatorFiles = GetAllCalculatorFiles();
            var results = new List<FitnessResult>();
            var fitnessAnalyzer = new FitnessAnalyzer();
            Console.WriteLine($"Solution Directory: {_solutionDirectory}");
            Console.WriteLine($"Found {calculatorFiles.Count} calculator files to analyze:");
            foreach (var file in calculatorFiles)
            {
                Console.WriteLine($"  - {Path.GetFileName(file)}");
            }

            Assert.True(calculatorFiles.Any(), "No calculator files found for analysis");

            // Act - Analyze each calculator
            foreach (var filePath in calculatorFiles)
            {
                try
                {
                    var calculatorName = Path.GetFileNameWithoutExtension(filePath);
                    var syntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(filePath));
                    var compilation = CSharpCompilation.Create("Test").AddSyntaxTrees(syntaxTree);
                    var model = compilation.GetSemanticModel(syntaxTree);
                    var result = fitnessAnalyzer.AnalyzeCode(calculatorName, File.ReadAllText(filePath), 10, 3);
                    
                    // Set the calculator name on the result if it has that property
                    
                    results.AddRange(result);
                    Console.WriteLine($"✓ Analyzed: {calculatorName}");
                }
                catch (Exception ex)
                {
                    var calculatorName = Path.GetFileNameWithoutExtension(filePath);
                    Console.WriteLine($"✗ Failed to analyze {calculatorName}: {ex.Message}");
                }
            }

            // Assert
            Assert.True(results.Any(), "No calculators were successfully analyzed");

            // Analyze results
            var failedAnalyses = results.Where(r => !r.Passed).ToList();
            var totalAnalyzed = results.Count;

            Console.WriteLine($"\n=== Big O Analysis Results ===");
            Console.WriteLine($"Total Calculators Analyzed: {totalAnalyzed}");
            Console.WriteLine($"Acceptable Complexity: {totalAnalyzed - failedAnalyses.Count}");
            Console.WriteLine($"Concerning Complexity: {failedAnalyses.Count}");
            Console.WriteLine();

            foreach (var result in results.OrderBy(r => r.TestName))
            {
                Console.WriteLine($"{result.TestName}: {result.Passed} - {result.Details }");
            }

            // Assert that most calculators have acceptable complexity
            var acceptableRatio = (double)(totalAnalyzed - failedAnalyses.Count) / totalAnalyzed;
            Assert.True(acceptableRatio >= 0.7,
                $"Only {acceptableRatio:P0} of calculators have acceptable complexity. Expected at least 70%.");
        }

        [Theory]
        [InlineData(CalculatorNameEnum.SMA)]
        [InlineData(CalculatorNameEnum.EMA)]
        [InlineData(CalculatorNameEnum.RSI)]
        [InlineData(CalculatorNameEnum.ADX)]
        [InlineData(CalculatorNameEnum.MACD)]
        public void Test_BigO_SpecificCalculator_ShouldHaveLinearComplexity(CalculatorNameEnum calculatorType)
        {
            // Arrange
            var calculatorFile = FindCalculatorFile(calculatorType);

            if (calculatorFile == null)
            {
                // If we can't find the file, let's see what files we can find
                var allFiles = GetAllCalculatorFiles();
                Console.WriteLine($"Could not find file for {calculatorType}. Available files:");
                foreach (var file in allFiles)
                {
                    Console.WriteLine($"  - {Path.GetFileName(file)}");
                }

                Assert.True(false, $"Calculator file for {calculatorType} not found. Check file paths and naming conventions.");
            }

            // Act
            var result = AnalyzeCalculatorComplexity(calculatorFile);

            // Assert
            Console.WriteLine($"{calculatorType} Analysis:");
            Console.WriteLine($"  File: {Path.GetFileName(calculatorFile)}");
            Console.WriteLine($"  Estimated Complexity: {result.EstimatedComplexity}");
            Console.WriteLine($"  Loop Count: {result.LoopCount}");
            Console.WriteLine($"  Nested Loop Depth: {result.MaxNestedLoopDepth}");
            Console.WriteLine($"  Method Count: {result.MethodCount}");
            Console.WriteLine($"  Is Acceptable: {result.IsAcceptableComplexity}");

            if (result.AnalysisNotes.Any())
            {
                Console.WriteLine($"  Notes: {string.Join(", ", result.AnalysisNotes)}");
            }

            Assert.True(result.IsAcceptableComplexity,
                $"{calculatorType} should have acceptable complexity. Analysis: {result.EstimatedComplexity}");
        }

        private string FindSolutionDirectory()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var directory = new DirectoryInfo(currentDirectory);

            // Walk up the directory tree until we find the solution directory
            while (directory != null)
            {
                // Look for .sln file or the HC.TechnicalCalculators directory
                if (directory.GetFiles("*.sln").Any() ||
                    directory.GetDirectories("HC.TechnicalCalculators").Any())
                {
                    return directory.FullName;
                }
                directory = directory.Parent;
            }

            // Fallback: assume we're in the test project directory and go up two levels
            var testProjectDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            for (int i = 0; i < 5; i++) // Try going up 5 levels max
            {
                testProjectDir = Directory.GetParent(testProjectDir)?.FullName;
                if (testProjectDir != null && Directory.Exists(Path.Combine(testProjectDir, "HC.TechnicalCalculators")))
                {
                    return testProjectDir;
                }
            }

            throw new DirectoryNotFoundException("Could not find solution directory containing HC.TechnicalCalculators project");
        }

        private List<string> GetAllCalculatorFiles()
        {
            var files = new List<string>();

            foreach (var directory in _calculatorDirectories)
            {
                Console.WriteLine($"Checking directory: {directory}");

                if (Directory.Exists(directory))
                {
                    var csFiles = Directory.GetFiles(directory, "*.cs", SearchOption.TopDirectoryOnly)
                        .Where(f => !Path.GetFileName(f).Equals("BaseCalculator.cs", StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    files.AddRange(csFiles);
                    Console.WriteLine($"  Found {csFiles.Count} files");
                }
                else
                {
                    Console.WriteLine($"  Directory does not exist: {directory}");
                }
            }

            return files;
        }

        private string? FindCalculatorFile(CalculatorNameEnum calculatorType)
        {
            var allFiles = GetAllCalculatorFiles();
            var searchName = calculatorType.ToString().ToLower();

            // Try exact matches first
            var exactMatch = allFiles.FirstOrDefault(f =>
                Path.GetFileNameWithoutExtension(f).ToLower().Equals(searchName + "calculator"));

            if (exactMatch != null)
                return exactMatch;

            // Try partial matches
            var partialMatch = allFiles.FirstOrDefault(f =>
                Path.GetFileNameWithoutExtension(f).ToLower().Contains(searchName));

            if (partialMatch != null)
                return partialMatch;

            // Special cases for naming inconsistencies
            return calculatorType switch
            {
                CalculatorNameEnum.SMA => allFiles.FirstOrDefault(f => f.Contains("SimpleMovingAverage")),
                CalculatorNameEnum.EMA => allFiles.FirstOrDefault(f => f.Contains("ExponentialMovingAverage")),
                CalculatorNameEnum.MACD => allFiles.FirstOrDefault(f => f.Contains("MacdCalculator")),
                _ => null
            };
        }

        private BigOAnalysisResult AnalyzeCalculatorComplexity(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Calculator file not found: {filePath}");
            }

            var sourceCode = File.ReadAllText(filePath);
            var tree = CSharpSyntaxTree.ParseText(sourceCode);
            var root = tree.GetCompilationUnitRoot();

            var analyzer = new ComplexityAnalyzer();
            return analyzer.Analyze(filePath, root);
        }
    }

    public class BigOAnalysisResult
    {
        public required string CalculatorName { get; set; }
        public required string FilePath { get; set; }
        public required string EstimatedComplexity { get; set; }
        public required bool IsAcceptableComplexity { get; set; }
        public int LoopCount { get; set; }
        public int MaxNestedLoopDepth { get; set; }
        public int MethodCount { get; set; }
        public int RecursiveMethodCount { get; set; }
        public List<string> AnalysisNotes { get; set; } = new();
    }

    public class ComplexityAnalyzer : CSharpSyntaxWalker
    {
        private int _loopCount;
        private int _currentNestingLevel;
        private int _maxNestingLevel;
        private int _methodCount;
        private int _recursiveMethodCount;
        private readonly List<string> _analysisNotes = new();
        private readonly HashSet<string> _methodNames = new();

        public BigOAnalysisResult Analyze(string filePath, SyntaxNode root)
        {
            // Reset state
            _loopCount = 0;
            _currentNestingLevel = 0;
            _maxNestingLevel = 0;
            _methodCount = 0;
            _recursiveMethodCount = 0;
            _analysisNotes.Clear();
            _methodNames.Clear();

            // Walk the syntax tree
            Visit(root);

            var calculatorName = Path.GetFileNameWithoutExtension(filePath);
            var estimatedComplexity = EstimateComplexity();
            var isAcceptable = IsComplexityAcceptable(estimatedComplexity);

            return new BigOAnalysisResult
            {
                CalculatorName = calculatorName,
                FilePath = filePath,
                EstimatedComplexity = estimatedComplexity,
                IsAcceptableComplexity = isAcceptable,
                LoopCount = _loopCount,
                MaxNestedLoopDepth = _maxNestingLevel,
                MethodCount = _methodCount,
                RecursiveMethodCount = _recursiveMethodCount,
                AnalysisNotes = _analysisNotes.ToList()
            };
        }

        public override void VisitForStatement(ForStatementSyntax node)
        {
            _loopCount++;
            _currentNestingLevel++;
            _maxNestingLevel = Math.Max(_maxNestingLevel, _currentNestingLevel);

            base.VisitForStatement(node);
            _currentNestingLevel--;
        }

        public override void VisitWhileStatement(WhileStatementSyntax node)
        {
            _loopCount++;
            _currentNestingLevel++;
            _maxNestingLevel = Math.Max(_maxNestingLevel, _currentNestingLevel);

            base.VisitWhileStatement(node);
            _currentNestingLevel--;
        }

        public override void VisitForEachStatement(ForEachStatementSyntax node)
        {
            _loopCount++;
            _currentNestingLevel++;
            _maxNestingLevel = Math.Max(_maxNestingLevel, _currentNestingLevel);

            base.VisitForEachStatement(node);
            _currentNestingLevel--;
        }

        public override void VisitDoStatement(DoStatementSyntax node)
        {
            _loopCount++;
            _currentNestingLevel++;
            _maxNestingLevel = Math.Max(_maxNestingLevel, _currentNestingLevel);

            base.VisitDoStatement(node);
            _currentNestingLevel--;
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            _methodCount++;
            var methodName = node.Identifier.Text;
            _methodNames.Add(methodName);

            // Check for potential recursion (simplified check)
            var methodBody = node.Body?.ToString() ?? string.Empty;
            if (methodBody.Contains(methodName) && methodName != "ToString" && methodName != "GetHashCode")
            {
                _recursiveMethodCount++;
                _analysisNotes.Add($"Potential recursion in method: {methodName}");
            }

            // Check for LINQ operations that might indicate complexity
            if (methodBody.Contains("OrderBy") || methodBody.Contains("Sort"))
            {
                _analysisNotes.Add($"Sorting operation detected in method: {methodName}");
            }

            if (methodBody.Contains("GroupBy") || methodBody.Contains("Join"))
            {
                _analysisNotes.Add($"Grouping/Join operation detected in method: {methodName}");
            }

            base.VisitMethodDeclaration(node);
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            var memberAccess = node.Expression as MemberAccessExpressionSyntax;
            var methodName = memberAccess?.Name?.Identifier.Text ??
                            (node.Expression as IdentifierNameSyntax)?.Identifier.Text;

            // Check for potentially expensive operations
            if (methodName != null)
            {
                var expensiveOperations = new[] { "Sort", "OrderBy", "Reverse", "Distinct", "GroupBy" };
                if (expensiveOperations.Contains(methodName))
                {
                    _analysisNotes.Add($"Potentially expensive operation: {methodName}");
                }

                // Check for TALib function calls
                var expression = node.Expression.ToString();
                if (expression.StartsWith("TALib") || expression.Contains("Core."))
                {
                    _analysisNotes.Add("TALib function call detected (typically O(n))");
                }
            }

            base.VisitInvocationExpression(node);
        }

        private string EstimateComplexity()
        {
            // Estimate based on loop nesting and other factors
            if (_maxNestingLevel >= 3)
            {
                return "O(n³) or worse";
            }
            else if (_maxNestingLevel == 2)
            {
                return "O(n²)";
            }
            else if (_loopCount > 0 || _analysisNotes.Any(n => n.Contains("TALib")))
            {
                return "O(n)";
            }
            else if (_analysisNotes.Any(n => n.Contains("Sort") || n.Contains("OrderBy")))
            {
                return "O(n log n)";
            }
            else if (_recursiveMethodCount > 0)
            {
                return "O(n) or O(log n) - Recursive";
            }
            else
            {
                return "O(1) or O(log n)";
            }
        }

        private bool IsComplexityAcceptable(string complexity)
        {
            var unacceptableComplexities = new[] { "O(n³)", "O(n²)" };
            return !unacceptableComplexities.Any(unacceptable => complexity.Contains(unacceptable));
        }
    }
}
