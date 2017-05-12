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
        public struct Trial
        {
            public string xy_type;
            public string z_type;
            public Vector3 startPos;
            public Vector3 targetPos;

            public Trial(string _xy, string _z, Vector3 _sp, Vector3 _tp)
            {
                xy_type = _xy;
                z_type = _z;
                startPos = _sp;
                targetPos = _tp;
            }
        }
        public int UserId;
        public bool UseSpatialMapping;
        public InteractionType InteractionType;

        public Vector3 OriginPos = new Vector3(1.75f, 0.7f, 0.5f);
        public Vector3 WallInitPos = new Vector3(1.75f, 0.7f, 10.5f);
        public Vector3 WallInitScale = new Vector3(3.5f, 3f, 0.001f);
        public float LeastDistance = 0.25f;

        public event EventHandler SetWallComplete;
        public event EventHandler TrialComplete;

        private List<Trial> practiceTrials = new List<Trial>();
        private List<Trial> trials = new List<Trial>();
        private int trialIdx = 0;
        private string method = "unknown";

        public GameObject wallPrefab;
        public GameObject itemPrefab;
        public GameObject targetPrefab;

        private GameObject itemObj;
        private GameObject wallObj;
        private GameObject targetObj;

        public bool TRIALS_READY = false;

        void Start()
        {
            if (UseSpatialMapping)
            {
                return;
            }

            TrialComplete += ExperimentManager_TrialComplete;
            gameObject.transform.position = Vector3.zero;
            gameObject.transform.rotation = Quaternion.identity;

            InitObjects();
#if UNITY_EDITOR
            trials.Clear();
            trialIdx = 0;
            method = "DIST_W";
            TRIALS_READY = true;
            SetInteractionMethod("DIST_W");
            trials.Add(new Trial("I2O", "M2S", new Vector3(0.776f, 1.126f, 1.511f), new Vector3(1.176f, 1.126f, 1.511f)));
            trials.Add(new Trial("I2O", "M2S", new Vector3(0.776f, 1.126f, 1.511f), new Vector3(1.176f, 1.126f, 1.511f)));
            StartTrial(0);
#endif
        }

        void Update()
        {
            if (TRIALS_READY)
            {
                if (RepositionManager.Instance.GetItemStatusMode(itemObj.GetInstanceID()) == ItemStatusModes.IDLE
                    && RepositionManager.Instance.GetWallStatusMode(wallObj.GetInstanceID()) == WallStatusModes.IDLE
                    && Vector3.Magnitude(targetObj.transform.position - itemObj.transform.position) < LeastDistance)
                {
                    EventHandler handler = TrialComplete;
                    if (handler != null)
                    {
                        handler(this, EventArgs.Empty);
                    }
                }
            }
        }

        private void ExperimentManager_TrialComplete (object source, EventArgs args)
        {
            Debug.Log("Trial complete, " + trialIdx);
            trialIdx++;
            if (trialIdx >= trials.Count)
            {
                DebugTextController.Instance.SetMessage("Done!");
                TRIALS_READY = false;
                return;
            }
            
            StartTrial(trialIdx);
        }

        private void InitObjects()
        {
            // set item
            if (itemObj != null)
            {
                Destroy(itemObj);
                itemObj = null;
            }
            itemObj = Instantiate(itemPrefab);
            itemObj.transform.parent = gameObject.transform;
            SetStartPos(Vector3.forward * 3f + Vector3.right * -0.5f);

            // set target
            if (targetObj != null)
            {
                Destroy(targetObj);
                targetObj = null;
            }
            targetObj = Instantiate(targetPrefab);
            targetObj.transform.parent = gameObject.transform;
            SetTargetPos(Vector3.forward * 8f + Vector3.right * 0.5f);

            // set wall
            if (wallObj != null)
            {
                Destroy(wallObj);
                wallObj = null;
            }
            wallObj = Instantiate(wallPrefab);
            wallObj.transform.parent = gameObject.transform;
            wallObj.layer = SpatialMappingManager.Instance.PhysicsLayer;

            wallObj.transform.position = WallInitPos;
            wallObj.transform.rotation = Quaternion.identity;
            wallObj.transform.localScale = WallInitScale;

            // We are done set wall, trigger an event.
            EventHandler handler = SetWallComplete;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void SetInteractionMethod(string methodName)
        {
            // methodName : CONST_N, DIST_N, ADAPT_N, CONST_W, DIST_W, ADAPT_W
            string[] tokens = methodName.Split('_');
            string interType= tokens[0];
            string wEnabled = tokens[1];

            if (interType == "CONST") InteractionType = InteractionType.CONST;
            else if (interType == "DIST") InteractionType = InteractionType.DIST;
            else if (interType == "ADAPT") InteractionType = InteractionType.ADAPT;
            else
            {
                Debug.LogError("Invalid interaction type name: " + interType);
                return;
            }

            if (wEnabled == "N") EnableWallObj(false);
            else if (wEnabled == "W") EnableWallObj(true);
            else
            {
                Debug.LogError("Invalid wall enabled flag: " + wEnabled);
                return;
            }
        }

        private void EnableWallObj(bool enable)
        {
            if (enable)
            {
                wallObj.layer = LayerMask.NameToLayer("SpatialMapping");
                wallObj.GetComponent<MeshRenderer>().enabled = true;
            }
            else
            {
                wallObj.layer = LayerMask.NameToLayer("Ignore Raycast");
                wallObj.GetComponent<MeshRenderer>().enabled = false;
            }
        }

        private void StartTrial(int trialNumber)
        {
            if (trials == null || trials.Count == 0)
            {
                Debug.LogError("trials is null or empty");
                return;
            }
            if (itemObj == null || targetObj == null)
            {
                Debug.LogError("itemObj == null or targetObj == null while StartCurrentTrial");
                return;
            }

            Trial trial = trials[trialNumber];

            SetStartPos(trial.startPos);
            SetTargetPos(trial.targetPos);

            DebugTextController.Instance.SetMessage("ID: " + UserId);
            DebugTextController.Instance.AddMessage("Method: " + method);
            DebugTextController.Instance.AddMessage("Trial: " + trialNumber + "/" + trials.Count);
        }

        private void SetStartPos(Vector3 start)
        {
            if (!RepositionManager.Instance.IsWallAvailable)
            {
                return;
            }
            if (itemObj != null && RepositionManager.Instance.GetItemStatusMode(itemObj.GetInstanceID()) == ItemStatusModes.DRAGGING)
            {
                Debug.Log("Item is dragged. cannot reset init pos");
            }

            GameObject initObj = RepositionManager.Instance.GetItemInitObject(itemObj.GetInstanceID());

            initObj.transform.position = start + OriginPos;
        }

        private void SetTargetPos(Vector3 target)
        {
            targetObj.transform.position = target + OriginPos;
        }

        public void SetTrialList(SequenceData sd)
        {
            UserId = int.Parse(sd.id);
            trials.Clear();
            trialIdx = 0;
            method = sd.method;
            SetInteractionMethod(sd.method);

            for (int i = 0; i < 4; i++)
            {
                SequenceData.TrialString trial = sd.trials[i];
            //foreach (SequenceData.TrialString trial in sd.trials)
            //{
                Vector3 startPos = new Vector3(float.Parse(trial.start[0]), float.Parse(trial.start[1]), float.Parse(trial.start[2]));
                Vector3 targetPos = new Vector3(float.Parse(trial.target[0]), float.Parse(trial.target[1]), float.Parse(trial.target[2]));
                trials.Add(new Trial(trial.xy_type, trial.z_type, startPos, targetPos));
                //Debug.Log(trials.Count + "/ " + trial.xy_type + ", " + trial.z_type + ", " + startPos.ToString("F2") + ", " + targetPos.ToString("F2"));
            }

            TRIALS_READY = true;
            StartTrial(0);
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
            UDPManager.Instance.DisposeSocket();
            DebugTextController.Instance.SetMessage("Scene reloaded");
            SceneManager.LoadScene("Main");
        }
    }
}
