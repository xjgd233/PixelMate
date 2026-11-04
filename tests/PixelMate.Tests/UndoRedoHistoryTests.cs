using PixelMate.Editing;

namespace PixelMate.Tests;

public sealed class UndoRedoHistoryTests
{
    [Fact]
    public void UndoAndRedo_RestoreRecordedStates()
    {
        var history = new UndoRedoHistory<string>();
        history.Reset("empty", "打开图片");
        history.Record("measure", "添加测量");
        history.Record("annotation", "添加批注");

        var undone = history.Undo(out var afterFirstUndo);
        var undoneAgain = history.Undo(out var afterSecondUndo);
        var redone = history.Redo(out var afterFirstRedo);
        var redoneAgain = history.Redo(out var afterSecondRedo);

        Assert.Equal("添加批注", undone);
        Assert.Equal("measure", afterFirstUndo);
        Assert.Equal("添加测量", undoneAgain);
        Assert.Equal("empty", afterSecondUndo);
        Assert.Equal("添加测量", redone);
        Assert.Equal("measure", afterFirstRedo);
        Assert.Equal("添加批注", redoneAgain);
        Assert.Equal("annotation", afterSecondRedo);
    }

    [Fact]
    public void RecordingAfterUndo_DiscardsRedoBranch()
    {
        var history = new UndoRedoHistory<int>();
        history.Reset(0, "初始状态");
        history.Record(1, "第一步");
        history.Record(2, "第二步");
        _ = history.Undo(out _);

        history.Record(3, "新分支");
        var redone = history.Redo(out _);

        Assert.Null(redone);
        Assert.False(history.CanRedo);
    }

    [Fact]
    public void Capacity_KeepsMostRecentEntries()
    {
        var history = new UndoRedoHistory<int>(capacity: 2);
        history.Reset(0, "初始状态");
        history.Record(1, "第一步");
        history.Record(2, "第二步");

        Assert.True(history.CanUndo);
        Assert.Equal("第二步", history.Undo(out var current));
        Assert.Equal(1, current);
        Assert.False(history.CanUndo);
    }
}
