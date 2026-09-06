using BenchmarkDotNet.Attributes;
using fourfours.Core;

namespace fourfours.Benchmarks;

[MemoryDiagnoser]
public class FFSolverBenchmarks
{
    // Same case as the console app in ../fourfours/Program.cs
    [Benchmark]
    public FFSolution Solve_Baseline()
    {
        var solver = new FFSolver(4, 4, 4, 4);
        return solver.Solve();
    }
    
    [Benchmark]
    public FFSolution Solve()
    {
        var solver = FFSolverFactory.CreateSolver(4, 4, 4, 4);
        return solver.Solve();
    }
}
