using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using SubjectNerd.Utilities;
using Microsoft.Win32;
using System.Collections.Generic;

public class MenuStrip : MonoBehaviour {

	public static MenuStrip instance = null;

	void Awake()
	{
		instance = this;
	}

	public GameObject itemPrefab;
	public GameObject itemsBg;
	public Transform mainParent;

    [HideInInspector]
    public string recentFilePath;

    [Reorderable("Menu Item", true, false)]
    public MenuObject[] mainMenu;

    public delegate void OnClickNew();
    public static event OnClickNew onClickNew;

    public delegate void OnClickExit();
    public static event OnClickExit onClickExit;

    public delegate void OnClickOpen();
    public static event OnClickOpen onClickOpen;

    public delegate void OnClickSave();
    public static event OnClickSave onClickSave;

    public delegate void OnClickSaveAs();
    public static event OnClickSaveAs onClickSaveAs;

    public delegate void OnClickUndo();
    public static event OnClickUndo onClickUndo;

    public delegate void OnClickRedo();
    public static event OnClickRedo onClickRedo;

    public delegate void OnClickImport();
    public static event OnClickImport onClickImport;


    IEnumerator Start()
	{
		foreach (MenuObject menuItem in mainMenu) {
			GameObject item = (GameObject)Instantiate (itemPrefab);
			item.transform.SetParent (transform);
			item.name = menuItem.name;
            item.transform.localScale = Vector3.one;
			item.GetComponentInChildren<Text> ().text = menuItem.name;
			yield return null;
			item.GetComponent<LayoutElement> ().preferredWidth = ((RectTransform)item.transform.GetChild (0)).sizeDelta.x + 20;
            item.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, item.GetComponent<LayoutElement>().preferredWidth);
            item.GetComponent<Item> ().subItems = menuItem.subItems;
		}

