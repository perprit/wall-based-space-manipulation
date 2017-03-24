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
        // the range of the hand position that is recognizable
        public float MinimumArmLength = 0.3f;
        public float MaximumArmLength = 0.7f;

        public bool GenerateInitialWallObject = true;

        // variables for wall objects currently being dragged
        private IInputSource wallInputSource = null;
        private uint wallInputSourceId;
        private GameObject currentWallObject = null;
        private GameObject initialWallObject = null;
        private bool isDraggingWall = false;

        // variables for items (virtual objects) currently being dragged
        private IInputSource itemInputSource = null;
        private uint itemInputSourceId;
        private GameObject currentItemObject = null;
        private bool isDraggingItem = false;

        private Camera mainCamera;
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
                Vector3 wallRefCameraPosition = initialWallObject.transform.InverseTransformPoint(mainCamera.transform.position);
                wallRefCameraPosition = Vector3.Scale(wallRefCameraPosition, new Vector3(1, 1, 0));
                wallRefCameraPosition = initialWallObject.transform.TransformPoint(wallRefCameraPosition);
                float wallCameraDistance = Vector3.Magnitude(wallRefCameraPosition - mainCamera.transform.position);
                DebugTextController.Instance.SetMessage(wallCameraDistance);
                //Vector3 wallRefCameraPosition = 
                // TODO calculate shortest distance between camera and wall, then rescale distance between every objects on the range of (1 ~ distance)
            }

            if (!isDraggingWall && initialWallObject != null)
            {
                Destroy(initialWallObject);
            }
        }

        public void StartReposition(IInputSource source, uint sourceId, GameObject obj, DraggableType type)
        {
            if (type == DraggableType.Wall)
            {
                if (isDraggingWall)
                {
                    Debug.Log("Is dragging a wall already, StartReposition()/RepositionManager");
                    return;
                }

                wallInputSource = source;
                wallInputSourceId = sourceId;

                currentWallObject = obj;
                // for indicating and saving initial wall position
                initialWallObject = Instantiate(currentWallObject);

                if (!GenerateInitialWallObject)
                {
                    initialWallObject.GetComponent<MeshRenderer>().enabled = false;
                    initialWallObject.GetComponent<MeshCollider>().sharedMesh = null;
                }

                isDraggingWall = true;
            }

            if (type == DraggableType.Item)
            {
                if (isDraggingItem)
                {
                    Debug.Log("Is dragging an item already, StartReposition()/RepositionManager");
                    return;
                }

                itemInputSource = source;
                itemInputSourceId = sourceId;

                currentItemObject = obj;

                isDraggingItem = true;
            }
        }

        public void StopReposition(DraggableType type)
        {
            if (type == DraggableType.Wall)
            {
                isDraggingWall = false;
            }

            if (type == DraggableType.Item)
            {
                isDraggingItem = false;
            }
        }
    }
}

