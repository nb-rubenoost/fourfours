namespace fourfours.Core;

public sealed class FFSolution
{
    private readonly Dictionary<long, string> _solutions;

    internal FFSolution(Dictionary<long, string> solutions)
    {
        _solutions = solutions;
    }
    
    public IEnumerable<long> SortedKeys => _solutions.Keys.Order();
    
    public string? GetSolution(long target)
    {
        return _solutions.GetValueOrDefault(target);
    }
}