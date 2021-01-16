using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class ConsoleToggle : MonoBehaviour
{


	void Start() {
		print("In ConsoleToggle.Start() ...");
		consoleToggle(false);
		print("ConsoleToggle ... Starting scene with IngameDebugConsole's visibility turned off");
	}

	public void consoleToggle(bool toggle) {
			GameObject console=GameObject.Find("IngameDebugConsole");
			if(console == null ) {
				print("ConsoleToggle.consoleToggle() ... Could not find GameObject IngameDebugConsole");
				return;
			}
			Canvas canvas = console.GetComponent<Canvas>();
			if(canvas == null ) {
				print("ConsoleToggle.consoleToggle() ... Could not find Canvas component in GameObject IngameDebugConsole");
				return;
			}
			if(toggle) {
				canvas.enabled=true; 
				print("ConsoleToggle.consoleToggle() ... canvas enabled");
			} else { 
				canvas.enabled=false; 	
				print("ConsoleToggle.consoleToggle() ... canvas enabled");
			}
	}
}
