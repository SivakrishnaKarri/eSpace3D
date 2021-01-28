using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UndoRedoSystem : MonoBehaviour
{
    private Stack<UndoableAction> availableUndos = new Stack<UndoableAction>();
    private Stack<UndoableAction> availableRedos = new Stack<UndoableAction>();

    public void TrackChangeAction(UndoableActionType type, UnityEngine.Object target, object from, object to)
    {
        // if you change something redo is cleared
        availableRedos.Clear();

        // Add this action do undoable actions
        availableUndos.Push(new UndoableAction(type, target, from, to));
    }

    public void Redo()
    {
        if (availableRedos.Count == 0) return;

        // get latest entry added to available Redos
        var redo = availableRedos.Pop();

        switch (redo.Type)
        {
         
            case UndoableActionType.SetActive:
                ((GameObject)redo.target).SetActive((bool)redo.to);
                break;

            case UndoableActionType.Position:
                ((Transform)redo.target).position = (Vector3)redo.to;
                break;

                // ... According to your needs 
        }

        // finally this is now a new undoable action
        availableUndos.Push(redo);
    }

    public void Undo()
    {
        if (availableUndos.Count == 0) return;

        // get latest entry added to available Undo
        var undo = availableUndos.Pop();

        switch (undo.Type)
        {
           
            case UndoableActionType.SetActive:
                ((GameObject)undo.target).SetActive((bool)undo.from);
                break;

            case UndoableActionType.Position:
                ((Transform)undo.target).position = (Vector3)undo.from;
                break;

                // ... According to your needs 
        }

        // finally this is now a new  redoable action
        availableRedos.Push(undo);
    }
}