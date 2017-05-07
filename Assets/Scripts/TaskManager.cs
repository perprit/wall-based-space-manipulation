using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HoloToolkit.Unity;
namespace ManipulateWalls
{
    public enum InteractionType
    {
        GOGO, CONST, DIST
    }

    public enum RepositionType
    {
        C2C, C2F, F2C, F2F
    }

    struct TaskSetting
    {
        InteractionType interactionType;
        RepositionType repositionType;
        Vector3 ItemPosition;
        Vector3 TargetPosition;

        TaskSetting(InteractionType _it, RepositionType _rt, Vector3 _ip, Vector3 _tp)
        {
            interactionType = _it;
            repositionType = _rt;
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
