using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Docking
{
    public enum DockingTimeType
    {
        // 表示该动作一直都需要被docked状态
        DOCKED,
        // 表示该动作在某一时刻是要被docked状态
        DOCKING_FIXED_TIME
    }

    [System.Serializable]
    public class DockingAnimSegment
    {
        public DockingAnimSegment()
        {
            dockingBone = HumanBodyBones.LastBone;
        }

        [Header("DockingBone")]
        // docking Bone是否是双手中心，若是，上面的dockingBone则失效
        public bool isCenterofHands = false;
        
        public bool linearVelocity = false;  // 
        public HumanBodyBones dockingBone;
        

        [Header("DockingTime")]
        public DockingTimeType dockingTimeType = DockingTimeType.DOCKING_FIXED_TIME;
        // 若上面是DockingTimeType.DOCKING_FIXED_TIME，则dockedFixedTime有效
        public float dockedFixedNormalizedTime = 1.0f;

        public float startNormalizedTime = 0;
        public float endNormalizedTime = 1;
    }

    [System.Serializable]
    public class DockingAnimation{
        DockingAnimation(){            
        }
        public DockingAnimation(AnimationClip c){            
            clip = c; 
        }        
        
        public AnimationClip clip;

        public DockingAnimSegment[] segments; 
    }

    [CreateAssetMenu(fileName = "DockingAnims", menuName = "Docking/DockingAnims", order = 1)]
    public class DockingAnimationSet : ScriptableObject{
        public GameObject character;
        public DockingAnimation[] anmis;
    }
}



