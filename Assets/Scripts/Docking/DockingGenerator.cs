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
        // 固有信息
        // lastBone表示root
        //public HumanBodyBones       m_dockingBone = HumanBodyBones.LastBone;

        public Vector3              m_translationOffset;
        public Quaternion           m_rotationOffset = Quaternion.identity;

        public BlendType            m_blendType;

        [HideInInspector]
        public DockingFlagBits      m_flags;
        
        public float                m_intervalStartNormalizedTime = 0;   // docking blend 混合开始归一化时间
        public float                m_intervalEndNormalizedTime = 1;     // docking blend 混合结束归一化时间

        
        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            SetInputMode(animator);

            float normalizedTime = stateInfo.normalizedTime;
            float localTime = normalizedTime * stateInfo.length;
            float preLocalTime = Mathf.Clamp(localTime - Time.deltaTime, 0, localTime);
            float preNomalizedTime = preLocalTime / stateInfo.length;

            DockingControlData dockingControlData = new DockingControlData();
            dockingControlData.m_dockingBlend = GetDockingBlendWeight(normalizedTime);
            dockingControlData.m_previousDockingBlend = GetDockingBlendWeight(preNomalizedTime);
            //dockingControlData.m_dockingBone = m_dockingBone;
            //dockingControlData.m_timeOffset = Time.deltaTime;
            dockingControlData.m_targetOffsetMS = new DockingTransform();
            dockingControlData.m_targetOffsetMS.translation = m_translationOffset;
            dockingControlData.m_targetOffsetMS.rotation = m_rotationOffset;
                       
            GetDockingDriver(animator).Notify(dockingControlData);
        }

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            GetCurrentDockingController(animator).OnFSMStateExit();
        }

        //// OnStateMove is called right after Animator.OnAnimatorMove()
        //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    //Implement code that processes and affects root motion

        //}

        // OnStateIK is called right after Animator.OnAnimatorIK()
        //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    // Implement code that sets up animation IK (inverse kinematics)
        //}       
        private float GetDockingBlendWeight(float normalizedTime)
        {
            normalizedTime = normalizedTime - Mathf.Floor(normalizedTime);

            if (normalizedTime < m_intervalStartNormalizedTime || normalizedTime > m_intervalEndNormalizedTime) return 0;

            float blendWeight = 0;
            switch (m_blendType)
            {
                case BlendType.DOCKED_FULL_ON:
                    blendWeight = 1.0f;
                    break;
                case BlendType.DOCKING_BLEND:
                    blendWeight = (normalizedTime - m_intervalStartNormalizedTime)
                        / (m_intervalEndNormalizedTime - m_intervalStartNormalizedTime);
                    break;
            }
            return blendWeight;
        }    
        
        private void SetInputMode(Animator animator)
        {
            // 在docking blend 期间，禁止input输入
            switch (m_blendType)
            {
                case BlendType.DOCKED_FULL_ON:
                    GetCurrentDockingController(animator).SetEnableInput(true);
                    break;
                case BlendType.DOCKING_BLEND:
                    GetCurrentDockingController(animator).SetEnableInput(false);
                    break;
            }
        }


        private Docking.DockingDriver GetDockingDriver(Animator animator)
        {
            var controllerMan = animator.GetComponent<ControllerManager>();
            if (!controllerMan) Debug.LogError("avatar has no controller mangers!");
            return controllerMan.GetDockingDriver();
        }

        private Docking.DockingController GetCurrentDockingController(Animator animator)
        {
            var controllerMan = animator.GetComponent<ControllerManager>();
            if (!controllerMan) Debug.LogError("avatar has no controller mangers!");
            return controllerMan.GetCurrentDockingController();
        }
    }
}

