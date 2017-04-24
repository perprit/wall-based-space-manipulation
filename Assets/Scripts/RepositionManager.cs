using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity;
using HoloToolkit.Unity.SpatialMapping;
using ManipulateWalls;

namespace HoloToolkit.Unity.InputModule
{
    public enum DraggableType
    {
        WALL, ITEM
    }

    /// <summary>
    /// Component that manages every aspects of movement of objects (which means, wall and object) with hands.
    /// Assumes that the user manipulates a wall in one hand and an virutal object in the other hand (irrelevant to hand dominance)
    /// TODO Every logic on position changes of objects needs to be migrated from HandDraggableWall and HandDraggableItem classes.
    /// </summary>
    public class RepositionManager : Singleton<RepositionManager>
    {
        // the range of the hand position that is recognizable
        public float MinimumArmLength = 0.3f;
        public float MaximumArmLength = 0.55f;   // MaximumArmLength must be bigger than MinimumArmLength!
        public float MinimumDistanceToWall = 1.5f;
        public float DefaultMovementScale = 5f;
        public float SmoothingRatio = 0.5f;

        public bool ShowInitialWallObject = false;
        public bool GlobalRepositionEveryObject = true;

        private Camera mainCamera;

        private Dictionary<int, WallStatus> wallStatusDic = new Dictionary<int, WallStatus>();
        private bool isWallAvailable = false;

        // variables for items (virtual objects) currently being dragged
        private GameObject currentItemObject = null;
        private bool isMovingItem = false;
        private Dictionary<int, ItemStatus> itemStatusDic = new Dictionary<int, ItemStatus>();

        void Start()
        {
            mainCamera = Camera.main;
            //currentItemsTransforms = VirtualItemsManager.Instance.GetAllObjectTransforms();
            SurfaceMeshesToPlanes.Instance.MakePlanesComplete += SurfaceMeshesToPlanes_MakePlanesComplete;
        }
        
        void Update()
        {
            // just pass by update logics if planes are not detected yet
            if (!isWallAvailable)
            {
                return;
            }

            // init wallStatusKeys with WallStatus objects that are dragging or locked
            List<int> wallStatusKeys = new List<int>(wallStatusDic.Keys);

            foreach (int wallStatusId in wallStatusKeys)
            {
                WallStatus wallStatus = wallStatusDic[wallStatusId];

                if (wallStatus.mode == RepositionModes.IDLE)
                {
                    continue;
                }
                
                if ((wallStatus.mode == RepositionModes.DRAGGING || wallStatus.mode == RepositionModes.LOCKED) && wallStatus.initObj != null)
                {
                    // calculate initialDistanceToWall and wallMovementScale
                    Vector3 initWallProjectedCameraPos = wallStatus.initObj.transform.InverseTransformPoint(mainCamera.transform.position);
                    initWallProjectedCameraPos = Vector3.Scale(initWallProjectedCameraPos, new Vector3(1, 1, 0));
                    initWallProjectedCameraPos = wallStatus.initObj.transform.TransformPoint(initWallProjectedCameraPos);
                    float cameraDistanceToInitWall = Vector3.Magnitude(initWallProjectedCameraPos - mainCamera.transform.position);

                    // calculate current distance to wall, by projecting camera position into the wall in wall's z-axis
                    Vector3 wallProjectedCameraPosition = wallStatus.obj.transform.InverseTransformPoint(mainCamera.transform.position);
                    wallProjectedCameraPosition = Vector3.Scale(wallProjectedCameraPosition, new Vector3(1, 1, 0));
                    wallProjectedCameraPosition = wallStatus.obj.transform.TransformPoint(wallProjectedCameraPosition);
                    float cameraDistanceToWall = Vector3.Magnitude(wallProjectedCameraPosition - mainCamera.transform.position);

                    // the scale of wall movement 
                    wallStatus.movementScale = (cameraDistanceToInitWall - MinimumDistanceToWall) / (MaximumArmLength - MinimumArmLength);

                    if (GlobalRepositionEveryObject)
                    {
                        // the scale between initial wall and current wall
                        float distanceScale = cameraDistanceToWall / cameraDistanceToInitWall;    // TODO needs error handling for zero divide or something?

                        // reposition every items
                        List<int> itemStatusKeys = new List<int>(itemStatusDic.Keys);
                        foreach (int itemStatusId in itemStatusKeys)
                        {
                            ItemStatus itemStatus = itemStatusDic[itemStatusId];

                            // the items not between the camera and the initial wall must not be affected
                            if (cameraDistanceToInitWall < itemStatus.distanceToWallsDic[wallStatus.obj.GetInstanceID()])
                            {
                                continue;
                            }
                            
                            // initial position of the item currently being dragged is recalculated in real time
                            if (isMovingItem && currentItemObject != null && itemStatus.obj.GetInstanceID() == currentItemObject.GetInstanceID())
                            {
                                Vector3 initWallRefItemPos = wallStatus.initObj.transform.InverseTransformPoint(itemStatus.obj.transform.position);
                                Vector3 initWallRefCameraPos = wallStatus.initObj.transform.InverseTransformPoint(mainCamera.transform.position);
                                Vector3 cameraToItemDirection = initWallRefItemPos - initWallRefCameraPos;
                                cameraToItemDirection = Vector3.Scale(cameraToItemDirection, new Vector3(1, 1, 1 / distanceScale));
                                initWallRefItemPos = initWallRefCameraPos + cameraToItemDirection;
                                initWallRefItemPos = wallStatus.initObj.transform.TransformPoint(initWallRefItemPos);
                                // TODO below for handling multiple walls
                                itemStatus.initPos = initWallRefItemPos;
                            }
                            // otherwise, in case of the other items
                            else
                            {
                                Vector3 initWallRefItemPos = wallStatus.initObj.transform.InverseTransformPoint(itemStatus.initPos);
                                Vector3 initWallRefCameraPos = wallStatus.initObj.transform.InverseTransformPoint(mainCamera.transform.position);
                                Vector3 cameraToItemDirection = initWallRefItemPos - initWallRefCameraPos;
                                cameraToItemDirection = Vector3.Scale(cameraToItemDirection, new Vector3(1, 1, distanceScale));
                                initWallRefItemPos = initWallRefCameraPos + cameraToItemDirection;
                                initWallRefItemPos = wallStatus.initObj.transform.TransformPoint(initWallRefItemPos);
                                // TODO below for handling multiple walls
                                itemStatus.obj.transform.position = initWallRefItemPos;
                            }
                            itemStatusDic[itemStatusId] = itemStatus;
                        }
                    }
                }
                wallStatusDic[wallStatusId] = wallStatus;
            }
        }

