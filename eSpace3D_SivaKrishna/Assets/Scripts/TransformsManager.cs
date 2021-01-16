using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using mattatz.TransformControl;
using HighlightingSystem;



[RequireComponent(typeof(Highlighter))]
public class TransformsManager : MonoBehaviour
{
	

	
	GameObject[] selectableGOS;
	GameObject selectedGO;
	TransformControl transformControl;
	enum Mode {None, Translate, Rotate, Scale};
	Mode mode;

	
	
	[SerializeField]
	SelectionManager selectionManager;
	
	
	
	
	
    // Start is called before the first frame update
    void Start()
    {
        selectableGOS = GameObject.FindGameObjectsWithTag("Selectable");  
			//returns GameObject[]. Re-using tag "Selectable" for "transforms" also since a GameObject can have only one tag
		foreach (GameObject go in selectableGOS) {
			transformControl=go.GetComponent<TransformControl>();
			if( transformControl == null ) {
					transformControl=go.AddComponent<TransformControl>();
			}
			transformControl.enabled = false; //reset. 
		}
    }

    // Update is called once per frame
    void Update()
    {
		if (mode == Mode.None) { return; }
		if (EventSystem.current.IsPointerOverGameObject() ) return; 
		
		//selectedGO=m_EventSystem.currentSelectedGameObject;
		selectedGO=selectionManager.selectedGO;
		if(selectedGO == null ) {
			print("TransformsManager.Update() ... Nothing selected");
			return;
		}
		
		
		//if (selectionManager != null) selectionManager.enabled=false; 
		
		if (selectedGO != null  ){
				print("TransformsManager.Update() ... selectedGO.name=" + selectedGO.name);
				transformControl=selectedGO.GetComponent<TransformControl>();
				//print(transformControl.ToString());
				if (transformControl == null )  {
					print("TransformsManager.Update() ... transformsControl is null for " + selectedGO.name );
				} else {
					transformControl.enabled=true;
					if (mode == Mode.Translate) 
						transformControl.mode = TransformControl.TransformMode.Translate;
					else if (mode == Mode.Rotate) 
						transformControl.mode = TransformControl.TransformMode.Rotate;
					else if (mode == Mode.Scale) 
						transformControl.mode = TransformControl.TransformMode.Scale;
					if (selectionManager != null) selectionManager.enabled=false;  // To ensure object detection by camera is in transformControl's hands. Otherwise confusion.
					transformControl.Control();
				}
			}
        
    }
	
		public void OnModeChanged(int index) {
			switch(index) {
			case 0:
				reset();
				break;
			case 1:
				move();
				break;
			case 2:
				rotate();
				break;
			case 3:
				scale();
				break;
			case 4:
				reset();
				break;
			}
		}
	
		public void move() {
			print("In TranformsManager.move() ");
			mode=Mode.Translate;
		}
		public void rotate() {
			print("In TransformsManager.rotate() ");
			mode=Mode.Rotate;
		}
		public void scale() {
			print("In TransformsManager.scale() ");
			mode=Mode.Scale;
		}
		
		public void reset() {
			print("In TransformManager.reset() ...");
			mode=Mode.None;
			if (selectedGO != null ) {
				Highlighter highlighter=selectedGO.GetComponent<Highlighter>();
				if(highlighter != null ) {
					highlighter.ConstantOffImmediate();
					print("TransformsManager.reset() ... Disabled highlighter for selected GameObject = " + selectedGO.name);
				}
				TransformControl transformControl=selectedGO.GetComponent<TransformControl>();
				if (transformControl != null ) {
					transformControl.mode = TransformControl.TransformMode.None;
					print("TransformManager.reset() ...  Set TransformControl component to TransformMode.None for GameObject = " + selectedGO);
				}
			}
			
			if (selectionManager != null ) selectionManager.enabled=true;
		DebugDisplay.instance.DisplayDebugMessage("Reset clicked");
		Debug.Log("Reset clicked");
		}
	
}
