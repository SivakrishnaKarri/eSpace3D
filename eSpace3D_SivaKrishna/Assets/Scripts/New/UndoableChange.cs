using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct UndoableChange 
{
    private List<ObjectState> _before;
    private List<ObjectState> _after;
    private ObjectState before;
    private ObjectState after;

   /* public UndoableChange(List<ObjectState> before, List<ObjectState> after)
    {
        _before = before;
        _after = after;
    }
    */
    public UndoableChange(ObjectState before, ObjectState after) : this()
    {
        this.before = before;
        this.after = after;
    }

    public void Undo()
    {
        foreach (var state in _before)
        {
            state.Apply();
        }
    }

    public void Redo()
    {
        foreach (var state in _after)
        {
            state.Apply();
        }
    }
}