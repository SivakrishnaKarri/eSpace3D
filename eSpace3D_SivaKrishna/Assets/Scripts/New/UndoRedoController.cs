using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class UndoRedoController : MonoBehaviour
{
    public GameObject testGameObject;
    public ObjectState before;
    public ObjectState after;
    // Start is called before the first frame update
    void Start()
    {
         before = new ObjectState(testGameObject);
            
    }

   public void Button1()
    {
         after = new ObjectState(testGameObject);
    }
    public void Button2()
    {
        UndoRedoClass.AddAction(new UndoableChange(before, after));
    }
}