        //if (!ImpExManager.instance.HasCADPermission())
        //{
        //    mainMenu[2].subItems[2].name = "";
        //}

	}


	private bool isOnMenuItem = false;
	private bool hoverFlag = false;

    private bool dataPrep = false;

	public void Entered(GameObject enterObject)
	{
		if (hoverFlag)
			ActualLogic (enterObject);
	}

	private GameObject rootObject = null;

	void ActualLogic(GameObject enterObject)
	{
		isOnMenuItem = true;

		MenuObject[] subItems = enterObject.GetComponent<Item> ().subItems;

		if(subItems.Length > 0)
		{
			RectTransform enterObjectRect = enterObject.GetComponent<RectTransform> ();
			GameObject bg = (GameObject)Instantiate (itemsBg);
            Transform bgTransform = bg.transform;

            foreach (MenuObject itemName in subItems)
            {
                if (string.IsNullOrEmpty(itemName.name))
                    continue;
                GameObject itemClone = (GameObject)Instantiate(itemPrefab);
                itemClone.transform.SetParent(bgTransform);
                itemClone.name = itemName.name;
                itemClone.GetComponentInChildren<Text>().text = itemName.name;
                itemClone.GetComponent<Item>().isChild = true;
                itemClone.GetComponent<Item>().subItems = itemName.subItems;
                if (itemName.subItems.Length > 0)
                    itemClone.transform.Find("Arrow").gameObject.SetActive(true);
                else
                {
                    if (enterObject.name.Equals("Exporter") && itemName.name.Equals(PlayerPrefs.GetString("exporter"), System.StringComparison.CurrentCultureIgnoreCase) ||
                        enterObject.name.Equals("Environment") && itemName.name.Equals(PlayerPrefs.GetString("environment"), System.StringComparison.CurrentCultureIgnoreCase) ||
                        enterObject.name.Equals("Quality") && itemName.name.Equals(PlayerPrefs.GetString("quality"), System.StringComparison.CurrentCultureIgnoreCase) ||
                            (itemName.name == "Data Prep" && dataPrep))

                    {
                        itemClone.transform.Find("Selection").gameObject.SetActive(true);
                    }
                }

                /*if (itemName.name == "Undo")
                    itemClone.GetComponent<Button>().interactable = RuntimeUndo.CanUndo;
                else if (itemName.name == "Redo")
                    itemClone.GetComponent<Button>().interactable = RuntimeUndo.CanRedo;*/

                //yield return null;
                StartCoroutine(WidthSetter(itemClone));
            }

            if (enterObjectRect.parent.name == "MenuStrip")
            {
                if (rootObject)
					Destroy (rootObject);
				rootObject = bg;
			} else {
				enterObject.GetComponent<Item> ().subMenuObject = bg;
			}
            if (enterObject.GetComponent<Item>().isChild)
            {
                bgTransform.SetParent(enterObject.transform);
                bgTransform.position = enterObjectRect.position + new Vector3(enterObjectRect.sizeDelta.x / 2 * mainParent.lossyScale.x, enterObjectRect.sizeDelta.y / 2 * mainParent.lossyScale.y, 0);
            }
            else
            {
                bgTransform.SetParent(mainParent);
                bgTransform.position = enterObjectRect.position - new Vector3(enterObjectRect.sizeDelta.x / 2 * mainParent.lossyScale.x, enterObjectRect.sizeDelta.y / 2 * mainParent.lossyScale.y, 0);
            }

            if (bgTransform != null)
                bgTransform.localScale = Vector3.one;
        }
	}

    IEnumerator WidthSetter(GameObject itemClone)
    {
        yield return null;
        if (itemClone != null)
        {
            if (itemClone.GetComponent<LayoutElement>().preferredWidth < ((RectTransform)itemClone.transform.GetChild(0)).sizeDelta.x)
            {
                itemClone.GetComponent<LayoutElement>().preferredWidth = ((RectTransform)itemClone.transform.GetChild(0)).sizeDelta.x + 20;
                itemClone.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, itemClone.GetComponent<LayoutElement>().preferredWidth);
            }
        }
    }

	public void Exit(GameObject exitObject)
	{
		isOnMenuItem = false;
		if (exitObject.GetComponent<Item> ().subMenuObject)
			Destroy (exitObject.GetComponent<Item> ().subMenuObject);
	}

	public void Clicked(GameObject clickedObject)
	{
		if (!clickedObject.GetComponent<Item> ().isChild && !hoverFlag) {
			hoverFlag = true;
			ActualLogic (clickedObject);
		} else {
			if (rootObject != null && clickedObject.GetComponent<Item> ().subItems.Length == 0) {
				hoverFlag = false;
				Destroy (rootObject, 0.15f);
			}
		}
        //Debug.Log (clickedObject.name);

        StartCoroutine(DelayAction(clickedObject));
	}

    IEnumerator DelayAction(GameObject g)
    {
        string nameString = g.name;

        yield return new WaitForSeconds(0.2f);


        if (nameString == "New")
        {
            if (onClickNew != null)
                onClickNew();
        }
        else if (nameString == "Open")
        {
            if (onClickOpen != null)
                onClickOpen();
        }
        else if (nameString == "Exit")
        {
            if (onClickExit != null)
                onClickExit();
        }
        else if (nameString == "Save")
        {
            if (onClickSave != null)
                onClickSave();
        }
        else if (nameString == "Save as")
        {
            if (onClickSaveAs != null)
                onClickSaveAs();
        }
        else if (nameString == "Undo")
        {
            if (onClickUndo != null)
                onClickUndo();
        }
        else if (nameString == "Redo")
        {
            if (onClickRedo != null)
                onClickRedo();
        }
        else if (nameString == "Import")
        {
            if (onClickImport != null)
                onClickImport();
        }
    }

    void Update()
	{
		if (Input.GetMouseButtonDown (0)) {
			if (!isOnMenuItem) {
				hoverFlag = false;
				if (rootObject)
					Destroy (rootObject);
			}
		}
	}
}
