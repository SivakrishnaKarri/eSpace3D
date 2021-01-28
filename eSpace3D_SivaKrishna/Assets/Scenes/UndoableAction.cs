using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UndoableAction : MonoBehaviour
{
    public UndoableActionType Type;
    public Object target;
    public object from;
    public object to;

    public UndoableAction(UndoableActionType type, Object target, object from, object to)
    {
        Type = type;
        this.target = target;
        this.from = from;
        this.to = to;
    }
}


public enum UndoableActionType
{
    Enable,
    SetActive,
    Position,

    // ... according to your needs
}