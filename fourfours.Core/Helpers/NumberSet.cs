namespace fourfours.Core.Helpers;

public class NumberSet : IEquatable<NumberSet>
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