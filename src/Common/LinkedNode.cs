namespace Common;

internal sealed class LinkedNode<T>
{
    private T _item;

    public LinkedNode(T value)
    {
        _item = value;
    }

    public LinkedNode<T>? Next { get; private set; }

    public LinkedNode<T>? Previous { get; private set; }

    public T Value
    {
        get => _item;
        set => _item = value;
    }

    /// <summary>Gets a reference to the value held by the node.</summary>
    public ref T ValueRef => ref _item;

    public void AddPrevious(LinkedNode<T> node)
    {
        if (Previous is not null)
        {
            Previous.Next = node;
            node.Previous = Previous;
        }

        Previous = node;
        Previous.Next = this;
    }

    public void AddAfter(LinkedNode<T> node)
    {
        if (Next is not null)
        {
            Next.Previous = node;
            node.Next = Next;
        }

        Next = node;
        Next.Previous = this;
    }

    public void ReplacePrevious(LinkedNode<T> node)
    {
        node.Previous = Previous?.Previous;
        node.Next = this;
        Previous = node;
    }

    public void ReplaceNext(LinkedNode<T> node)
    {
        node.Next = Next?.Next;
        node.Previous = this;
        Next = node;
    }

    public void RemoveNext()
    {
        Next = Next?.Next;
    }

    public void RemovePrevious()
    {
        Previous = Previous?.Previous;
    }
}