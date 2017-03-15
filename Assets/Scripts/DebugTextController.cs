using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ManipulateWalls
{
    public class DebugTextController : MonoBehaviour
    {

        Text text;

        // Use this for initialization
        void Start()
        {
            text = gameObject.GetComponent<Text>();
        }

        void Update()
        {
        }

        public void SetMessage(string message, LogType type = LogType.Log)
        {
            text.text = message;
        }

        public void AddMessage(string message, LogType type = LogType.Log)
        {
            text.text += "\n" + message;
        }

        public void ClearMessage()
        {
            text.text = "";
        }
    }
}