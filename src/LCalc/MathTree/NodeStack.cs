namespace LCalc.MathTree;

internal sealed class NodeStack
{
    private readonly LinkedList<List<IMathNode>> _list;
    private LinkedListNode<List<IMathNode>> _current;

    public NodeStack()
    {
        _list = new LinkedList<List<IMathNode>>();
        _current = new LinkedListNode<List<IMathNode>>(new List<IMathNode>());
        _list.AddLast(_current);
    }

    public List<IMathNode>? PreviousLevel => _current.Previous?.Value;
    public List<IMathNode> CurrentLevel => _current.Value;

    public void AddLevel()
    {
        _list.AddLast(new List<IMathNode>());
    }

    public bool MoveUp()
    {
        var prevLevel = _current.Previous;
        if (prevLevel is null)
            return false;

        _current = prevLevel;
        return true;
    }

    public bool MoveDownAndClear()
    {
        if (!MoveDown())
            return false;

        ClearLevel();
        return true;
    }

    public bool MoveDown()
    {
        var nextLevel = _current.Next;
        if (nextLevel is null)
            return false;

        _current = nextLevel;
        return true;
    }

    public void ClearLevel()
    {
        _current.Value.Clear();
    }
}