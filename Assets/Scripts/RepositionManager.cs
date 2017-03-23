using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity;

namespace HoloToolkit.Unity.InputModule
{
    public class RepositionManager : Singleton<RepositionManager>
    {
        public enum DraggableType
        {
            Wall, Object
        }

        private IInputSource wallInputSource = null;
        private uint wallInputSourceId;

        private IInputSource objectInputSource = null;
        private uint objectInputSourceId;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SetInputSource(IInputSource source, uint sourceId, DraggableType type)
        {
            if(type == DraggableType.Wall)
            {
                wallInputSource = source;
                wallInputSourceId = sourceId;
            }
            
            if (type == DraggableType.Wall)
            {
                objectInputSource = source;
                objectInputSourceId = sourceId;
            }
        }
    }
}

