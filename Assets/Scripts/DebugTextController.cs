using UnityEngine;
using UnityEngine.UI;
using HoloToolkit.Unity;

namespace ManipulateWalls
{
    public class DebugTextController : Singleton<DebugTextController>
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

        public void SetMessage(float messageFloat, LogType type = LogType.Log)
        {
            text.text = messageFloat.ToString("F4");
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