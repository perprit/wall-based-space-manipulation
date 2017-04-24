using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.SpatialMapping;

namespace HoloToolkit.Unity.InputModule
{
    public enum RepositionModes
    {
        IDLE, DRAGGING, LOCKED
    }

    public struct WallStatus
    {
        public GameObject obj;
        public GameObject initObj;
        public RepositionModes mode;
        public float movementScale;

        public WallStatus(GameObject obj_)
        {
            obj = obj_;
            initObj = null;
            mode = RepositionModes.IDLE;
            movementScale = RepositionManager.Instance.DefaultMovementScale;
        }
    }
}