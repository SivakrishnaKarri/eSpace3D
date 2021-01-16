/* Window example */

using UnityEngine;
using System.Collections;

public class UnderConstruction : MonoBehaviour 
{
         

	bool isUnderConstruction=false;
	
    
	
		 
    private Rect windowRect = new Rect (100, 100, 200, 100 );
    
    void OnGUI ()
    {
		if( ! isUnderConstruction ) return; 

        windowRect = GUI.Window (0, windowRect, WindowFunction, "");
    }
    
    void WindowFunction (int windowID) 
    {
        GUI.Button(new Rect(30, 40, 150, 40), "Under Construction ");
		
		GUI.DragWindow();
		
    }


	public void underConstruction() {
		print("UnderConstruction.underConstruction... In ...");
		isUnderConstruction= ! isUnderConstruction;
	}
	
	public void toggleUnderConstruction(bool toggle) {
		print("UnderConstruction.toggleUnderConstruction... In ...");
		if(toggle) {
			isUnderConstruction=true;
		} else {
			isUnderConstruction=false;
		}
	}

}