using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity;

namespace ManipulateWalls
{
    public class UtilFunctions : Singleton<UtilFunctions>
    {
        public float QuadraticInOut (float k, float min, float max)
        {
            if (min >= max) return k;
            float v = Mathf.Clamp(k, min, max);
            v = v / (max - min);

            if ((v *= 2f) < 1f)
            {
                v = 0.5f * v * v;
            }
            else
            {
                v = -0.5f * ((v -= 1f) * (v - 2f) - 1f);
            }
            return v * (max - min);
        }
    }

}