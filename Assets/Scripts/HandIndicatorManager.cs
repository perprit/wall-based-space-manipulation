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
                //DebugTextController.Instance.SetMessage("Camera: " + Camera.main.transform.position.ToString("F3"));
                //DebugTextController.Instance.AddMessage("Source: " + Camera.main.transform.InverseTransformPoint(indicatorPosition).ToString("F3"));
                // indicator must be farther than 0.85m in z-axis (near clipping plane) to be able to be seen
                indicatorPosition = Camera.main.transform.InverseTransformPoint(indicatorPosition);
                indicatorPosition += new Vector3(0, 0, 0.85f);
                indicatorPosition = Camera.main.transform.TransformPoint(indicatorPosition);
                hand.indicator.transform.position = indicatorPosition;
            }
        }

        public void OnSourceDetected(SourceStateEventData eventData)
        {
            DebugTextController.Instance.SetMessage("SourceDetected, id: " + eventData.SourceId);
            if (handDictionary.ContainsKey(eventData.SourceId))
            {
                Debug.Log("handDictonary already contains key: " + eventData.SourceId + " / OnSourceDetected, HandIndicatorManager");
                return;
            }

            if (HandIndicatorPrefab == null)
            {
                return;
            }

            GameObject indicator = Instantiate(HandIndicatorPrefab);

            handDictionary.Add(eventData.SourceId, new Hand(eventData.InputSource, eventData.SourceId, indicator));
        }

        public void OnSourceLost(SourceStateEventData eventData)
        {
            DebugTextController.Instance.SetMessage("Source Lost, id: " + eventData.SourceId);
            if (!handDictionary.ContainsKey(eventData.SourceId))
            {
                Debug.Log("handDictonary does not contain key: " + eventData.SourceId + " / OnSourceDetected, HandIndicatorManager");
                return;
            }

            Hand lostHand;
            handDictionary.TryGetValue(eventData.SourceId, out lostHand);
            Destroy(lostHand.indicator);

            handDictionary.Remove(eventData.SourceId);
        }
    }
}

