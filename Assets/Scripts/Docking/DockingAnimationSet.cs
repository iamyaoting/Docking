using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Docking
{
    public enum DockingTimeType
    {
        // 表示该动作一直都需要被docked状态
        DOCKING_ALL_TIME,
        // 表示该动作在某一时刻是要被docked状态
        DOCKING_FIXED_TIME
    }

    [System.Serializable]
    public class DockingAnimation{
        DockingAnimation(){
            dockingBone = HumanBodyBones.LastBone;
        }
        public DockingAnimation(AnimationClip c){
            dockingBone = HumanBodyBones.LastBone;
            clip = c; 
        }        
        
        public AnimationClip clip;

        [Header("DockingBone")]
        // docking Bone是否是双手中心，若是，上面的dockingBone则失效
        public bool isCenterofHands = false;
        public HumanBodyBones dockingBone;       

        [Header("DockingTime")]
        public DockingTimeType dockingTimeType = DockingTimeType.DOCKING_FIXED_TIME;
        // 若上面是DockingTimeType.DOCKING_FIXED_TIME，则dockedFixedTime有效
        public float dockedFixedNormalizedTime = 1.0f;
    }

    [CreateAssetMenu(fileName = "DockingAnims", menuName = "Docking/DockingAnims", order = 1)]
    public class DockingAnimationSet : ScriptableObject{
        public GameObject character;
        public DockingAnimation[] anmis;
    }
}



