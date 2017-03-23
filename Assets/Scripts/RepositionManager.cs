using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity;
using ManipulateWalls;

namespace HoloToolkit.Unity.InputModule
{
    public enum DraggableType
    {
        Wall, Item
    }

    public class RepositionManager : Singleton<RepositionManager>
    {

        private IInputSource wallInputSource = null;
        private uint wallInputSourceId;
        private GameObject currentWallObject;
        private bool isDraggingWall = false;
        private Transform initialWallTransform; 

        private IInputSource itemInputSource = null;
        private uint itemInputSourceId;
        private GameObject currentItemObject;
        private bool isDraggingItem = false;

        private Camera mainCamera;

        public Vector3 wallInitialPosition;

        public float MinimumArmLength = 0f;
        public float MaximumArmLength = 1f;

        private float MinimumDistanceToWall = 1f;

        private List<Transform> itemsTransforms;

        // Use this for initialization
        void Start()
        {
            mainCamera = Camera.main;
            itemsTransforms = VirtualItemsManager.Instance.GetAllObjectTransforms();
        }

        // Update is called once per frame
        void Update()
        {
            if (isDraggingWall)
            {

            }
        }

        public void SetInputSource(IInputSource source, uint sourceId, DraggableType type)
        {
            if(type == DraggableType.Wall)
            {
                wallInputSource = source;
                wallInputSourceId = sourceId;
            }
            
            if (type == DraggableType.Item)
            {
                itemInputSource = source;
                itemInputSourceId = sourceId;
            }
        }

        public void SetDraggingStatus(bool isDragging, DraggableType type)
        {
            if (type == DraggableType.Wall)
            {
                isDraggingWall = isDragging;
            }

            if (type == DraggableType.Item)
            {
                isDraggingItem = isDragging;
            }
        }

        public void SetCurrentObject(GameObject obj, DraggableType type)
        {
            if (type == DraggableType.Wall)
            {
                currentWallObject = obj;
                initialWallTransform = obj.transform;
            }

            if (type == DraggableType.Item)
            {
                currentItemObject = obj;
            }
        }
    }
}

