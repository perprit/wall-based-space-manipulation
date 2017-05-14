using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;

namespace ManipulateWalls
{
    public class HandIndicatorManager : Singleton<HandIndicatorManager>, ISourceStateHandler
    {
        class Hand
        {
            public Hand(IInputSource inputSource_, uint inputSourceId_, GameObject indicator_)
            {
                inputSource = inputSource_;
                inputSourceId = inputSourceId_;
                indicator = indicator_;
            }

            public IInputSource inputSource;
            public uint inputSourceId;
            public GameObject indicator;
        }

        public GameObject HandIndicatorPrefab;

        private Dictionary<uint, Hand> handDictionary = new Dictionary<uint, Hand>();

        void Start()
        {
            InputManager.Instance.AddGlobalListener(gameObject);
            if (HandIndicatorPrefab == null)
            {
                Debug.LogError("Please include a GameObject for the HandIndicator");
            }
        }
        
        void Update()
        {
            // update every position of indicators in handDictionary
            foreach(KeyValuePair<uint, Hand> entry in handDictionary)
            {
                Hand hand = entry.Value;
                Vector3 indicatorPosition;
                hand.inputSource.TryGetPosition(entry.Key, out indicatorPosition);

                // save hand position to record hand movement distance
                ExperimentManager.Instance.SaveNewHandPosition(indicatorPosition);

                // indicator must be farther than 0.85m in z-axis (near clipping plane) to be able to be seen
                indicatorPosition = Camera.main.transform.InverseTransformPoint(indicatorPosition);
                indicatorPosition += new Vector3(0, 0, 0.85f);
                indicatorPosition = Camera.main.transform.TransformPoint(indicatorPosition);
                hand.indicator.transform.position = indicatorPosition;
            }
        }

        public void OnSourceDetected(SourceStateEventData eventData)
        {
            if (handDictionary.ContainsKey(eventData.SourceId))
            {
                Debug.Log("handDictonary already contains key: " + eventData.SourceId + " / OnSourceDetected, HandIndicatorManager");
                return;
            }

            if (handDictionary.Count > 0)
            {
                Debug.Log("Do not allow hands to be detected more than one");
            }

            if (HandIndicatorPrefab == null)
            {
                return;
            }

            GameObject indicator = Instantiate(HandIndicatorPrefab);

            handDictionary.Add(eventData.SourceId, new Hand(eventData.InputSource, eventData.SourceId, indicator));

            ExperimentManager.Instance.AddEventLog(LogEvent.HAND_FOUND);
        }

        public void OnSourceLost(SourceStateEventData eventData)
        {
            if (!handDictionary.ContainsKey(eventData.SourceId))
            {
                Debug.Log("handDictonary does not contain key: " + eventData.SourceId + " / OnSourceDetected, HandIndicatorManager");
                return;
            }

            Hand lostHand;
            handDictionary.TryGetValue(eventData.SourceId, out lostHand);
            Destroy(lostHand.indicator);

            handDictionary.Remove(eventData.SourceId);

            ExperimentManager.Instance.AddEventLog(LogEvent.HAND_LOST);
        }

        public bool IsHandFound()
        {
            return handDictionary.Count > 0;
        }
    }
}

