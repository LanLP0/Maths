namespace Common;

internal sealed class LinkedNode<T>
{
    private LinkedNode<T>? _next;
    private LinkedNode<T>? _prev;
    private T _item;

    public LinkedNode(T value)
    {
        _item = value;
    }

    public LinkedNode<T>? Next => _next;

    public LinkedNode<T>? Previous => _prev;

    public T Value
    {
        get => _item;
        set => _item = value;
    }

    public void AddPrevious(LinkedNode<T> node)
    {
        if (_prev is not null)
            _prev._next = node;

        _prev = node;
        _prev._next = this;
    }

    public void AddAfter(LinkedNode<T> node)
    {
        if (_next is not null)
            _next._prev = this;

        _next = node;
        _next._prev = this;
    }

    /// <summary>Gets a reference to the value held by the node.</summary>
    public ref T ValueRef => ref _item;
}