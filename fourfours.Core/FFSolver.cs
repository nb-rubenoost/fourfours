using fourfours.Core.Helpers;
using MathNet.Numerics;

namespace fourfours.Core;

public sealed class FFSolver
{
    private readonly NumberSet _numbers;

    public FFSolver(params int[] numbers)
    {
        _numbers = new NumberSet(numbers);
    }

    public FFSolution Solve()
    {
        var k = _numbers.Count;

        var tempStorage = new Dictionary<int, Dictionary<NumberSet, Dictionary<double, string>>>();

        // TODO: Base this on the numbers
        AddSolution(tempStorage, [4], 4, "4");
        AddSolution(tempStorage, [4, 4], 44, "44");
        AddSolution(tempStorage, [4, 4, 4], 444, "444");
        AddSolution(tempStorage, [4, 4, 4, 4], 4444, "4444");
        
        //AddSolution(tempStorage, [4], 0.4d, ".4");
        //AddSolution(tempStorage, [4, 4], 0.44d, ".44");
        //AddSolution(tempStorage, [4, 4, 4], 0.444d, ".444");
        //AddSolution(tempStorage, [4, 4, 4, 4], 0.4444d, ".4444");
        
        //AddSolution(tempStorage, [4], (double) 4 / 9, ".444444....");
        
        for (var totalNumberCount = 2; totalNumberCount <= k; totalNumberCount++)
        {
            var limitA = totalNumberCount - 1;
            for (var countA = 1; countA <= limitA; countA++)
            {
                var numberSetsA = tempStorage.GetValueOrDefault(countA);
                if (numberSetsA == null || numberSetsA.Count == 0) continue;

                var countB = totalNumberCount - countA;
                var numberSetsB = tempStorage.GetValueOrDefault(countB);
                if (numberSetsB == null || numberSetsB.Count == 0) continue;

                foreach (var (numberSetA, solutionsA) in numberSetsA)
                {
                    foreach (var (numberSetB, solutionsB) in numberSetsB)
                    {
                        var combinedNumberset = numberSetA + numberSetB;
                        if (!combinedNumberset.IsSubsetOf(_numbers)) continue;

                        foreach (var (solutionA, expressionA) in solutionsA)
                        {
                            foreach (var (solutionB, expressionB) in solutionsB)
                            {
                                AddSolution(tempStorage, combinedNumberset, solutionA + solutionB, $"({expressionA} + {expressionB})");
                                AddSolution(tempStorage, combinedNumberset, solutionA - solutionB, $"({expressionA} - {expressionB})");
                                AddSolution(tempStorage, combinedNumberset, solutionA * solutionB, $"({expressionA} * {expressionB})");
                                if (solutionB != 0)
                                {
                                    AddSolution(tempStorage, combinedNumberset, solutionA / solutionB,
                                        $"({expressionA} / {expressionB})");
                                }
                            }
                        }
                    }
                }
            }
        }

        return new FFSolution(tempStorage[4]
            //.SelectMany(x => x.Value) // TODO: only select k
            .SelectMany(x => x.Value)
            .Where(x => double.IsInteger(x.Key) && x.Key < 1_000_000)
            .GroupBy(x => x.Key)
            .ToDictionary(x => (long)x.Key, x => x.First().Value));
    }

    private void AddSolution(
        Dictionary<int, Dictionary<NumberSet, Dictionary<double, string>>> tempStorage,
        int[] numbers,
        double solution,
        string expression)
    {
        AddSolution(tempStorage, new NumberSet(numbers), solution, expression);
    }

    private void AddSolution(
        Dictionary<int, Dictionary<NumberSet, Dictionary<double, string>>> tempStorage,
        NumberSet numberSet,
        double solution,
        string expression)
    {
        AddSolution_Impl(tempStorage, numberSet, solution, expression);
        AddSolution_Impl(tempStorage, numberSet, Math.Sqrt(solution), $"sqrt({expression})");
        AddSolution_Impl(tempStorage, numberSet, Math.Sqrt(Math.Sqrt(solution)), $"sqrt(sqrt({expression}))");
        if (solution > 0 && solution < 10)
        {
            if (double.IsInteger(solution))
            {
                AddSolution_Impl(tempStorage, numberSet, Factorial(solution - 1), $"gamma({expression})");
                AddSolution_Impl(tempStorage, numberSet, Factorial(solution), $"({expression})!");
            }
            else
            {
                AddSolution_Impl(tempStorage, numberSet, SpecialFunctions.Gamma(solution), $"gamma({expression})");
                AddSolution_Impl(tempStorage, numberSet, SpecialFunctions.Gamma(solution + 1), $"({expression})!");
            }
        }
    }
    
    private double Factorial(double n)
    {
        if (n < 0) throw new ArgumentException("Negative input is not allowed.", nameof(n));
        if (n == 0 || n == 1) return 1;
        double result = 1;
        for (double i = 2; i <= n; i++)
        {
            result *= i;
        }
        return result;
    }
    
    private void AddSolution_Impl(
        Dictionary<int, Dictionary<NumberSet, Dictionary<double, string>>> tempStorage,
        NumberSet numberSet,
        double solution,
        string expression)
    {
        if (!tempStorage.TryGetValue(numberSet.Count, out var numberSetSolutions))
        {
            numberSetSolutions = new Dictionary<NumberSet, Dictionary<double, string>>();
            tempStorage[numberSet.Count] = numberSetSolutions;
        }

        if (!numberSetSolutions.TryGetValue(numberSet, out var solutions))
        {
            solutions = new Dictionary<double, string>();
            numberSetSolutions[numberSet] = solutions;
        }

        if (!solutions.ContainsKey(solution))
        {
            solutions[solution] = expression;
        }
    }
}