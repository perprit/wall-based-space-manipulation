using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.SpatialMapping;

namespace HoloToolkit.Unity.InputModule
{
    public struct ItemStatus
    {
        public GameObject obj;
        //public GameObject initObj;
        public Vector3 initPos;
        //public bool isDragging;
        public Dictionary<int, float> distanceToWallsDic;

        public ItemStatus(GameObject obj_)
        {
            obj = obj_;
            initPos = Vector3.zero;
            distanceToWallsDic = new Dictionary<int, float>();
        }
    }
}