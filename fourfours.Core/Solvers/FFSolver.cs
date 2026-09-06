using MathNet.Numerics;

namespace fourfours.Core;

public static class FFSolver
{
    public static FFSolution Solve(params int[] numbers)
    {
        var _numbers = new NumberSet(numbers);
        
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
                                AddSolution(tempStorage, combinedNumberset, solutionA + solutionB,
                                    $"({expressionA} + {expressionB})");
                                AddSolution(tempStorage, combinedNumberset, solutionA - solutionB,
                                    $"({expressionA} - {expressionB})");
                                AddSolution(tempStorage, combinedNumberset, solutionA * solutionB,
                                    $"({expressionA} * {expressionB})");
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

    private static void AddSolution(
        Dictionary<int, Dictionary<NumberSet, Dictionary<double, string>>> tempStorage,
        int[] numbers,
        double solution,
        string expression)
    {
        AddSolution(tempStorage, new NumberSet(numbers), solution, expression);
    }

    private static void AddSolution(
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

    private static double Factorial(double n)
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

    private static void AddSolution_Impl(
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

    private sealed class NumberSet : IEquatable<NumberSet>
    {
        private readonly int[] _numbers;

        public int Count => _numbers.Length;

        public NumberSet(params int[] numbers)
        {
            _numbers = numbers;
            Array.Sort(_numbers);
        }

        /// <summary>
        /// Checks if this is a subset, but also takes into account the occurence of numbers. For example, {1, 1, 2} is not a subset of {1, 2}.
        /// </summary>
        public bool IsSubsetOf(NumberSet other)
        {
            var thisCounts = GetCounts(_numbers);
            var otherCounts = GetCounts(other._numbers);

            foreach (var kvp in thisCounts)
            {
                if (!otherCounts.TryGetValue(kvp.Key, out var count) || count < kvp.Value)
                {
                    return false;
                }
            }

            return true;
        }

        public static NumberSet operator +(NumberSet set1, NumberSet set2)
        {
            var combinedNumbers = new int[set1._numbers.Length + set2._numbers.Length];
            Array.Copy(set1._numbers, combinedNumbers, set1._numbers.Length);
            Array.Copy(set2._numbers, 0, combinedNumbers, set1._numbers.Length, set2._numbers.Length);
            return new NumberSet(combinedNumbers);
        }

        private static Dictionary<int, int> GetCounts(int[] numbers)
        {
            var counts = new Dictionary<int, int>();
            foreach (var number in numbers)
            {
                if (counts.ContainsKey(number))
                {
                    counts[number]++;
                }
                else
                {
                    counts[number] = 1;
                }
            }

            return counts;
        }

        public bool Equals(NumberSet? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return _numbers.SequenceEqual(other._numbers);
        }

        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((NumberSet)obj);
        }

        public override int GetHashCode()
        {
            return _numbers.Aggregate(17, (hash, number) => hash * 31 + number);
        }
    }
}