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
    public enum LogEvent
    {
        TRIAL_START, TRIAL_END,
        ITEM_DRAGGING, ITEM_IDLE,
        WALL_DRAGGING, WALL_LOCKED, WALL_IDLE,
        HAND_LOST, HAND_FOUND
    }

    public enum InteractionType
    {
        CONST, DIST, ADAPT
    }

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
        public Vector3 ItemOriginScale = new Vector3(0.20f, 0.20f, 0.20f);

        public event EventHandler SetWallComplete;
        public event EventHandler CountdownEnd;
        public event EventHandler TrialComplete;

        public int CountdownDuration = 5;

        private List<Trial> trials = new List<Trial>();
        private int trialIdx = 0;
        private string method = "unknown";
        private float leastDistance = 0.20f;

        public GameObject invisibleWallPrefab;
        public GameObject wallPrefab;
        public GameObject itemPrefab;
        public GameObject targetPrefab;

        public GameObject targetCube;
        public GameObject itemCube;
        public GameObject targetSphere;
        public GameObject itemSphere;

        private GameObject itemObj;
        private GameObject wallObj;
        private GameObject targetObj;
        private GameObject leftWall, rightWall, ceil, floor;

        public bool TRIALS_READY = false;
        private bool ON_COUNTDOWN = false;
        
        private bool SEND_LOG = true;
        private float trialStartTime = 0f;
        private Vector3 handMoveVector = new Vector3(0.0f, 0.0f, 0.0f);
        private string currXYType = "unknown";
        private string currZType = "unknown";
        private float deltaTimeCounter = 0f;
        private int countdownSecLeft = 0;

        private Vector3 prevHandPos = Vector3.zero;

        void Start()
        {
            if (UseSpatialMapping)
            {
                return;
            }

            TrialComplete += ExperimentManager_TrialComplete;
            CountdownEnd += ExperimentManager_CounddownEnd;
            gameObject.transform.position = Vector3.zero;
            gameObject.transform.rotation = Quaternion.identity;

            InitObjects();
            trials.Clear();
            trialIdx = 0;
            method = "DIST_W";
            TRIALS_READY = true;
            SetInteractionMethod("DIST_W");
            trials.Add(new Trial("D1", "M2S", new Vector3(-0.300f, -0.300f, 2.000f), new Vector3(0.200f, 0.300f, 9.000f)));
            trials.Add(new Trial("D1", "M2F", new Vector3(-0.300f, -0.300f, 2.000f), new Vector3(0.200f, 0.300f, 9.000f)));
            trials.Add(new Trial("D1", "F2S", new Vector3(-0.300f, -0.300f, 9.000f), new Vector3(0.200f, 0.300f, 2.000f)));
            trials.Add(new Trial("D1", "F2M", new Vector3(-0.300f, -0.300f, 9.000f), new Vector3(0.200f, 0.300f, 2.000f)));
            trials.Add(new Trial("D1", "M2S", new Vector3(-0.300f, -0.300f, 2.000f), new Vector3(0.200f, 0.300f, 9.000f)));
            trials.Add(new Trial("D1", "M2F", new Vector3(-0.300f, -0.300f, 2.000f), new Vector3(0.200f, 0.300f, 9.000f)));
            trials.Add(new Trial("D1", "F2S", new Vector3(-0.300f, -0.300f, 9.000f), new Vector3(0.200f, 0.300f, 2.000f)));
            trials.Add(new Trial("D1", "F2M", new Vector3(-0.300f, -0.300f, 9.000f), new Vector3(0.200f, 0.300f, 2.000f)));
            trials.Add(new Trial("D1", "M2S", new Vector3(-0.300f, -0.300f, 2.000f), new Vector3(0.200f, 0.300f, 9.000f)));
            trials.Add(new Trial("D1", "M2F", new Vector3(-0.300f, -0.300f, 2.000f), new Vector3(0.200f, 0.300f, 9.000f)));
            trials.Add(new Trial("D1", "F2S", new Vector3(-0.300f, -0.300f, 9.000f), new Vector3(0.200f, 0.300f, 2.000f)));
            trials.Add(new Trial("D1", "F2M", new Vector3(-0.300f, -0.300f, 9.000f), new Vector3(0.200f, 0.300f, 2.000f)));
            trials.Add(new Trial("D1", "M2S", new Vector3(-0.300f, -0.300f, 2.000f), new Vector3(0.200f, 0.300f, 9.000f)));
            trials.Add(new Trial("D1", "M2F", new Vector3(-0.300f, -0.300f, 2.000f), new Vector3(0.200f, 0.300f, 9.000f)));
            trials.Add(new Trial("D1", "F2S", new Vector3(-0.300f, -0.300f, 9.000f), new Vector3(0.200f, 0.300f, 2.000f)));
            trials.Add(new Trial("D1", "F2M", new Vector3(-0.300f, -0.300f, 9.000f), new Vector3(0.200f, 0.300f, 2.000f)));
            trials.Add(new Trial("D1", "M2S", new Vector3(-0.300f, -0.300f, 2.000f), new Vector3(0.200f, 0.300f, 9.000f)));
            trials.Add(new Trial("D1", "M2F", new Vector3(-0.300f, -0.300f, 2.000f), new Vector3(0.200f, 0.300f, 9.000f)));
            trials.Add(new Trial("D1", "F2S", new Vector3(-0.300f, -0.300f, 9.000f), new Vector3(0.200f, 0.300f, 2.000f)));
            trials.Add(new Trial("D1", "F2M", new Vector3(-0.300f, -0.300f, 9.000f), new Vector3(0.200f, 0.300f, 2.000f)));
            trials.Add(new Trial("D1", "M2S", new Vector3(-0.300f, -0.300f, 2.000f), new Vector3(0.200f, 0.300f, 9.000f)));
            trials.Add(new Trial("D1", "M2F", new Vector3(-0.300f, -0.300f, 2.000f), new Vector3(0.200f, 0.300f, 9.000f)));
            trials.Add(new Trial("D1", "F2S", new Vector3(-0.300f, -0.300f, 9.000f), new Vector3(0.200f, 0.300f, 2.000f)));
            trials.Add(new Trial("D1", "F2M", new Vector3(-0.300f, -0.300f, 9.000f), new Vector3(0.200f, 0.300f, 2.000f)));
            StartTrial();
        }

        void Update()
        {
            if (TRIALS_READY)
            {
                if (ON_COUNTDOWN)
                {
                    deltaTimeCounter += Time.deltaTime;
                    if (deltaTimeCounter > 1)
                    {
                        UITextManager.Instance.PrintMessage(countdownSecLeft.ToString());
                        countdownSecLeft--;
                        deltaTimeCounter = 0f;
                    }
                    if (countdownSecLeft <= 0)
                    {
                        ON_COUNTDOWN = false;
                        EventHandler handler = CountdownEnd;
                        if (handler != null)
                        {
                            handler(this, EventArgs.Empty);
                        }
                    }
                }
                else if (isTrialFinished())
                {
                    EventHandler handler = TrialComplete;
                    if (handler != null)
                    {
                        handler(this, EventArgs.Empty);
                    }
                }
            }
        }

        private bool isTrialFinished()
        {
            return RepositionManager.Instance.GetItemStatusMode(itemObj.GetInstanceID()) == ItemStatusModes.IDLE
                    && RepositionManager.Instance.GetWallStatusMode(wallObj.GetInstanceID()) == WallStatusModes.IDLE
                    && Vector3.Magnitude(targetObj.transform.position - itemObj.transform.position) < leastDistance;
        }

        public void AddEventLog(LogEvent logEvent)
        {
            if(!SEND_LOG || ON_COUNTDOWN)
            {
                return;
            }
            string message = "";
            message += UserId + "\t";
            message += method + "\t";
            message += trialIdx + "\t";
            message += currXYType + "\t";
            message += currZType + "\t";
            message += (Time.time - trialStartTime).ToString("F4") + "\t";
            message += handMoveVector.x.ToString("F4") + "\t";
            message += handMoveVector.y.ToString("F4") + "\t";
            message += handMoveVector.z.ToString("F4") + "\t";
            message += logEvent.ToString() + "\t";
            Debug.Log(message);
            LogManager.Instance.SendLogMessage(UserId+".tsv", message);
        }

        public void SaveNewHandPosition(Vector3 newHandPos)
        {
            handMoveVector.x += Math.Abs(newHandPos.x - prevHandPos.x);
            handMoveVector.y += Math.Abs(newHandPos.x - prevHandPos.x);
            handMoveVector.z += Math.Abs(newHandPos.x - prevHandPos.x);

            prevHandPos = newHandPos;
        }

        private void ExperimentManager_CounddownEnd (object source, EventArgs args)
        {
            trialStartTime = Time.time;
            handMoveVector = Vector3.zero;
            AddEventLog(LogEvent.TRIAL_START);
        }

        private void ExperimentManager_TrialComplete (object source, EventArgs args)
        {
            //Debug.Log("Trial complete, " + trialIdx);
            UITextManager.Instance.PrintSuccess();
            AddEventLog(LogEvent.TRIAL_END);
            trialIdx++;
            if (trialIdx >= trials.Count)
            {
                DebugTextController.Instance.SetMessage("Done!");
                TRIALS_READY = false;
                return;
            }

            StartTrial();
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
            itemObj.transform.localScale = ItemOriginScale;
            SetStartPos(Vector3.forward * 3f + Vector3.right * -0.5f);

            // set target
            if (targetObj != null)
            {
                Destroy(targetObj);
                targetObj = null;
            }
            //GameObject targetInst = Instantiate(targetPrefab);
            //targetObj = new GameObject();
            //targetInst.transform.parent = targetObj.transform;
            //targetInst.transform.localPosition = new Vector3(0f, -0.125f, 0f);
            //targetObj.transform.localRotation = Quaternion.Euler(-60f, 0f, 0f);
            //targetObj.transform.parent = gameObject.transform;
            targetObj = Instantiate(targetPrefab);
            targetObj.transform.parent = gameObject.transform;
            targetObj.transform.localScale = ItemOriginScale;
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

            // set invisible wall
            if (leftWall != null)
            {
                Destroy(leftWall);
                leftWall = null;
            }
            leftWall = Instantiate(invisibleWallPrefab);
            leftWall.transform.parent = gameObject.transform;
            leftWall.layer = SpatialMappingManager.Instance.PhysicsLayer;
            leftWall.transform.position = new Vector3(-0.15f, 0.62f, 5.5f);
            leftWall.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            leftWall.transform.localScale = new Vector3(12f, 3f, 0.01f);
            if (rightWall != null)
            {
                Destroy(rightWall);
                rightWall = null;
            }
            rightWall = Instantiate(invisibleWallPrefab);
            rightWall.transform.parent = gameObject.transform;
            rightWall.layer = SpatialMappingManager.Instance.PhysicsLayer;
            rightWall.transform.position = new Vector3(3.65f, 0.62f, 5.5f);
            rightWall.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            rightWall.transform.localScale = new Vector3(12f, 3f, 0.01f);
            if (ceil != null)
            {
                Destroy(ceil);
                ceil = null;
            }
            ceil = Instantiate(invisibleWallPrefab);
            ceil.transform.parent = gameObject.transform;
            ceil.layer = SpatialMappingManager.Instance.PhysicsLayer;
            ceil.transform.position = new Vector3(1.64f, 2.21f, 5.5f);
            ceil.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            ceil.transform.localScale = new Vector3(3.5f, 12f, 0.01f);
            if (floor != null)
            {
                Destroy(floor);
                floor = null;
            }
            floor = Instantiate(invisibleWallPrefab);
            floor.transform.parent = gameObject.transform;
            floor.layer = SpatialMappingManager.Instance.PhysicsLayer;
            floor.transform.position = new Vector3(1.64f, -1.25f, 5.5f);
            floor.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            floor.transform.localScale = new Vector3(3.5f, 12f, 0.01f);

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

        private void StartTrial()
        {
            if (trials == null || trials.Count == 0 || trials.Count <= trialIdx)
            {
                Debug.LogError("trials is null or empty or less than trialNumber");
                return;
            }
            if (itemObj == null || targetObj == null)
            {
                Debug.LogError("itemObj == null or targetObj == null while StartCurrentTrial");
                return;
            }

            Trial trial = trials[trialIdx];

            SetStartPos(trial.startPos);
            SetTargetPos(trial.targetPos);

            currXYType = trial.xy_type;
            currZType = trial.z_type;

            DebugTextController.Instance.SetMessage("ID: " + UserId);
            DebugTextController.Instance.AddMessage("Method: " + method);
            DebugTextController.Instance.AddMessage("Trial: " + trialIdx + "/" + trials.Count);

            deltaTimeCounter = 0f;
            countdownSecLeft = CountdownDuration;
            ON_COUNTDOWN = true;
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
            if (!RepositionManager.Instance.IsWallAvailable)
            {
                return;
            }

            GameObject targetInitObj = RepositionManager.Instance.GetItemInitObject(targetObj.GetInstanceID());

            targetInitObj.transform.position = target + OriginPos;
        }

        public void SetTrialList(SequenceData sd)
        {
            UserId = int.Parse(sd.id);
            trials.Clear();
            trialIdx = 0;
            method = sd.method;
            if(sd.mode == "p")
            {
                SEND_LOG = false;
            }
            else
            {
                SEND_LOG = true;
            }
            SetInteractionMethod(sd.method);
            
            foreach (SequenceData.TrialString trial in sd.trials)
            {
                Vector3 startPos = new Vector3(float.Parse(trial.start[0]), float.Parse(trial.start[1]), float.Parse(trial.start[2]));
                Vector3 targetPos = new Vector3(float.Parse(trial.target[0]), float.Parse(trial.target[1]), float.Parse(trial.target[2]));
                trials.Add(new Trial(trial.xy_type, trial.z_type, startPos, targetPos));
            }

            TRIALS_READY = true;
            StartTrial();
        }

        public GameObject GetItemObject()
        {
            return itemObj;
        }

        public GameObject GetWallObject()
        {
            return wallObj;
        }

        public GameObject GetTargetObject()
        {
            return targetObj;
        }
        
        public void ResetScene()
        {
            UDPManager.Instance.DisposeSocket();
            DebugTextController.Instance.SetMessage("Scene reloaded");
            SceneManager.LoadScene("Main");
        }

        public void SetItem(string type)
        {
            if (type == "cube")
            {
                itemObj.GetComponent<MeshFilter>().sharedMesh = null;
                itemObj.GetComponent<MeshFilter>().sharedMesh = itemCube.GetComponent<MeshFilter>().sharedMesh;
                targetObj.GetComponent<MeshFilter>().sharedMesh = null;
                targetObj.GetComponent<MeshFilter>().sharedMesh = targetCube.GetComponent<MeshFilter>().sharedMesh;
            }
            else if (type == "sphere")
            {
                itemObj.GetComponent<MeshFilter>().sharedMesh = null;
                itemObj.GetComponent<MeshFilter>().sharedMesh = itemSphere.GetComponent<MeshFilter>().sharedMesh;
                targetObj.GetComponent<MeshFilter>().sharedMesh = null;
                targetObj.GetComponent<MeshFilter>().sharedMesh = targetSphere.GetComponent<MeshFilter>().sharedMesh;
            }
        }

        public void SetScale(string scaleStr)
        {
            try
            {
                float scale = float.Parse(scaleStr);
                leastDistance = scale;
                ItemOriginScale = new Vector3(scale, scale, scale);
                itemObj.transform.localScale = ItemOriginScale;
                targetObj.transform.localScale = ItemOriginScale;
            }
            catch (Exception e)
            {
                Debug.Log("Experiment.SetScale got exception");
                return;
            }
        }

        public bool IsOnCountdown()
        {
            return ON_COUNTDOWN;
        }
    }
}
