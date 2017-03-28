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

    /// <summary>
    /// Component that manages every aspects of movement of objects (which means, wall and object) with hands.
    /// Assumes that the user manipulates a wall in one hand and an virutal object in the other hand (irrelevant to hand dominance)
    /// TODO Every logic on position changes of objects needs to be migrated from HandDraggableWall and HandDraggableItem classes.
    /// </summary>
    public class RepositionManager : Singleton<RepositionManager>
    {
        // the range of the hand position that is recognizable
        public float MinimumArmLength = 0.4f;
        public float MaximumArmLength = 0.8f;   // MaximumArmLength must be bigger than MinimumArmLength!
        public float MinimumDistanceToWall = 1f;
        public float DefaultMovementScale = 2f;

        public bool GenerateInitialWallObject = true;
        public bool GlobalRepositionEveryObject = true;

        // variables for wall objects currently being dragged
        private IInputSource wallInputSource = null;
        private uint wallInputSourceId;
        private GameObject currentWallObject = null;
        private GameObject initialWallObject = null;
        private bool isDraggingWall = false;
        private float wallMovementScale;

        // variables for items (virtual objects) currently being dragged
        private IInputSource itemInputSource = null;
        private uint itemInputSourceId;
        private GameObject currentItemObject = null;
        private bool isDraggingItem = false;

        private Camera mainCamera;

        // TODO these 3 lists below that deal with items (virtual objects) should be encapsulated later
        // * For this time, we assume that items are NOT newly added or removed during the runtime
        private List<Transform> currentItemsTransforms; // WARNING changes on Transform would be applied to the the objects
        List<Vector3> initialItemsPositions = new List<Vector3>();  // item's initial status
        List<float> initialItemsDistances = new List<float>();

        void Start()
        {
            mainCamera = Camera.main;
            wallMovementScale = DefaultMovementScale;
            currentItemsTransforms = VirtualItemsManager.Instance.GetAllObjectTransforms();
        }
        
        void Update()
        {
            // destroy initialWallObject if not dragging now
            if (!isDraggingWall && initialWallObject != null)
            {
                Destroy(initialWallObject);
            }

            if (isDraggingWall && initialWallObject != null)
            {
                // calculate initialDistanceToWall and wallMovementScale
                Vector3 initialWallProjectedCameraPosition = initialWallObject.transform.InverseTransformPoint(mainCamera.transform.position);
                initialWallProjectedCameraPosition = Vector3.Scale(initialWallProjectedCameraPosition, new Vector3(1, 1, 0));
                initialWallProjectedCameraPosition = initialWallObject.transform.TransformPoint(initialWallProjectedCameraPosition);
                float cameraDistanceToInitialWall = Vector3.Magnitude(initialWallProjectedCameraPosition - mainCamera.transform.position);

                wallMovementScale = cameraDistanceToInitialWall > MinimumDistanceToWall ?
                                (cameraDistanceToInitialWall - MinimumDistanceToWall) / (MaximumArmLength - MinimumArmLength)
                                : DefaultMovementScale;

                // calculate current distance to wall, by projecting camera position into the wall in wall's z-axis
                Vector3 wallProjectedCameraPosition = currentWallObject.transform.InverseTransformPoint(mainCamera.transform.position);
                wallProjectedCameraPosition = Vector3.Scale(wallProjectedCameraPosition, new Vector3(1, 1, 0));
                wallProjectedCameraPosition = currentWallObject.transform.TransformPoint(wallProjectedCameraPosition);
                float cameraDistanceToWall = Vector3.Magnitude(wallProjectedCameraPosition - mainCamera.transform.position);

                float distanceScale = cameraDistanceToWall / cameraDistanceToInitialWall;    // TODO needs error handling for zero divide or something?

                if(GlobalRepositionEveryObject)
                {
                    currentItemsTransforms = VirtualItemsManager.Instance.GetAllObjectTransforms();
                    for (int i=0; i<currentItemsTransforms.Count; i++)
                    {
                        // the item currently being dragged must be ignored in the global reposition
                        if (isDraggingItem && currentItemObject != null && currentItemsTransforms[i].GetInstanceID() == currentItemObject.transform.GetInstanceID())
                        {
                            continue;
                        }

                        // the items not between the camera and the initial wall must not be affected
                        if (cameraDistanceToInitialWall < initialItemsDistances[i])
                        {
                            continue;
                        }
                        
                        Vector3 initialWallRefItemPosition = initialWallObject.transform.InverseTransformPoint(initialItemsPositions[i]);
                        Vector3 initialWallRefCameraPosition = initialWallObject.transform.InverseTransformPoint(mainCamera.transform.position);
                        Vector3 cameraToItemDirection = initialWallRefItemPosition - initialWallRefCameraPosition;
                        cameraToItemDirection = Vector3.Scale(cameraToItemDirection, new Vector3(1, 1, distanceScale));
                        initialWallRefItemPosition = initialWallRefCameraPosition + cameraToItemDirection;
                        initialWallRefItemPosition = initialWallObject.transform.TransformPoint(initialWallRefItemPosition);
                        currentItemsTransforms[i].position = initialWallRefItemPosition;
                    }
                }
            }
        }

        public void StartReposition(IInputSource source, uint sourceId, GameObject obj, DraggableType type)
        {
            if (type == DraggableType.Wall)
            {
                if (isDraggingWall)
                {
                    Debug.Log("Is dragging a wall already, just pass over the call. StartReposition()/RepositionManager");
                    return;
                }

                if (MaximumArmLength < MinimumArmLength)
                {
                    Debug.Log("MaximumArmLength < MinimumArmLength, just pass over the call. StartReposition()/RepositionManager");
                    return;
                }

                wallInputSource = source;
                wallInputSourceId = sourceId;

                // initialize currentWallObject
                currentWallObject = obj;

                // for indicating initial wall position
                initialWallObject = Instantiate(currentWallObject);

                if (!GenerateInitialWallObject)
                {
                    initialWallObject.GetComponent<MeshRenderer>().enabled = false;
                    initialWallObject.GetComponent<MeshCollider>().sharedMesh = null;
                }

                // save items initial positions and distances to the wall (which would be used for retoration)
                // TODO encapsulate position and distance
                initialItemsPositions = VirtualItemsManager.Instance.GetAllObjectPositions();
                initialItemsDistances.Clear();

                for (int i = 0; i < initialItemsPositions.Count; i++)
                {
                    // calculate initial distance to wall, by projecting the item's position into the wall in wall's z-axis
                    Vector3 wallProjectedItemPosition = currentWallObject.transform.InverseTransformPoint(initialItemsPositions[i]);
                    wallProjectedItemPosition = Vector3.Scale(wallProjectedItemPosition, new Vector3(1, 1, 0));
                    wallProjectedItemPosition = currentWallObject.transform.TransformPoint(wallProjectedItemPosition);
                    float itemDistanceToWall = Vector3.Magnitude(wallProjectedItemPosition - initialItemsPositions[i]);

                    initialItemsDistances.Add(itemDistanceToWall);
                }

                isDraggingWall = true;
            }

            if (type == DraggableType.Item)
            {
                if (isDraggingItem)
                {
                    Debug.Log("Is dragging an item already, just pass over the call. StartReposition()/RepositionManager");
                    return;
                }

                itemInputSource = source;
                itemInputSourceId = sourceId;

                currentItemObject = obj;

                isDraggingItem = true;
            }
        }

        public void StopReposition(uint sourceId, DraggableType type)
        {
            if (sourceId == wallInputSourceId && type == DraggableType.Wall)
            {
                isDraggingWall = false;
                wallMovementScale = DefaultMovementScale;

                // restore items' position
                for (int i=0; i<currentItemsTransforms.Count; i++)
                {
                    currentItemsTransforms[i].position = initialItemsPositions[i];
                }
            }

            if (sourceId == itemInputSourceId && type == DraggableType.Item)
            {
                isDraggingItem = false;
            }
        }

        // TODO would be deprecated since reposition logic would be migrated to this class
        public float GetWallMovementScale()
        {
            return wallMovementScale;
        }
    }
}

