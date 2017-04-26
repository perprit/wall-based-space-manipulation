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
        public float NearClippingPlaneDist = 0.85f;

        public bool ShowInitialWallObject = false;

        public bool IsWallAvailable = false;

        private Camera mainCamera;

        private Dictionary<int, WallStatus> wallStatusDic = new Dictionary<int, WallStatus>();
        
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
            if (!IsWallAvailable)
            {
                return;
            }

            // for each items,
            List<int> itemStatusKeys = new List<int>(itemStatusDic.Keys);
            foreach (int itemStatusId in itemStatusKeys)
            {
                ItemStatus itemStatus = itemStatusDic[itemStatusId];
                
                Vector3 itemPosChange = Vector3.zero;
                Vector3 itemInitPosChange = Vector3.zero;

                // calculate obj.transform.position and initPos of itemStatus based on each walls
                List<int> wallStatusKeys = new List<int>(wallStatusDic.Keys);
                foreach (int wallStatusId in wallStatusKeys)
                {
                    WallStatus wallStatus = wallStatusDic[wallStatusId];

                    if (wallStatus.mode == WallStatusModes.IDLE)
                    {
                        continue;
                    }
                    else if (wallStatus.mode == WallStatusModes.DRAGGING && wallStatus.initObj != null)
                    {
                        // calculate initialDistanceToWall and wallMovementScale
                        Vector3 initWallProjectedCameraPos = wallStatus.initObj.transform.InverseTransformPoint(GetCameraFrontPosition());
                        initWallProjectedCameraPos = Vector3.Scale(initWallProjectedCameraPos, new Vector3(1, 1, 0));
                        initWallProjectedCameraPos = wallStatus.initObj.transform.TransformPoint(initWallProjectedCameraPos);
                        float cameraDistanceToInitWall = Vector3.Magnitude(initWallProjectedCameraPos - GetCameraFrontPosition());

                        // calculate current distance to wall, by projecting camera position into the wall in wall's z-axis
                        Vector3 wallProjectedCameraPosition = wallStatus.obj.transform.InverseTransformPoint(GetCameraFrontPosition());
                        wallProjectedCameraPosition = Vector3.Scale(wallProjectedCameraPosition, new Vector3(1, 1, 0));
                        wallProjectedCameraPosition = wallStatus.obj.transform.TransformPoint(wallProjectedCameraPosition);
                        float cameraDistanceToWall = Vector3.Magnitude(wallProjectedCameraPosition - GetCameraFrontPosition());

                        // the scale of wall movement 
                        wallStatus.movementScale = (cameraDistanceToInitWall - MinimumDistanceToWall) / (MaximumArmLength - MinimumArmLength);
                        // the scale between initial wall and current wall
                        wallStatus.distanceScale = cameraDistanceToWall / cameraDistanceToInitWall;    // TODO needs error handling for zero divide or something?

                        // calculate initial distance to wall, by projecting the item's position into the wall in wall's z-axis
                        Vector3 wallProjectedItemPosition = wallStatus.obj.transform.InverseTransformPoint(itemStatus.obj.transform.position);
                        wallProjectedItemPosition = Vector3.Scale(wallProjectedItemPosition, new Vector3(1, 1, 0));
                        wallProjectedItemPosition = wallStatus.obj.transform.TransformPoint(wallProjectedItemPosition);
                        float itemDistanceToWall = Vector3.Magnitude(wallProjectedItemPosition - itemStatus.obj.transform.position);

                        // the items not between the camera and the initial wall must not be affected
                        if (cameraDistanceToInitWall < itemDistanceToWall)
                        {
                            continue;
                        }

                        // recalculate initPos while the item is being dragged
                        if (itemStatus.mode == ItemStatusModes.DRAGGING && itemStatus.obj != null)
                        {
                            Vector3 initWallRefItemPos = wallStatus.initObj.transform.InverseTransformPoint(itemStatus.obj.transform.position);
                            Vector3 initWallRefCameraPos = wallStatus.initObj.transform.InverseTransformPoint(GetCameraFrontPosition());
                            Vector3 cameraToItemDirection = initWallRefItemPos - initWallRefCameraPos;
                            cameraToItemDirection = Vector3.Scale(cameraToItemDirection, new Vector3(1, 1, 1 / wallStatus.distanceScale));
                            initWallRefItemPos = initWallRefCameraPos + cameraToItemDirection;
                            initWallRefItemPos = wallStatus.initObj.transform.TransformPoint(initWallRefItemPos);

                            itemInitPosChange += initWallRefItemPos - itemStatus.obj.transform.position;
                        }
                        // otherwise, in case of the other items
                        // rescaled position is calculated based on initPos
                        else if (itemStatus.mode == ItemStatusModes.IDLE && itemStatus.obj != null)
                        {
                            Vector3 initWallRefItemPos = wallStatus.initObj.transform.InverseTransformPoint(itemStatus.initPos);
                            Vector3 initWallRefCameraPos = wallStatus.initObj.transform.InverseTransformPoint(GetCameraFrontPosition());
                            Vector3 cameraToItemDirection = initWallRefItemPos - initWallRefCameraPos;
                            cameraToItemDirection = Vector3.Scale(cameraToItemDirection, new Vector3(1, 1, wallStatus.distanceScale));
                            initWallRefItemPos = initWallRefCameraPos + cameraToItemDirection;
                            initWallRefItemPos = wallStatus.initObj.transform.TransformPoint(initWallRefItemPos);

                            itemPosChange += initWallRefItemPos - itemStatus.initPos;
                        }
                    }
                    else if (wallStatus.mode == WallStatusModes.LOCKED && wallStatus.initObj != null)
                    {
                        // TODO BUG do not handle when camera position changes while the walls are locked
                        // while locked, we use cameraFrontWhenLocked for camera position
                        // recalculate initPos while the item is being dragged
                        if (itemStatus.mode == ItemStatusModes.DRAGGING && itemStatus.obj != null)
                        {
                            Vector3 initWallRefItemPos = wallStatus.initObj.transform.InverseTransformPoint(itemStatus.obj.transform.position);
                            Vector3 initWallRefCameraPos = wallStatus.initObj.transform.InverseTransformPoint(wallStatus.cameraFrontWhenLocked);
                            Vector3 cameraToItemDirection = initWallRefItemPos - initWallRefCameraPos;
                            cameraToItemDirection = Vector3.Scale(cameraToItemDirection, new Vector3(1, 1, 1 / wallStatus.distanceScale));
                            initWallRefItemPos = initWallRefCameraPos + cameraToItemDirection;
                            initWallRefItemPos = wallStatus.initObj.transform.TransformPoint(initWallRefItemPos);

                            itemInitPosChange += initWallRefItemPos - itemStatus.obj.transform.position;
                        }
                        // otherwise, in case of the other items
                        // rescaled position is calculated based on initPos
                        else if (itemStatus.mode == ItemStatusModes.IDLE && itemStatus.obj != null)
                        {
                            Vector3 initWallRefItemPos = wallStatus.initObj.transform.InverseTransformPoint(itemStatus.initPos);
                            Vector3 initWallRefCameraPos = wallStatus.initObj.transform.InverseTransformPoint(wallStatus.cameraFrontWhenLocked);
                            Vector3 cameraToItemDirection = initWallRefItemPos - initWallRefCameraPos;
                            cameraToItemDirection = Vector3.Scale(cameraToItemDirection, new Vector3(1, 1, wallStatus.distanceScale));
                            initWallRefItemPos = initWallRefCameraPos + cameraToItemDirection;
                            initWallRefItemPos = wallStatus.initObj.transform.TransformPoint(initWallRefItemPos);

                            itemPosChange += initWallRefItemPos - itemStatus.initPos;
                        }
                    }
                    wallStatusDic[wallStatusId] = wallStatus;
                }
                
                if (itemStatus.mode == ItemStatusModes.DRAGGING && itemStatus.obj != null)
                {
                    itemStatus.initPos = itemStatus.obj.transform.position + itemInitPosChange;
                }
                else if (itemStatus.mode == ItemStatusModes.IDLE && itemStatus.obj != null)
                {
                    itemStatus.obj.transform.position = itemStatus.initPos + itemPosChange;
                }

                itemStatusDic[itemStatusId] = itemStatus;
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
            IsWallAvailable = true;

            // initialize itemStatusDic
            List<GameObject> items = VirtualItemsManager.Instance.GetItemObjects();
            for (int i = 0; i < items.Count; i++)
            {
                ItemStatus itemStatus = new ItemStatus(items[i]);
                itemStatus.initPos = items[i].transform.position;
                itemStatusDic.Add(items[i].GetInstanceID(), itemStatus);
            }
        }

        public void SetWallMode(GameObject obj, WallStatusModes mode)
        {
            if (mode == WallStatusModes.DRAGGING)
            {
                if (MaximumArmLength < MinimumArmLength)
                {
                    Debug.Log("MaximumArmLength < MinimumArmLength. StartReposition()/RepositionManager");
                    return;
                }
                
                WallStatus wallStatus = wallStatusDic[obj.GetInstanceID()];

                wallStatus.obj = obj;
                wallStatus.initObj = Instantiate(obj);
                Destroy(wallStatus.initObj.GetComponent<HandDraggableWall>());
                wallStatus.initObj.layer = LayerMask.NameToLayer("Ignore Raycast");
                if (!ShowInitialWallObject)
                {
                    wallStatus.initObj.GetComponent<MeshRenderer>().enabled = false;
                    wallStatus.initObj.GetComponent<MeshCollider>().sharedMesh = null;
                }
                wallStatus.mode = WallStatusModes.DRAGGING;

                // assign updated wallStatus
                wallStatusDic[obj.GetInstanceID()] = wallStatus;
            }
            else if (mode == WallStatusModes.LOCKED)
            {
                WallStatus wallStatus = wallStatusDic[obj.GetInstanceID()];

                wallStatus.mode = WallStatusModes.LOCKED;
                wallStatus.cameraFrontWhenLocked = GetCameraFrontPosition();

                wallStatusDic[obj.GetInstanceID()] = wallStatus;
            }
            else if (mode == WallStatusModes.IDLE)
            {
                WallStatus wallStatus = wallStatusDic[obj.GetInstanceID()];

                wallStatus.mode = WallStatusModes.IDLE;
                wallStatus.movementScale = DefaultMovementScale;
                Destroy(wallStatus.initObj);
                wallStatus.initObj = null;
                wallStatus.distanceScale = 1f;
                wallStatus.cameraFrontWhenLocked = Vector3.zero;
                wallStatusDic[obj.GetInstanceID()] = wallStatus;

                // restore items' position
                List<int> itemStatusKeys = new List<int>(itemStatusDic.Keys);
                foreach (int itemStatusId in itemStatusKeys)
                {
                    ItemStatus itemStatus = itemStatusDic[itemStatusId];
                    itemStatus.obj.transform.position = itemStatus.initPos;
                    itemStatusDic[itemStatusId] = itemStatus;
                }
            }
        }

        public void SetItemMode(GameObject obj, ItemStatusModes mode)
        {
            if (mode == ItemStatusModes.DRAGGING)
            {
                ItemStatus itemStatus = itemStatusDic[obj.GetInstanceID()];
                itemStatus.mode = ItemStatusModes.DRAGGING;
                itemStatusDic[obj.GetInstanceID()] = itemStatus;
            }
            else if (mode == ItemStatusModes.IDLE)
            {
                ItemStatus itemStatus = itemStatusDic[obj.GetInstanceID()];
                itemStatus.mode = ItemStatusModes.IDLE;
                itemStatusDic[obj.GetInstanceID()] = itemStatus;
            }
        }

        private Vector3 GetCameraFrontPosition()
        {
            return mainCamera.transform.position + mainCamera.transform.forward * NearClippingPlaneDist;
        }
        
        // TODO would be deprecated since reposition logic would be migrated to this class
        public float GetWallMovementScale(int instanceId)
        {
            return wallStatusDic[instanceId].movementScale;
        }
        
        public ItemStatusModes GetItemStatusMode(int itemObjectId)
        {
            ItemStatus itemStatus;
            if (!itemStatusDic.TryGetValue(itemObjectId, out itemStatus))
            {
                Debug.LogError("item " + itemObjectId + " doesn't exist");
                return ItemStatusModes.IDLE;
            }
            return itemStatus.mode;
        }

        public WallStatusModes GetWallStatusMode(int wallObjectId)
        {
            WallStatus wallStatus;
            if (!wallStatusDic.TryGetValue(wallObjectId, out wallStatus))
            {
                Debug.LogError("wall " + wallObjectId + " doesn't exist");
                return WallStatusModes.IDLE;
            }
            return wallStatus.mode;
        }

        public bool IsItemExist(int itemObjectId)
        {
            return itemStatusDic.ContainsKey(itemObjectId);
        }

        public bool IsWallExist(int wallObjectId)
        {
            return wallStatusDic.ContainsKey(wallObjectId);
        }
    }
}

