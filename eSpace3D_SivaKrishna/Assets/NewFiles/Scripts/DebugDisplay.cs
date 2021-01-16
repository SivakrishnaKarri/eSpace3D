using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;



    public class DebugDisplay : MonoBehaviour
    {
        public static DebugDisplay instance;
        public TextMeshProUGUI debugText;

        private void Awake()
        {
            instance = this;
        }
        // Start is called before the first frame update
        void Start()
        {
            debugText.text = "";
        }

        public void DisplayDebugMessage(string message)
        {
        debugText.gameObject.SetActive(true);
        debugText.text = message;
        StartCoroutine(DisableMessage());
        }

    IEnumerator DisableMessage()
    {
        yield return new WaitForSeconds(5f);
        debugText.gameObject.SetActive(false);
    }
    }


