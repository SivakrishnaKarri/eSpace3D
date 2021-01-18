						In Game Debug Console Documentation
                                                ====================================

Please follow below steps to integrate In game Debug Console plugin : 
====================================================================

Remove the old plugin and import the new IngameDebugConsole Plugin.
Design has changed and added a few game objects as per requirement in “IngameDebugConsole_NewUI” prefab.
Drag and drop “IngameDebugConsole_NewUI” gameobject in hierarchy.
Create a new script named as DebugDisplay.cs and add below code.

DebugDisplay.cs :

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

    public class DebugDisplay : MonoBehaviour
    {   
    public static DebugDisplay instance;
    public static Action<string, Sprite> OnDebugMessageDisplayed;
    public Text debugText;
    public Image iconImage;
    public GameObject DebugParent;

    private void Awake()
        {
            instance = this;
        	OnDebugMessageDisplayed += DebugMessageDisplayed;
        	DebugParent.SetActive(false);
        }

    private void OnDestroy()
    {
        OnDebugMessageDisplayed -= DebugMessageDisplayed;
    }
    // Start is called before the first frame update
    void Start()
        {
            debugText.text = "";       
        }
    public void DebugMessageDisplayed(string message, Sprite icon)
    {
        DebugParent.SetActive(true);
        debugText.gameObject.SetActive(true);
         debugText.text = message;
         iconImage.sprite = icon;
        // StartCoroutine(DisableMessage());
    }
   
    IEnumerator DisableMessage()
        {
            yield return new WaitForSeconds(5f);
            debugText.gameObject.SetActive(false);
        }    
    }



Attach DebugDisplay.cs script in IngameDebugConsole_NewUI gameobject.

Open DebugLogItem.cs script and add below lines in respected lines.

Line no : 90   	public string DebuMessage; 
Line no : 144  	DebuMessage = isExpanded ? logEntry.ToString() : logEntry.logString;
Line No : 146  	DebugDisplay.OnDebugMessageDisplayed?.Invoke(DebuMessage, logEntry.logTypeSpriteRepresentation);
