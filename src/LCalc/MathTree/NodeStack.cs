using Common;

namespace LCalc.MathTree;

internal sealed class NodeStack
{
    private LinkedNode<List<IMathNode>> _current = new(new List<IMathNode>());

    public List<IMathNode>? PreviousLevel => _current.Previous?.Value;
    public List<IMathNode> CurrentLevel => _current.Value;

    public bool MoveUp()
    {
        var prevLevel = _current.Previous;
        if (prevLevel is null)
            return false;

        _current = prevLevel;
        return true;
    }

    public void MoveDownAndClear()
    {
        if (!MoveDown())
        {
            _current.AddAfter(new LinkedNode<List<IMathNode>>(new List<IMathNode>()));
            _current = _current.Next!;
        }

        ClearLevel();
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