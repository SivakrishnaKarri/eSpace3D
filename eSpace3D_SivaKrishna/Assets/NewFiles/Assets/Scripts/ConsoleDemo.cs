using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConsoleDemo : MonoBehaviour
{
    public Button exitBtn;
    public Button warningButton;
    public Button debugButton;
    public Button errorButton;

    // Start is called before the first frame update
    void Start()
    {
        exitBtn.onClick.AddListener(CloseApp);
        debugButton.onClick.AddListener(DebugButtonClicked);
        warningButton.onClick.AddListener(WarningButtonClicked);
        errorButton.onClick.AddListener(ErrorButtonClicked);
    }
    private void OnDestroy()
    {
        exitBtn.onClick.RemoveListener(CloseApp);
        debugButton.onClick.RemoveListener(DebugButtonClicked);
        warningButton.onClick.RemoveListener(WarningButtonClicked);
        errorButton.onClick.RemoveListener(ErrorButtonClicked);
    }

    private void WarningButtonClicked()
    {
        Debug.LogWarning("Warning button Clicked");
    }
    private void DebugButtonClicked()
    {
        Debug.Log("Informarion button Clicked");
    }
    private void ErrorButtonClicked()
    {
        Debug.LogError("Error button Clicked");
    }
    public void CloseApp()
    {
        Application.Quit();
    }  
}
