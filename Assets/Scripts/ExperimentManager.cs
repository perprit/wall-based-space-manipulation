using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using HoloToolkit.Unity;
using UnityEngine.SceneManagement;
using HoloToolkit.Unity.SpatialMapping;
using HoloToolkit.Unity.InputModule;

namespace ManipulateWalls
{

    public class ExperimentManager : Singleton<ExperimentManager>
    {
        public GameObject wallPrefab;
        public GameObject itemPrefab;
        public int UserId = 0;
        public int PhaseId = 0;
        public int TrialId = 0;
        public bool UseSpatialMapping = false;

        public Vector3 WallInitPosition = new Vector3(1.75f, 0.7f, 10.5f);
        public Vector3 WallInitScale = new Vector3(3.5f, 3f, 0.001f);

        public event EventHandler SetWallComplete;

        private GameObject itemObj;
        private GameObject wallObj;

        private List<TaskSetting> taskSettingList = new List<TaskSetting>();

        void Start()
        {
            if (UseSpatialMapping)
            {
                return;
            }
            gameObject.transform.position = Vector3.zero;
            gameObject.transform.rotation = Quaternion.identity;
            SetItemAndWall();
        }

        public void InitExperiment()
        {
            //taskSettingList = TaskManager.Instance.LoadTaskSequence(UserId);
            //SetTaskSettingSequence();
        }

        private void SetItemAndWall()
        {
            // set item
            if (itemObj != null)
            {
                itemObj = null;
            }
            itemObj = Instantiate(itemPrefab);
            itemObj.transform.parent = gameObject.transform;
            itemObj.transform.position = WallInitPosition + Vector3.back * 6f;

            // set wall
            if (wallObj != null)
            {
                Destroy(wallObj);
                wallObj = null;
            }
            wallObj = Instantiate(wallPrefab);
            wallObj.transform.parent = gameObject.transform;
            wallObj.layer = SpatialMappingManager.Instance.PhysicsLayer;

            wallObj.transform.position = WallInitPosition;
            wallObj.transform.rotation = Quaternion.identity;
            wallObj.transform.localScale = WallInitScale;

            // We are done set wall, trigger an event.
            EventHandler handler = SetWallComplete;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        public GameObject GetItemObject()
        {
            return itemObj;
        }

        public GameObject GetWallObject()
        {
            return wallObj;
        }
        
        public void ResetScene()
        {
            SceneManager.LoadScene("Main");
        }
    }
}
