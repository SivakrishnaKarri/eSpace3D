using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Linq;

public class UndoRedoController : MonoBehaviour
{
    public List<GameObject> testGameObject;
    public List<ObjectState> before;
    public List<ObjectState> after;
    // Start is called before the first frame update
    void Start()
    {
      //  testGameObject = new List<GameObject>();
        //before = new ObjectState(testGameObject);
        // before = new List<ObjectState>(testGameObject[0]);
        // before.Add(testGameObject.gameObject);
        before = testGameObject.Select(obj => new ObjectState(obj)).ToList();
    }

   public void Button1()
    {
        //  after = new ObjectState(testGameObject);
        after = testGameObject.Select(obj => new ObjectState(obj)).ToList();
    }
    public void Button2()
    {
        UndoRedoClass.AddAction(new UndoableChange(before, after));
    }
    public void Button3()
    {
        UndoRedoClass.Undo();
    }
}
