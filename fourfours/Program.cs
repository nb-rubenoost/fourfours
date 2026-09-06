using fourfours.Core;

var solver = FFSolverFactory.CreateSolver(4, 4, 4, 4);
var solution = solver.Solve();

foreach(var key in solution.SortedKeys)
{
    var expression = solution.GetSolution(key);
    Console.WriteLine($"{key} = {expression}");
}