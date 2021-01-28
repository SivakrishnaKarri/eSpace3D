using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drag : MonoBehaviour
{
    private Vector3 ObjectInitialPosition;
    private Vector3 LastMousePosition;
    private Vector3 Delta;
    private Vector3 NewPosition;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnMouseDown()
    {
        ObjectInitialPosition = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        Delta = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, ObjectInitialPosition.z));
        LastMousePosition = Input.mousePosition;
    }

        void OnMouseDrag()
    {
        if (LastMousePosition != Input.mousePosition)
        {
            Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, ObjectInitialPosition.z);

            Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + Delta;

            transform.position = curPosition;

          //  Moved = true;

            LastMousePosition = Input.mousePosition;

            NewPosition = transform.position;
        }

    }
}
