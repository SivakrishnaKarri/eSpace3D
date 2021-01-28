using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct UndoableChange 
{
    private List<ObjectState> _before;
    private List<ObjectState> _after;
   

    public UndoableChange(List<ObjectState> before, List<ObjectState> after)
    {
        _before = before;
        _after = after;
      //  Undo();
    }
    
   /* public UndoableChange(ObjectState before, ObjectState after) : this()
    {
        this.before = before;
        this.after = after;
      //  Undo();
        before.Apply();
    }*/

    public void Undo()
    {
       // Debug.Log("Coming"+ _before.Count);
      //  var i = _before.Count - 1;
     //   _before[i].Apply();
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