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
        public float MaximumArmLength = 0.55f;   // MaximumArmLength must be bigger than MinimumArmLength
        public float MinimumDistanceToWall = 1.5f;
        public float DefaultMovementScale = 5f;
        public float SmoothingRatio = 0.8f;
        public float NearClippingPlaneDist = 0.85f;

        public bool IsWallAvailable = false;

        public GameObject InitItemPrefab;

        private Camera mainCamera;

        private Dictionary<int, WallStatus> wallStatusDic = new Dictionary<int, WallStatus>();
        private Dictionary<int, ItemStatus> itemStatusDic = new Dictionary<int, ItemStatus>();

        void Start()
        {
            mainCamera = Camera.main;
            //currentItemsTransforms = VirtualItemsManager.Instance.GetAllObjectTransforms();
            if (ExperimentManager.Instance.UseSpatialMapping)
            {
                SurfaceMeshesToPlanes.Instance.MakePlanesComplete += SurfaceMeshesToPlanes_MakePlanesComplete;
            }
            else
            {
                ExperimentManager.Instance.SetWallComplete += ExperimentManager_SetWallComplete;
            }
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
                if (itemStatus.obj == null)
                {
                    Debug.LogError("itemStatus.obj == null");
                    return;
                }

                Vector3 itemPosChange = Vector3.zero;
                Vector3 itemInitPosChange = Vector3.zero;

                // calculate obj.transform.position and initPos of itemStatus based on each walls
                List<int> wallStatusKeys = new List<int>(wallStatusDic.Keys);
                foreach (int wallStatusId in wallStatusKeys)
                {
                    WallStatus wallStatus = wallStatusDic[wallStatusId];
                    if (wallStatus.obj == null)
                    {
                        Debug.LogError("wallStatus.obj == null");
                        return;
                    }

                    if (wallStatus.mode == WallStatusModes.IDLE)
                    {
                        continue;
                    }
                    else if (wallStatus.mode == WallStatusModes.DRAGGING)
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

                        // calculate item distance to wall, by projecting the item's position into the wall in wall's z-axis
                        Vector3 initWallProjItemPosition = wallStatus.initObj.transform.InverseTransformPoint(itemStatus.obj.transform.position);
                        initWallProjItemPosition = Vector3.Scale(initWallProjItemPosition, new Vector3(1, 1, 0));
                        initWallProjItemPosition = wallStatus.initObj.transform.TransformPoint(initWallProjItemPosition);
                        float itemDistanceToInitWall = Vector3.Magnitude(initWallProjItemPosition - itemStatus.obj.transform.position);

                        // the items not between the camera and the initial wall must not be affected
                        if (cameraDistanceToInitWall < itemDistanceToInitWall)
                        {
                            continue;
                        }
                        
                        // calculate initPos with respect to current wall
                        Vector3 cameraToItemDir = itemStatus.initObj.transform.position - GetCameraFrontPosition();
                        cameraToItemDir = wallStatus.initObj.transform.InverseTransformDirection(cameraToItemDir);
                        cameraToItemDir = Vector3.Scale(cameraToItemDir, new Vector3(1, 1, wallStatus.distanceScale));
                        cameraToItemDir = wallStatus.initObj.transform.TransformDirection(cameraToItemDir);
                        Vector3 newItemInitPos = GetCameraFrontPosition() + cameraToItemDir;
                        
                        // current position of the item
                        if (itemStatus.mode == ItemStatusModes.IDLE)
                        {
                            itemPosChange += newItemInitPos - itemStatus.initObj.transform.position;
                        }
                    }
                    else if (wallStatus.mode == WallStatusModes.LOCKED)
                    {
                        // calculate initPos with respect to current wall
                        // while locked, we use cameraFrontWhenLocked for camera position
                        Vector3 cameraToItemDir = itemStatus.initObj.transform.position - wallStatus.cameraFrontWhenLocked;
                        cameraToItemDir = wallStatus.initObj.transform.InverseTransformDirection(cameraToItemDir);
                        cameraToItemDir = Vector3.Scale(cameraToItemDir, new Vector3(1, 1, wallStatus.distanceScale));
                        cameraToItemDir = wallStatus.initObj.transform.TransformDirection(cameraToItemDir);
                        Vector3 newItemInitPos = wallStatus.cameraFrontWhenLocked + cameraToItemDir;

                        // recalculate initPos while the item is being dragged
                        if (itemStatus.mode == ItemStatusModes.DRAGGING)
                        {
                            itemInitPosChange += itemStatus.initObj.transform.position - newItemInitPos;
                            // clamping
                            Vector3 initRefChange = wallStatus.initObj.transform.InverseTransformDirection(itemInitPosChange);
                            if (initRefChange.z < 0f) initRefChange.z = 0f;
                            initRefChange = wallStatus.initObj.transform.TransformDirection(initRefChange);
                            itemInitPosChange = initRefChange;
                        }
                        // or current position of the item
                        else if (itemStatus.mode == ItemStatusModes.IDLE)
                        {
                            itemPosChange += newItemInitPos - itemStatus.initObj.transform.position;
                            // clamping
                            Vector3 itemRefChange = wallStatus.initObj.transform.InverseTransformDirection(itemPosChange);
                            if (itemRefChange.z > 0f) itemRefChange.z = 0f;
                            itemRefChange = wallStatus.initObj.transform.TransformDirection(itemRefChange);
                            itemPosChange = itemRefChange;
                        }
                    }
                    wallStatusDic[wallStatusId] = wallStatus;
                }
                
                if (itemStatus.mode == ItemStatusModes.DRAGGING)
                {
                    itemStatus.initObj.transform.position = itemStatus.obj.transform.position + itemInitPosChange;
                }
                else if (itemStatus.mode == ItemStatusModes.IDLE)
                {
                    itemStatus.obj.transform.position = itemStatus.initObj.transform.position + itemPosChange;
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
                itemStatus.initObj = Instantiate(InitItemPrefab);
                itemStatus.initObj.transform.position = itemStatus.obj.transform.position;

                itemStatusDic.Add(items[i].GetInstanceID(), itemStatus);
            }
        }

        private void ExperimentManager_SetWallComplete(object source, System.EventArgs args)
        {
            // set surface meshes disabled
            SpatialMappingManager.Instance.EnableSurfaceMeshes(false);

            // initialize wall
            GameObject wallObj = ExperimentManager.Instance.GetWallObject();
            WallStatus wallStatus = new WallStatus(wallObj);
            wallStatusDic.Add(wallObj.GetInstanceID(), wallStatus);

            IsWallAvailable = true;

            // initialize item
            GameObject itemObj = ExperimentManager.Instance.GetItemObject();
            ItemStatus itemStatus = new ItemStatus(itemObj);
            itemStatus.initObj = Instantiate(InitItemPrefab);
            itemStatus.initObj.transform.position = itemStatus.obj.transform.position;
            itemStatusDic.Add(itemObj.GetInstanceID(), itemStatus);

            GameObject targetObj = ExperimentManager.Instance.GetTargetObject();
            ItemStatus targetItemStatus = new ItemStatus(targetObj);
            targetItemStatus.initObj = Instantiate(InitItemPrefab);
            targetItemStatus.initObj.transform.position = targetItemStatus.obj.transform.position;
            itemStatusDic.Add(targetObj.GetInstanceID(), targetItemStatus);
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

                wallStatus.mode = WallStatusModes.DRAGGING;
                wallStatus.obj.GetComponent<Renderer>().material.color = new Color(0.1f, 1.0f, 0.7f);
                wallStatus.initObj = Instantiate(obj);
                wallStatus.initObj.layer = LayerMask.NameToLayer("Ignore Raycast");
                wallStatus.initObj.GetComponent<MeshRenderer>().enabled = false;
                Destroy(wallStatus.initObj.GetComponent<HandDraggableWall>());

                // assign updated wallStatus
                wallStatusDic[obj.GetInstanceID()] = wallStatus;
                ExperimentManager.Instance.AddEventLog(LogEvent.WALL_DRAGGING);
            }
            else if (mode == WallStatusModes.LOCKED)
            {
                WallStatus wallStatus = wallStatusDic[obj.GetInstanceID()];

                wallStatus.mode = WallStatusModes.LOCKED;
                wallStatus.obj.GetComponent<Renderer>().material.color = new Color(0.3f, 1.0f, 0.3f);
                wallStatus.cameraFrontWhenLocked = GetCameraFrontPosition();

                wallStatusDic[obj.GetInstanceID()] = wallStatus;
                ExperimentManager.Instance.AddEventLog(LogEvent.WALL_LOCKED);
            }
            else if (mode == WallStatusModes.IDLE)
            {
                WallStatus wallStatus = wallStatusDic[obj.GetInstanceID()];

                wallStatus.mode = WallStatusModes.IDLE;
                wallStatus.obj.GetComponent<Renderer>().material.color = new Color(1.0f, 1.0f, 1.0f);
                Destroy(wallStatus.initObj);
                wallStatus.initObj = null;
                wallStatus.movementScale = DefaultMovementScale;
                wallStatus.distanceScale = 1f;
                wallStatus.cameraFrontWhenLocked = Vector3.zero;
                wallStatusDic[obj.GetInstanceID()] = wallStatus;

                // restore items' position
                List<int> itemStatusKeys = new List<int>(itemStatusDic.Keys);
                foreach (int itemStatusId in itemStatusKeys)
                {
                    ItemStatus itemStatus = itemStatusDic[itemStatusId];
                    itemStatus.obj.transform.position = itemStatus.initObj.transform.position;
                    itemStatusDic[itemStatusId] = itemStatus;
                }
                ExperimentManager.Instance.AddEventLog(LogEvent.WALL_IDLE);
            }
        }

        public void SetItemMode(GameObject obj, ItemStatusModes mode)
        {
            if (mode == ItemStatusModes.DRAGGING)
            {
                ItemStatus itemStatus = itemStatusDic[obj.GetInstanceID()];
                itemStatus.mode = ItemStatusModes.DRAGGING;
                itemStatusDic[obj.GetInstanceID()] = itemStatus;
                ExperimentManager.Instance.AddEventLog(LogEvent.ITEM_DRAGGING);
            }
            else if (mode == ItemStatusModes.IDLE)
            {
                ItemStatus itemStatus = itemStatusDic[obj.GetInstanceID()];
                itemStatus.mode = ItemStatusModes.IDLE;
                itemStatusDic[obj.GetInstanceID()] = itemStatus;
                ExperimentManager.Instance.AddEventLog(LogEvent.ITEM_IDLE);
            }
        }

        public Vector3 GetCameraFrontPosition()
        {
            return mainCamera.transform.position + mainCamera.transform.forward * NearClippingPlaneDist;
        }

        public Vector3 GetCameraMinDistToWallPosition()
        {
            return mainCamera.transform.position + mainCamera.transform.forward * MinimumDistanceToWall;
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

        public GameObject GetItemInitObject(int itemObjectId)
        {
            ItemStatus itemStatus;
            if (!itemStatusDic.TryGetValue(itemObjectId, out itemStatus))
            {
                Debug.LogError("wall " + itemObjectId + " doesn't exist");
                return null;
            }

            if (itemStatus.initObj == null)
            {
                Debug.LogError("initObj of wall " + itemObjectId + " doesn't exist");
                return null;
            }

            return itemStatus.initObj;
        }

        public GameObject GetWallInitObject(int wallObjectId)
        {
            WallStatus wallStatus;
            if (!wallStatusDic.TryGetValue(wallObjectId, out wallStatus))
            {
                Debug.LogError("wall " + wallObjectId + " doesn't exist");
                return null;
            }
            
            if (wallStatus.initObj == null)
            {
                Debug.LogError("initObj of wall " + wallObjectId + " doesn't exist");
                return null;
            }

            return wallStatus.initObj;
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

