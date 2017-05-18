using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ManipulateWalls;

public class ItemIndicator : MonoBehaviour {

    private GameObject itemObj;
	// Use this for initialization
	void Start () {
        ExperimentManager.Instance.SetWallComplete += ExperimentManager_SetWallComplete;
	}

    private void ExperimentManager_SetWallComplete(object source, System.EventArgs args)
    {
        itemObj = ExperimentManager.Instance.GetItemObject();
    }
	
	// Update is called once per frame
	void Update () {
        if (itemObj != null)
        {
            //Vector3 relativePos = itemObj.transform.position - transform.position;
            //Quaternion rotation = Quaternion.LookRotation(relativePos);
            //Debug.Log(relativePos.ToString("F2") + ", " + rotation.eulerAngles.ToString("F2"));
            //transform.localRotation = rotation;
        }
	}
}
