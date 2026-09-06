using System.Globalization;
using System.Numerics;
using MathNet.Numerics;

namespace fourfours.Core;

public static class FFSolverFactory
{
    public static ISolver CreateSolver(params int[] numbers)
    {
        var max = new Count1<byte>(4);
        return new FFSolver2<Count1<byte>>([4, 4, 4, 4], [4], [new Count1<byte>(1)], max);
    }
}

public sealed class FFSolver2<TCount> : ISolver where TCount : ICount<TCount>
{
    private readonly int[] _allNumbers;
    private readonly int[] _distinctNumber;
    private readonly TCount[] _singularCounts;
    private readonly TCount _maxCount;

    public FFSolver2(int[] allNumbers, int[] distinctNumber, TCount[] singularCounts, TCount maxCount)
    {
        _allNumbers = allNumbers;
        _distinctNumber = distinctNumber;
        _singularCounts = singularCounts;
        _maxCount = maxCount;
    }

    public FFSolution Solve()
    {
        var k = _allNumbers.Length;

        // Init temp storage
        var tempStorage = new Dictionary<(TCount, double), IExpression>[k];
        for (var i = 0; i < k; i++)
        {
            tempStorage[i] = new Dictionary<(TCount, double), IExpression>();
        }

        // Init storage with constants
        // TODO: Base this on the numbers
        var zero = TCount.Zero;
        var x = _singularCounts[0];
        AddSolution(tempStorage[0], zero + x, 4, new ConstantExpression(4));
        AddSolution(tempStorage[1], zero + x + x, 44, new ConstantExpression(44));
        AddSolution(tempStorage[2], zero + x + x + x, 444, new ConstantExpression(444));
        AddSolution(tempStorage[3], zero + x + x + x + x, 4444, new ConstantExpression(4444));

        //AddSolution(tempStorage, [4], 0.4d, ".4");
        //AddSolution(tempStorage, [4, 4], 0.44d, ".44");
        //AddSolution(tempStorage, [4, 4, 4], 0.444d, ".444");
        //AddSolution(tempStorage, [4, 4, 4, 4], 0.4444d, ".4444");

        //AddSolution(tempStorage, [4], (double) 4 / 9, ".444444....");

        for (var totalNumberCount = 2; totalNumberCount <= k; totalNumberCount++)
        {
            var targetSet = tempStorage[totalNumberCount - 1];

            // Iterate through all possible counts for A
            var limitA = totalNumberCount - 1;
            for (var countA = 1; countA <= limitA; countA++)
            {
                var numberSetsA = tempStorage[countA - 1];
                if (numberSetsA.Count == 0) continue;

                // Figure out the required count for B
                var countB = totalNumberCount - countA;
                var numberSetsB = tempStorage[countB - 1];
                if (numberSetsB.Count == 0) continue;

                // Loop through all A and B combinations
                foreach (var (keyA, expressionA) in numberSetsA)
                {
                    foreach (var (keyB, expressionB) in numberSetsB)
                    {
                        if (!keyA.Item1.CanAdd(keyB.Item1, _maxCount)) continue;

                        var combinedNumberset = keyA.Item1 + keyB.Item1;

                        AddSolution(targetSet, combinedNumberset, keyA.Item2 + keyB.Item2,
                            new Addition(expressionA, expressionB));
                        AddSolution(targetSet, combinedNumberset, keyA.Item2 - keyB.Item2,
                            new Subtraction(expressionA, expressionB));
                        AddSolution(targetSet, combinedNumberset, keyA.Item2 * keyB.Item2,
                            new Multiplication(expressionA, expressionB));
                        if (keyB.Item2 != 0)
                        {
                            AddSolution(targetSet, combinedNumberset, keyA.Item2 / keyB.Item2,
                                new Division(expressionA, expressionB));
                        }
                    }
                }
            }
        }

        return new FFSolution(tempStorage[k - 1]
            .Where(x => double.IsInteger(x.Key.Item2) && x.Key.Item2 < 1_000_000)
            .GroupBy(x => x.Key.Item2)
            .ToDictionary(x => (long)x.Key, x => x.First().Value.BuildExpression()));
    }

    private void AddSolution(
        Dictionary<(TCount, double), IExpression> tempStorage,
        TCount numberSet,
        double solution,
        IExpression expression)
    {
        AddSolution_Impl(tempStorage, numberSet, solution, expression);

        var rootExpression = new SquareRoot(expression);
        var root = Math.Sqrt(solution);

        AddSolution_Impl(tempStorage, numberSet, root, rootExpression);
        AddSolution_Impl(tempStorage, numberSet, Math.Sqrt(root), new SquareRoot(rootExpression));
        if (solution > 0 && solution < 10)
        {
            if (double.IsInteger(solution))
            {
                AddSolution_Impl(tempStorage, numberSet, Fact(solution - 1), new Gamma(expression));
                AddSolution_Impl(tempStorage, numberSet, Fact(solution), new Factorial(expression));
            }
            else
            {
                AddSolution_Impl(tempStorage, numberSet, SpecialFunctions.Gamma(solution), new Gamma(expression));
                AddSolution_Impl(tempStorage, numberSet, SpecialFunctions.Gamma(solution + 1),
                    new Factorial(expression));
            }
        }
    }

    private double Fact(double n)
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
        Dictionary<(TCount, double), IExpression> tempStorage,
        TCount numberSet,
        double solution,
        IExpression expression)
    {
        var key = (numberSet, solution);
        tempStorage.TryAdd(key, expression);
    }

    private interface IExpression
    {
        string BuildExpression();
    }

    private record ConstantExpression(double Value) : IExpression
    {
        public string BuildExpression() => Value.ToString(CultureInfo.InvariantCulture);
    }

    private record Addition(IExpression ValueA, IExpression ValueB) : IExpression
    {
        public string BuildExpression() => $"({ValueA.BuildExpression()} + {ValueB.BuildExpression()})";
    }

    private record Subtraction(IExpression ValueA, IExpression ValueB) : IExpression
    {
        public string BuildExpression() => $"({ValueA.BuildExpression()} - {ValueB.BuildExpression()})";
    }

    private record Multiplication(IExpression ValueA, IExpression ValueB) : IExpression
    {
        public string BuildExpression() => $"({ValueA.BuildExpression()} * {ValueB.BuildExpression()})";
    }

    private record Division(IExpression ValueA, IExpression ValueB) : IExpression
    {
        public string BuildExpression() => $"({ValueA.BuildExpression()} / {ValueB.BuildExpression()})";
    }

    private record SquareRoot(IExpression Value) : IExpression
    {
        public string BuildExpression() => $"sqrt({Value.BuildExpression()})";
    }

    private record Factorial(IExpression Value) : IExpression
    {
        public string BuildExpression() => $"({Value.BuildExpression()})!";
    }

    private record Gamma(IExpression Value) : IExpression
    {
        public string BuildExpression() => $"gamma({Value.BuildExpression()})";
    }
}

public interface ICount<TCount> where TCount : ICount<TCount>
{
    bool CanAdd(TCount other, TCount max);
    static abstract TCount Zero { get; }
    static abstract TCount operator +(TCount left, TCount right);
}

file readonly record struct Count1<T>(T C1) : ICount<Count1<T>> where T : INumber<T>
{
    public bool CanAdd(Count1<T> other, Count1<T> max) => C1 + other.C1 <= max.C1;
    public static Count1<T> Zero => new(T.Zero);
    public static Count1<T> operator +(Count1<T> left, Count1<T> right) => new(left.C1 + right.C1);
}