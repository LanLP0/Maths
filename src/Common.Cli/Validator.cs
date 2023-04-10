using System.Collections;

namespace Common.Cli;

public sealed class Validator<T> : ICollection<Func<T, string?>>
{
    private readonly List<Func<T, string?>> _validators = new();

    public int Count => _validators.Count;
    public bool IsReadOnly => false;

    public IEnumerator<Func<T, string?>> GetEnumerator()
    {
        return _validators.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _validators.GetEnumerator();
    }

    public void Add(Func<T, string?> item)
    {
        _validators.Add(item);
    }

    public void Clear()
    {
        _validators.Clear();
    }

    public bool Contains(Func<T, string?> item)
    {
        return _validators.Contains(item);
    }

    public void CopyTo(Func<T, string?>[] array, int arrayIndex)
    {
        _validators.CopyTo(array, arrayIndex);
    }

    public bool Remove(Func<T, string?> item)
    {
        return _validators.Remove(item);
    }

    /// <summary>
    ///     Run all validators until a validation error occurred
    /// </summary>
    /// <param name="input">The string to be validated</param>
    /// <param name="errorLine">The error line</param>
    /// <returns>true if there is no validation error occurred; false otherwise</returns>
    public bool RunUntilError(T input, out string errorLine)
    {
        foreach (var validator in _validators)
        {
            var result = validator(input);
            if (string.IsNullOrEmpty(result))
                continue;

            errorLine = result;
            return false;
        }

        errorLine = string.Empty;
        return true;
    }
}