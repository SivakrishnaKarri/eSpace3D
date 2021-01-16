using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using IngameDebugConsole;

public class Scroll : MonoBehaviour
{
    public RectTransform consoleUI;
    public bool openConsole = true;
    public Button consoleButton;
    [SerializeField]
    private DebugLogManager debugManager;
    // Start is called before the first frame update
    void Start()
    {
        debugManager.ShowLogWindow();
        consoleButton.onClick.AddListener(ClickConsoleButton);
        Click();
    }
    private void OnDestroy()
    {
        consoleButton.onClick.RemoveListener(ClickConsoleButton);
    }

    public void ClickConsoleButton()
    {
        Click();
    }

    public void Click()
    {
        if (openConsole == false)
        {
            consoleUI.DOAnchorPos(new Vector2(0, -300), 0.25f);
            openConsole = true;
            
        }
        else
        {
            consoleUI.DOAnchorPos(new Vector2(0, 0), 0.25f);
            openConsole = false;
           
        }

    }
}
