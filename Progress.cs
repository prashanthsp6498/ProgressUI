namespace Progress;

public class ProgressUI : IDisposable
{
    private uint _total;
    private int _current;
    private readonly string _label;
    private readonly uint _offset;
    public static int cursorLine = 0;
    private int _currentCursorLine;
    private readonly string _offsetStr;

    static ProgressUI()
    {
        cursorLine = Console.GetCursorPosition().Top;
    }

    public ProgressUI(string label, uint total, uint offset)
    {
        _total = total;
        _current = 0;
        _label = label;
        _offset = offset;
        Console.WriteLine();
        _currentCursorLine = cursorLine++;
        _offsetStr = GetOffset();
        Tick(null);
    }

    private string GetOffset()
    {
        return _offset > 2 ? new String(' ', (int)_offset + 2 - 3) + "|-" : "";
    }

    private void Tick(object? state)
    {
        Console.SetCursorPosition(0, _currentCursorLine);

        var text = _total == 0 ?
            $"\r {_offsetStr}{_label}" :
            $"\r {_offsetStr}[{_current}/{_total}] {_label}";
        Console.Write(text);
    }

    public void Increment(int value = 1)
    {
        if (_current + value <= _total)
            Interlocked.Add(ref _current, value);
        Tick(null);
    }

    public void Dispose()
    {
        Tick(null);
        Console.SetCursorPosition(0, _currentCursorLine);
        Console.Write("");
    }

    internal void IncrementTask()
    {
        _total = _total + 1;
        Tick(null);
    }
}

public class Node : IDisposable
{
    public readonly ProgressUI _mainProgress;
    protected List<Node> _subProgresses;
    private uint _offset;

    public Node(string label)
    {
        _offset = 0;
        _mainProgress = new ProgressUI(label, 0, _offset);
        _subProgresses = new List<Node>();
    }

    public Node(string label, uint totalTask, uint offset = 1)
    {
        _mainProgress = new ProgressUI(label, totalTask, offset);
        _subProgresses = new List<Node>();
        _offset = offset;
    }

    public Node Start(string label, uint totalTask)
    {
        _mainProgress.IncrementTask();
        _subProgresses.Add(new Node(label, totalTask, _offset + 3));
        return _subProgresses.Last();
    }

    public void CompleteOne() => _mainProgress.Increment();
    public void Dispose()
    {
        foreach (var progress in _subProgresses)
            progress.Dispose();

        _mainProgress.Dispose();
        Console.SetCursorPosition(0, ProgressUI.cursorLine + 1);
    }
}
