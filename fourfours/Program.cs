using fourfours.Core;

var solution = FFSolver2.Solve(1,2,3,4);

foreach(var key in solution.SortedKeys)
{
    var expression = solution.GetSolution(key);
    Console.WriteLine($"{key} = {expression}");
}