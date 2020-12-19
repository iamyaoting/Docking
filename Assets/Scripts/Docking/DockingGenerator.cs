﻿using System.Collections;
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
        public HumanBodyBones       m_dockingBone = HumanBodyBones.LastBone;

        public Vector3              m_translationOffset;
        public Quaternion           m_rotationOffset = Quaternion.identity;

        public BlendType            m_blendType;

        [HideInInspector]
        public DockingFlagBits      m_flags;
        
        public float                m_intervalStartNormalizedTime = 0;   // docking blend 混合开始归一化时间
        public float                m_intervalEndNormalizedTime = 1;     // docking blend 混合结束归一化时间

        private DockingDriver       m_dockingdriver;
        private DockingController   m_dockingController;
        

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var controllerMan = animator.GetComponent<ControllerManager>();
            if (!controllerMan) Debug.LogError("avatar has no controller mangers!");
            m_dockingdriver = controllerMan.GetDockingDriver();
            if (null == m_dockingdriver) Debug.LogError("avatar has no docking driver");
            m_dockingController = controllerMan.GetCurrentDockingController();
            if (null == m_dockingController) Debug.LogError("avatar has no docking contoller");

            // 在docking blend 期间，禁止input输入
            switch (m_blendType)
            {
                case BlendType.DOCKED_FULL_ON:
                    m_dockingController.SetEnableInput(true);
                    break;
                case BlendType.DOCKING_BLEND:
                    m_dockingController.SetEnableInput(false);
                    break;
            }

            // 存储debug信息
            m_dockingController.fsmState = AnimatorState.BLEND_IN;
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {

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
                       
            m_dockingdriver.Notify(dockingControlData);

            
            // 存储debug信息
            if (m_dockingController.fsmState == AnimatorState.BLEND_IN && !animator.IsInTransition(layerIndex))
            {
                m_dockingController.fsmState = AnimatorState.NOT_TRANSITION;
            }

            if (m_dockingController.fsmState == AnimatorState.NOT_TRANSITION && animator.IsInTransition(layerIndex))
            {
                m_dockingController.fsmState = AnimatorState.BLEND_OUT;
            }
        }

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            m_dockingController.SetEnableInput(true);
            
            // 存储debug信息
            m_dockingController.fsmState = AnimatorState.BLEND_OUT;
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
    }
}

