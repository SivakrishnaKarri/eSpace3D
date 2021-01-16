using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;



    public class DebugTexDisplay : MonoBehaviour
    {
        public static DebugTexDisplay instance;
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
    }


