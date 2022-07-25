namespace LCalc.Helpers;

internal static class Guard
{
    public static Result IndexInRange<T>(IReadOnlyCollection<T> collection, int index) =>
        0 < index && index < collection.Count ? new Result() : Err(new IndexOutOfRangeException());
    
    public static Result IndexInRange<T>(IReadOnlyCollection<T> collection, int index1, int index2) =>
        0 <= index1 && index1 < collection.Count && 0 <= index2 && index2 < collection.Count ? new Result() : Err(new IndexOutOfRangeException());
}