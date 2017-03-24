using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity;

namespace ManipulateWalls
{
    public class VirtualItemsManager : Singleton<VirtualItemsManager>
    {

        private List<Transform> childTransforms = new List<Transform>();

        void Start()
        {
            Transform[] transforms = gameObject.GetComponentsInChildren<Transform>();
            for(int i=0; i< transforms.Length; i++)
            {
                // every child objects (except itself)
                if (transforms[i].GetInstanceID() != gameObject.transform.GetInstanceID())
                {
                    childTransforms.Add(transforms[i]);
                }
            }
        }
        
        void Update()
        {

        }

        public List<Transform> GetAllObjectTransforms()
        {
            return childTransforms;
        }

        public List<Vector3> GetAllObjectPositions()
        {
            List<Vector3> positions = new List<Vector3>();
            for(int i=0; i<childTransforms.Count; i++)
            {
                positions.Add(childTransforms[i].position);
            }
            return positions;
        }
    }
}
