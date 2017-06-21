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

    public enum ItemType
    {
        ITEM, TARGET
    }

    public struct ItemStatus
    {
        public GameObject obj;
        //public GameObject initObj;
        public GameObject initObj;
        //public bool isDragging;
        public ItemStatusModes mode;
        public ItemType type;

        public ItemStatus(GameObject obj_)
        {
            obj = obj_;
            initObj = null;
            mode = ItemStatusModes.IDLE;
            type = ItemType.ITEM;
        }

        public ItemStatus(GameObject obj_, ItemType type_)
        {
            obj = obj_;
            initObj = null;
            mode = ItemStatusModes.IDLE;
            type = type_;
        }
    }
}