using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;




public class Navigation : MonoBehaviour 
{	
	Camera cam;
	Button zoomButton;
	Button panButton;
	Button rotateButton;
	Button resetButton;
	
	bool isZooming;
	bool isPanning;
	bool isRotating;
	bool isResetting;
	
	Vector3 touchStart;
	float camOriginalFOV;
	Vector3 camOriginalPosition;
	Quaternion camOriginalRotation;
	
	
	 // Start is called before the first frame update
	void Start() {
		cam = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
		//cam = Camera.current;
		//cam = Camera.current.GetComponent<Camera>();
		camOriginalFOV = cam.fieldOfView;
		camOriginalPosition = cam.transform.position;
		camOriginalRotation = cam.transform.rotation;
		zoomButton = GameObject.Find("ZoomButton").GetComponent<Button>();
		panButton = GameObject.Find("PanButton").GetComponent<Button>();
		rotateButton = GameObject.Find("RotateButton").GetComponent<Button>();
		resetButton = GameObject.Find("ResetButton").GetComponent<Button>();
		resetInternal();
	}
	
	
	void resetInternal() {
		zoomButton.interactable = true;
		panButton.interactable = true;
		rotateButton.interactable = true;
		resetButton.interactable = true;
		isZooming = false;
		isPanning = false;
		isRotating = false;
		isResetting = false;
	}
	
	public void zoomEnabled() {
		print("Navigation.zooomEnabled() ... In  ");
		resetInternal();
		zoomButton.interactable=false;
		isZooming=true;
	}
	
	public void panEnabled() {
		print("Navigation.panEnabled() ... In");
		resetInternal();
		panButton.interactable=false;
		isPanning=true;
	}
	
	public void rotateEnabled() {
		print("Navigation.rotateEnabled() ... In");
		resetInternal();
		rotateButton.interactable=false;
		isRotating=true;
	}
	
	
	public void resetEnabled() {
		print("Navigation.resetEnabled() ... In");
		resetInternal();
		cam.transform.position = camOriginalPosition;
		cam.transform.rotation = camOriginalRotation;
		cam.fieldOfView = camOriginalFOV;
	}
	Vector3 GetWorldPosition(){
		Ray mousePos = cam.ScreenPointToRay(Input.mousePosition);
		Plane ground = new Plane(Vector3.forward, new Vector3(0,0,0));
		float distance;
		ground.Raycast(mousePos, out distance);
		return mousePos.GetPoint(distance);
   }
	// Update is called once per frame
	void Update() {
		//print("In Update() ");
		//print("isZooming= " +isZooming);
		if (isZooming) {
			if (Input.GetAxis("Mouse ScrollWheel") > 0 ) {
				cam.GetComponent<Camera>().fieldOfView -=1;
				//Camera.fieldOfView - The field of view of the camera in degrees.
		    }
			if (Input.GetAxis("Mouse ScrollWheel") < 0 ) {
				cam.GetComponent<Camera>().fieldOfView +=1;
			}
			if(Input.GetKey(KeyCode.UpArrow)){
				cam.GetComponent<Camera>().fieldOfView -=1;	
			}
			if(Input.GetKey(KeyCode.DownArrow)){
				cam.GetComponent<Camera>().fieldOfView +=1;
			}
		}
		if (isPanning) {
			if (Input.GetMouseButtonDown(0)){
				touchStart = GetWorldPosition();
            }
			if (Input.GetMouseButton(0)){
				Vector3 direction = touchStart - GetWorldPosition();
				cam.transform.position += direction;
			}
		}
		if (isRotating) {
				if (Input.GetMouseButtonDown(0)){
					touchStart = GetWorldPosition();
                }
				if (Input.GetMouseButton(0)){
					Vector3 direction = touchStart - GetWorldPosition();
					//cam.transform.Rotate(0.0f, direction.x * 0.1f, 0.0f);
					//cam.transform.Rotate(Vector3.up, -Input.GetAxisRaw("Mouse X") * 0.2f, Space.World);
					cam.transform.RotateAround(Vector3.zero, Vector3.up, direction.x * 0.5f );
				}	
		}
		
	}
}