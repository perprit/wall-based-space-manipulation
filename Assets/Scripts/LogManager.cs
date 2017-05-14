using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity;
using ManipulateWalls;


public class LogManager : Singleton<LogManager>
{
	void Start () {
	}
	
	void Update () {
		
	}

    public void SendLogMessage(string logMessage)
    {
        UDPLogManager.Instance.SendLogMessage(logMessage);
    }
}
