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
        public int UserId = 0;
        public int PhaseId = 0;
        public int TrialId = 0;
        public bool UseSpatialMapping = false;
        
        public event EventHandler SetWallComplete;

        private List<TaskSetting> taskSettingList = new List<TaskSetting>();
        private GameObject wallObj;

        void Start()
        {
            if (UseSpatialMapping)
            {
                return;
            }
            gameObject.transform.position = Vector3.zero;
            gameObject.transform.rotation = Quaternion.identity;
            SetWall();
        }

        public void InitExperiment()
        {
            //taskSettingList = TaskManager.Instance.LoadTaskSequence(UserId);
            //SetTaskSettingSequence();
        }

        public void SetWall()
        {
            if (wallObj != null)
            {
                Destroy(wallObj);
                wallObj = null;
            }
            wallObj = Instantiate(wallPrefab);
            wallObj.transform.parent = gameObject.transform;
            wallObj.layer = SpatialMappingManager.Instance.PhysicsLayer;
            
            wallObj.transform.position = new Vector3(0f, 0f, 8f);
            wallObj.transform.rotation = Quaternion.identity;
            wallObj.transform.localScale = new Vector3(2f, 2f, 0.001f);

            // We are done set wall, trigger an event.
            EventHandler handler = SetWallComplete;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
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
