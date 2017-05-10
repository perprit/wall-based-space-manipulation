using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity;

namespace ManipulateWalls
{
    public class UtilFunctions : Singleton<UtilFunctions>
    {
        public float QuadraticInOut (float k)
        {
            if ((k *= 2f) < 1f) return 0.5f * k * k;
            return -0.5f * ((k -= 1f) * (k - 2f) - 1f);
        }
    }

}