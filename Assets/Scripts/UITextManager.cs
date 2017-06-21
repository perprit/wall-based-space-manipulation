using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HoloToolkit.Unity;

namespace ManipulateWalls
{
    public class UITextManager : Singleton<UITextManager> {
        
        Text text;
        float lastPrintTime;
        float printDuration = 1f;

        // Use this for initialization
        void Start()
        {
            text = gameObject.GetComponent<Text>();
            lastPrintTime = Time.time;
        }

        void Update()
        {
            if (Time.time > lastPrintTime + printDuration)
            {
                text.text = "";
            }
        }

        public void PrintSuccess()
        {
            PrintMessage("success");
        }

        public void PrintMessage(string msg)
        {
            lastPrintTime = Time.time;
            text.text = msg;
        }
    }
}
