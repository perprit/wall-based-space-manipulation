using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HoloToolkit.Unity;

namespace ManipulateWalls
{
    public enum InteractionType
    {
        GGI, HOMER, GR
    }

    public enum RepositionType
    {
        C2C, C2F, F2C, F2F
    }

    public class ExperimentManager : Singleton<ExperimentManager>
    {
        class TaskSetting
        {
            InteractionType interactionType;
            RepositionType repositionType;

            TaskSetting(InteractionType _it, RepositionType _rt)
            {
                interactionType = _it;
                repositionType = _rt;
            }
        }

        public InteractionType interactionType = InteractionType.GR;
        public RepositionType repositionDirectionType = RepositionType.C2F;

        private List<TaskSetting> taskSettingList = new List<TaskSetting>();

        void Start()
        {

        }
        
        void SetTaskSettingSequence(List<TaskSetting> _tsl)
        {
            taskSettingList = _tsl;
        }
    }
}
