using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.SpatialMapping;

namespace HoloToolkit.Unity.InputModule
{
    public enum WallStatusModes
    {
        IDLE, DRAGGING, LOCKED
    }

    public struct WallStatus
    {
        public GameObject obj;
        public GameObject initObj;
        public WallStatusModes mode;
        public float movementScale;
        public float distanceScale;
        public Vector3 cameraFrontWhenLocked;

        public WallStatus(GameObject obj_)
        {
            obj = obj_;
            initObj = null;
            mode = WallStatusModes.IDLE;
            movementScale = RepositionManager.Instance.DefaultMovementScale;
            distanceScale = 1f;
            cameraFrontWhenLocked = Vector3.zero;
        }
    }
}