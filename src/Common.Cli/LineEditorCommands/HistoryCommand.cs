using RadLine;

namespace Common.Cli.LineEditorCommands;

/// <summary>
/// Go back in history
/// </summary>
public sealed class HistoryCommand : LineEditorCommand
{
    /// <summary>
    /// The history entries. This is never empty
    /// </summary>
    private readonly IReadOnlyList<string> _history;
    /// <summary>
    /// Current text position (-1 is current)
    /// </summary>
    private int _position = -1;
    private bool _goUp = true;
    
    public HistoryCommand(IReadOnlyList<string> history)
    {
        _history = history;
    }

    public HistoryCommand GoUp()
    {
        _goUp = true;
        return this;
    }

    public HistoryCommand GoDown()
    {
        _goUp = false;
        return this;
    }

    public override void Execute(LineEditorContext context)
    {
        if (!context.Buffer.AtEnd)
            return;

        if (_goUp)
        {
            GoUp(context);
            return;
        }
        
        GoDown(context);
    }

    private void GoUp(LineEditorContext context)
    {
        // Only execute if editor is empty or there is no modification
        // and there is at least one history entry remains
        if (context.Buffer.Length > 0)
        {
            if (_position is -1 ||
                _position + 1 >= _history.Count)
                return;
            
            if (context.Buffer.Content != _history[_position])
                return;
        }
        else
            _position = -1;

        SetLine(context.Buffer, _history[++_position]);
    }
    
    private void GoDown(LineEditorContext context)
    {
        // Only execute if editor is empty or there is no modification
        // and there is at least one history entry remains
        if (context.Buffer.Length is 0)
            return;
        
        if (_position is 0)
            return;

        if (context.Buffer.Content != _history[_position])
            return;

        SetLine(context.Buffer, _history[--_position]);
    }

    private void SetLine(LineBuffer buffer, string text)
    {
        buffer.Clear(0, Int32.MaxValue);
        buffer.MoveHome();
        buffer.Insert(text);
        buffer.MoveEnd();
    }
}