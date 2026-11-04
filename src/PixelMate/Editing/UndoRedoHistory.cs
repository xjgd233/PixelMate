namespace PixelMate.Editing;

public sealed record UndoRedoEntry<T>(T State, string Description);

public sealed class UndoRedoHistory<T>
{
    private readonly List<UndoRedoEntry<T>> _entries = [];
    private int _index = -1;

    public UndoRedoHistory(int capacity = 100)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity));
        }

        Capacity = capacity;
    }

    public event EventHandler? Changed;

    public int Capacity { get; }

    public bool CanUndo => _index > 0;

    public bool CanRedo => _index >= 0 && _index < _entries.Count - 1;

    public void Reset(T state, string description = "初始状态")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        _entries.Clear();
        _entries.Add(new UndoRedoEntry<T>(state, description));
        _index = 0;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void Record(T state, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        if (_index < 0)
        {
            throw new InvalidOperationException("撤销历史尚未初始化。");
        }

        if (_index < _entries.Count - 1)
        {
            _entries.RemoveRange(_index + 1, _entries.Count - _index - 1);
        }

        _entries.Add(new UndoRedoEntry<T>(state, description));
        _index = _entries.Count - 1;

        var excess = _entries.Count - Capacity;
        if (excess > 0)
        {
            _entries.RemoveRange(0, excess);
            _index -= excess;
        }

        Changed?.Invoke(this, EventArgs.Empty);
    }

    public string? Undo(out T state)
    {
        if (!CanUndo)
        {
            state = default!;
            return null;
        }

        var description = _entries[_index].Description;
        _index--;
        state = _entries[_index].State;
        Changed?.Invoke(this, EventArgs.Empty);
        return description;
    }

    public string? Redo(out T state)
    {
        if (!CanRedo)
        {
            state = default!;
            return null;
        }

        _index++;
        state = _entries[_index].State;
        Changed?.Invoke(this, EventArgs.Empty);
        return _entries[_index].Description;
    }
}
