using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using HighlightingSystem;




[RequireComponent(typeof(Highlighter))]
public class SelectionManager : MonoBehaviour
{
	string goname;
    Highlighter highlighter;
	GameObject[] selectableGOS;
	public GameObject selectedGO;
	GameObject prevSelectedGO, hitGO;
	string selectable="Selectable";
	Button selectButton, unselectButton, deleteButton;
	
	[SerializeField]
	TransformsManager transformsManager;
	
	enum Mode {None, Select, Unselect, Delete }; 
	Mode mode;

	
	void Start() {
		mode=Mode.None;
		selectButton=GameObject.Find("SelectButton").GetComponent<Button>();
		unselectButton=GameObject.Find("UnselectButton").GetComponent<Button>();
		deleteButton=GameObject.Find("DeleteButton").GetComponent<Button>();
		setButtonsState(null);
		selectableGOS = GameObject.FindGameObjectsWithTag(selectable);
		foreach(GameObject go in selectableGOS ) {
				goname=go.name;
				print("SelectionManager.Start() ... processing ... " + goname);
				highlighter=go.GetComponent<Highlighter>();
				if( highlighter == null ) {
						highlighter=go.AddComponent<Highlighter>();
				}
		}
	}

    // Update is called once per frame
	// Select Mode logic is in Update() 
    void Update()
    {
		if (mode != Mode.Select) return;
		if (Input.GetMouseButtonDown(0))
        {
			if (EventSystem.current.IsPointerOverGameObject() ) return;  //To prevent picking up clicks through UI 
			                                                             //Avoid / Detect Clicks Through your UI https://www.youtube.com/watch?v=rATAnkClkWU
            RaycastHit hit;
            //Send a ray from the camera to the mouseposition
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            //Create a raycast from the Camera and output anything it hits
            if (Physics.Raycast(ray, out hit)) {
                //Check the hit GameObject has a Collider
                if ( hit.collider != null )
                {
                    //Click a GameObject to return that GameObject your mouse pointer hit
                    hitGO = hit.collider.gameObject;
					print("SelectionManager.Update() ... Hit gameObject =   " + hitGO.name);
					if( selectable.Equals(hitGO.tag) ) {
						selectedGO=hitGO;
						highlighter=selectedGO.GetComponent<Highlighter>();
						if(highlighter != null) {
							highlighter.ConstantOn(Color.red);
							print("SelectionManager.Update() Highlighted gameObject = " + selectedGO.name);
							if (prevSelectedGO != selectedGO ) unHighlight(prevSelectedGO);
							prevSelectedGO=selectedGO;
						}
					} else {
						print("SelectionManager.Update() ... No Selectable tag for hit GameObject = " +  hitGO.name ); 
                        unSelectGOS();						
					}
				} else {
					print("SelectionManager.Update() ... hit.collider = null ");
					unSelectGOS();
				}
			} else {
				print("SelectionManager.Update() ... hit = null ");
				unSelectGOS();
			}
		}
        
    }
	
	private void unSelectGOS() {
		unHighlight(selectedGO);
		unHighlight(prevSelectedGO);
		selectedGO=null;
		prevSelectedGO=null;
	}
	
	private void setButtonsState(Button button) {
		selectButton.interactable=true;
		unselectButton.interactable=true;
		deleteButton.interactable=true;
		if (button == null ) {
			print("SelectionManager.setButtonsState() ... Select, Unselect, Delete buttons are in interactable state");
		} else {
			button.interactable=false;
			print("SelectionManager.setButtonsState()   " + button.name + " is not interactable");
		}
	}
	
	public void onSelect() {
		print("In onSelect() ...");
		mode=Mode.Select;
		setButtonsState(selectButton);
		if (transformsManager != null ) transformsManager.reset();
	}
	
	public void onUnselect() {
		print("SelectionManager.onUnselect() ... In onUnselect() ...");
		mode=Mode.Unselect;
		unSelectGOS();
		setButtonsState(unselectButton);
		if (transformsManager != null ) transformsManager.reset();
	}
	
	public void onDelete() {
		print("SelectionManager.onDelete() ... In onDelete() ...");
		mode=Mode.Delete;
		if ( selectedGO != null ) {
			Destroy(selectedGO);
		}
		setButtonsState(deleteButton);
	}
	
	private void unHighlight(GameObject go) {
		print("SelectionManager.unHighlight() ... In unHighlight() ... ");
		if (go != null) {
			print("SelectionManager.unHighlight() ... go.name = " + go.name);
			Highlighter highlighter=go.GetComponent<Highlighter>();
			if(highlighter != null) {
				highlighter.ConstantOffImmediate();
				print("SelectionManager.unHighlight() ... Highlight removed from gameObject = " + go.name);
			}
		}
	}
}
