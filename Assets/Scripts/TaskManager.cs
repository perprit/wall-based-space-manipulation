using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HoloToolkit.Unity;
namespace ManipulateWalls
{
    public enum InteractionType
    {
        ADAPT, CONST, DIST
    }

    public enum ZDistType
    {
        S2M, S2F, M2F, M2S, F2S, F2M
    }

    public enum XYPosType
    {
        I2I, I2O, O2I, O2O
    }

    struct TaskSetting
    {
        InteractionType interactionType;
        ZDistType zDistType;
        XYPosType xyPosType;
        Vector3 ItemPosition;
        Vector3 TargetPosition;

        TaskSetting(InteractionType _it, ZDistType _zd, XYPosType _xy, Vector3 _ip, Vector3 _tp)
        {
            interactionType = _it;
            zDistType = _zd;
            xyPosType = _xy;
            ItemPosition = _ip;
            TargetPosition = _tp;
        }
    }

    public class TaskManager : Singleton<TaskManager>
    {
        /*
        public List<TaskSetting> LoadTaskSequence(int userId)
        {
        }
        */
    }
}
