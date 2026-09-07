using BenchmarkDotNet.Attributes;
using fourfours.Core;

namespace fourfours.Benchmarks;

[MemoryDiagnoser]
public class FFSolverBenchmarks
{
    // Same case as the console app in ../fourfours/Program.cs
    [Benchmark(Baseline = true)]
    public FFSolution Solver1_4444()
    {
        return FFSolver.Solve(4, 4, 4, 4);
    }

    [Benchmark]
    public FFSolution Solver2_4444()
    {
        return FFSolver2.Solve(4, 4, 4, 4);
    }
    
    [Benchmark]
    public FFSolution Solver2_1234()
    {
        return FFSolver2.Solve(1, 2, 3, 4);
    }
    
    [Benchmark]
    public FFSolution Solver3_4444()
    {
        return FFSolver3.Solve(4, 4, 4, 4);
    }
    
    [Benchmark]
    public FFSolution Solver3_1234()
    {
        return FFSolver3.Solve(1, 2, 3, 4);
    }
}