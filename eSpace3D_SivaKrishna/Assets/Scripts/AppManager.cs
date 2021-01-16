using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppManager : MonoBehaviour
{
    [SerializeField]
    GameObject uiPanel;


    // Start is called before the first frame update
    IEnumerator Start()
    {
        Application.targetFrameRate = 60;

        //this refreshes UI to align UI properly
        uiPanel.SetActive(false);
        yield return null;
        uiPanel.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
