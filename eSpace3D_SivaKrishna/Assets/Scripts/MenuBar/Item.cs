using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Item : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

	public bool isChild = false;

	public MenuObject[] subItems;

	public GameObject subMenuObject;

	public void OnPointerEnter(PointerEventData data)
	{
		GetComponent<Image> ().color = new Color (0.2f, 0.2f, 0.2f, 1);
		GetComponentInChildren<Text> ().color = Color.white;
		MenuStrip.instance.Entered (gameObject);
	}

	public void OnPointerExit(PointerEventData data)
	{
		GetComponent<Image> ().color = Color.white;
		GetComponentInChildren<Text> ().color = new Color (0.2f, 0.2f, 0.2f, 1);
		MenuStrip.instance.Exit (gameObject);
	}

	public void OnPointerClick(PointerEventData data)
	{
		MenuStrip.instance.Clicked (gameObject);
	}
}
