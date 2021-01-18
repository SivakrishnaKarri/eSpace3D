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


