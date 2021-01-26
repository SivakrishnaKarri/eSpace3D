using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UndoRedoClass 
{
    private static Stack<UndoableChange> undoStack = new Stack<UndoableChange>();
    private static Stack<UndoableChange> redoStack = new Stack<UndoableChange>();

    public static void Undo()
    {
        if (undoStack.Count == 0) return;

        var lastAction = undoStack.Pop();

        lastAction.Undo();

        redoStack.Push(lastAction);
    }

    public static void Redo()
    {
        if (redoStack.Count == 0) return;

        var lastAction = redoStack.Pop();

        lastAction.Redo();

        undoStack.Push(lastAction);
    }

    public static void AddAction(UndoableChange action)
    {
        redoStack.Clear();

        undoStack.Push(action);
    }
}