        private void SurfaceMeshesToPlanes_MakePlanesComplete(object source, System.EventArgs args)
        {
            // initialize 
            List<GameObject> planes = SurfaceMeshesToPlanes.Instance.GetActivePlanes(PlaneTypes.Ceiling | PlaneTypes.Floor | PlaneTypes.Table | PlaneTypes.Wall);
            for (int i=0; i<planes.Count; i++)
            {
                WallStatus wallStatus = new WallStatus(planes[i]);
                wallStatusDic.Add(planes[i].GetInstanceID(), wallStatus);
            }
            isWallAvailable = true;

            // initialize itemStatusDic
            List<GameObject> items = VirtualItemsManager.Instance.GetItemObjects();
            for (int i = 0; i < items.Count; i++)
            {
                ItemStatus itemStatus = new ItemStatus(items[i]);
                itemStatus.initPos = items[i].transform.position;

                // initialize distanceToWallsDic (distance to each walls)
                foreach (KeyValuePair<int, WallStatus> entry in wallStatusDic)
                {
                    int wallStatusId = entry.Key;
                    WallStatus wallStatus = entry.Value;

                    // calculate initial distance to wall, by projecting the item's position into the wall in wall's z-axis
                    Vector3 wallProjectedItemPosition = wallStatus.obj.transform.InverseTransformPoint(items[i].transform.position);
                    wallProjectedItemPosition = Vector3.Scale(wallProjectedItemPosition, new Vector3(1, 1, 0));
                    wallProjectedItemPosition = wallStatus.obj.transform.TransformPoint(wallProjectedItemPosition);
                    float itemDistanceToWall = Vector3.Magnitude(wallProjectedItemPosition - items[i].transform.position);

                    itemStatus.distanceToWallsDic.Add(wallStatusId, itemDistanceToWall);
                }
                itemStatusDic.Add(items[i].GetInstanceID(), itemStatus);
            }
        }

        public void StartReposition(GameObject obj, DraggableType type)
        {
            if (type == DraggableType.WALL)
            {
                if (MaximumArmLength < MinimumArmLength)
                {
                    Debug.Log("MaximumArmLength < MinimumArmLength. StartReposition()/RepositionManager");
                    return;
                }

                WallStatus wallStatus;

                if(!wallStatusDic.TryGetValue(obj.GetInstanceID(), out wallStatus))
                {
                    Debug.Log("GameObject " + obj.GetInstanceID() + " doesn't exist");
                    return;
                }

                wallStatus.obj = obj;
                wallStatus.initObj = Instantiate(obj);
                if (!ShowInitialWallObject)
                {
                    wallStatus.initObj.GetComponent<MeshRenderer>().enabled = false;
                    wallStatus.initObj.GetComponent<MeshCollider>().sharedMesh = null;
                }
                wallStatus.mode = RepositionModes.DRAGGING;

                // assign updated wallStatus
                wallStatusDic[obj.GetInstanceID()] = wallStatus;
            }

            if (type == DraggableType.ITEM)
            {
                currentItemObject = obj;
                isMovingItem = true;
            }
        }

        public void StopReposition(GameObject obj, DraggableType type)
        {
            if (type == DraggableType.WALL)
            {
                WallStatus wallStatus;

                if (!wallStatusDic.TryGetValue(obj.GetInstanceID(), out wallStatus))
                {
                    Debug.Log("GameObject " + obj.GetInstanceID() + " doesn't exist");
                    return;
                }

                wallStatus.mode = RepositionModes.IDLE;
                wallStatus.movementScale = DefaultMovementScale;
                Destroy(wallStatus.initObj);
                wallStatusDic[obj.GetInstanceID()] = wallStatus;

                // restore items' position
                foreach (KeyValuePair<int, ItemStatus> entry in itemStatusDic)
                {
                    ItemStatus itemStatus = entry.Value;
                    itemStatus.obj.transform.position = itemStatus.initPos;
                }
            }

            if (type == DraggableType.ITEM)
            {
                currentItemObject = null;
                isMovingItem = false;

                ItemStatus itemStatus = itemStatusDic[obj.GetInstanceID()];
                itemStatus.initPos = obj.transform.position;
                itemStatusDic[obj.GetInstanceID()] = itemStatus;
            }
        }
        
        // TODO would be deprecated since reposition logic would be migrated to this class
        public float GetWallMovementScale(int instanceId)
        {
            return wallStatusDic[instanceId].movementScale;
        }
        
    }
}

