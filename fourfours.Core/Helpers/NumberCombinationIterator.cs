namespace fourfours.Core.Helpers;

public static class NumberCombinationIterator
{
    public static IEnumerable<long> Iterate(int[] digits)
    {
        HashSet<long> seen = new HashSet<long>();

        for (var i = 1; i <= digits.Length; i++)
        {
            var digit = digits[i - 1];
            if (seen.Add(digit))
                yield return digit;

            // splice the array
            var remainingDigits = digits.Take(i - 1).Concat(digits.Skip(i)).ToArray();
            foreach (var combination in Iterate(remainingDigits))
            {
                var x = 10L * combination + digit;
                if (seen.Add(x))
                    yield return x;
            }
        }
    }
}