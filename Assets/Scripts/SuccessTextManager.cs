using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using HoloToolkit.Unity;

namespace ManipulateWalls
{
    public class SuccessTextManager : Singleton<SuccessTextManager> {
        
        Text text;
        float lastPrintTime;

        // Use this for initialization
        void Start()
        {
            text = gameObject.GetComponent<Text>();
            lastPrintTime = Time.time;
        }

        void Update()
        {
            if (Time.time > lastPrintTime + 1f && text.text == "Success")
            {
                text.text = "";
            }
        }

        public void PrintSuccess()
        {
            lastPrintTime = Time.time;
            text.text = "Success";
        }
    }
}
