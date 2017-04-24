using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity;

namespace ManipulateWalls
{
    public class VirtualItemsManager : Singleton<VirtualItemsManager>
    {
        private List<GameObject> childObjects = new List<GameObject>();
        void Start()
        {
            Transform[] transforms = gameObject.GetComponentsInChildren<Transform>();
            for(int i=0; i< transforms.Length; i++)
            {
                // every child objects (except itself)
                if (transforms[i].GetInstanceID() != gameObject.transform.GetInstanceID())
                {
                    childObjects.Add(transforms[i].gameObject);
                }
            }
        }
        
        void Update()
        {

        }

        public List<GameObject> GetItemObjects()
        {
            return childObjects;
        }
    }
}
