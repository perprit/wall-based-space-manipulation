using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.SpatialMapping;

namespace HoloToolkit.Unity.InputModule
{
    public enum ItemStatusModes
    {
        DRAGGING, IDLE
    }

    public struct ItemStatus
    {
        public GameObject obj;
        //public GameObject initObj;
        public GameObject initObj;
        //public bool isDragging;
        public ItemStatusModes mode;

        public ItemStatus(GameObject obj_)
        {
            obj = obj_;
            initObj = null;
            mode = ItemStatusModes.IDLE;
        }
    }
}