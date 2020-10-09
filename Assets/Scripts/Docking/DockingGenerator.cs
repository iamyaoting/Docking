using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Docking
{
    public enum BlendType
    {
        DOCKING_BLEND,      // docking correction is being blended
        DOCKED_FULL_ON      // docking correction is fully on
    }

    public enum DockingFlagBits
    {
        FLAG_NONE                       = 0x0,
        FLAG_DOCK_TO_FUTURE_POSITION    = 0x1,
        FLAG_OVERRIDE_MOTION            = 0x2
    }

    public class DockingGenerator : StateMachineBehaviour
    {
        // lastBone表示root
        public HumanBodyBones   m_dockingBone = HumanBodyBones.LastBone;

        public Vector3          m_translationOffset;
        public Quaternion       m_rotationOffset;

        public BlendType        m_blendType;

        [HideInInspector]
        public DockingFlagBits  m_flags;

        //noramlized time
        public float            m_intervalStartLocalTime = 0;
        public float            m_intervalEndLocalTime = 1;

        private float           m_localTime = 0;
        private float           m_previousLocalTime = 0;
      

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    
        //}

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    
        //}

        // OnStateMove is called right after Animator.OnAnimatorMove()
        //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    // Implement code that processes and affects root motion
        //}

        // OnStateIK is called right after Animator.OnAnimatorIK()
        //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    // Implement code that sets up animation IK (inverse kinematics)
        //}
    }
}

