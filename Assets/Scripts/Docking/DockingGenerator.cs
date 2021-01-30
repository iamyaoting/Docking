using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Docking
{
    public enum BlendType
    {
        DOCKING_BLEND,                      // docking correction is being blended, [000->1000]
        DOCKING_BLEND_AND_DOCKED_FULL_ON,   // 在区间进行融合，后面则默认为fully docked[000->11111]
        DOCKED_FULL_ON                      // docking correction is fully on
    }

    public enum DockingFlagBits
    {
        FLAG_NONE                       = 0x0,
        FLAG_TRANSITION_FULLYDOCKED     = 0x1,   // 当transition到fully docked时候，会出现抖动，启动改选项
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
        public DockingFlagBits      m_flags;
        
        public float                m_intervalStartNormalizedTime = 0;   // docking blend 混合开始归一化时间
        public float                m_intervalEndNormalizedTime = 1;     // docking blend 混合结束归一化时间


        // 状态信息
        private float               m_lastNormalizedTime = 0;
        private float               m_lastDockingBlend = 0;

        //
        private float               m_fullyNoTransitionTime = 0;        // 该状态下没有transition的持续时间

        
        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            m_lastNormalizedTime = 0;
            m_lastDockingBlend = 0;
            m_fullyNoTransitionTime = 0;
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {            
            DockingControlData dockingControlData = new DockingControlData();
            dockingControlData.m_dockingBlend = GetDockingBlendWeight(stateInfo.normalizedTime);
            dockingControlData.m_previousDockingBlend = m_lastDockingBlend;
            //dockingControlData.m_dockingBone = m_dockingBone;
            //dockingControlData.m_timeOffset = Time.deltaTime;
            dockingControlData.m_targetOffsetMS = new DockingTransform();
            dockingControlData.m_targetOffsetMS.translation = m_translationOffset;
            dockingControlData.m_targetOffsetMS.rotation = m_rotationOffset;
            
            if(!animator.IsInTransition(layerIndex)) //不允许在Transition期间进行docked操作
            {
                m_fullyNoTransitionTime += Time.deltaTime;
                ProcessDockingControlDataFlags(dockingControlData);
                GetDockingDriver(animator).Notify(dockingControlData);
                m_lastDockingBlend = dockingControlData.m_dockingBlend;
            }

            m_lastNormalizedTime = stateInfo.normalizedTime;
            
        }

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            //GetCurrentDockingController(animator).OnFSMStateExit();
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

            if (normalizedTime < m_intervalStartNormalizedTime) return 0;

            float blendWeight = 0;
            switch (m_blendType)
            {
                case BlendType.DOCKED_FULL_ON:
                    if (normalizedTime <= m_intervalEndNormalizedTime)
                        blendWeight = 1.0f;
                    else
                        blendWeight = 0.0f;
                    break;
                case BlendType.DOCKING_BLEND:
                    if (normalizedTime <= m_intervalEndNormalizedTime)
                        blendWeight = (normalizedTime - m_intervalStartNormalizedTime)
                        / (m_intervalEndNormalizedTime - m_intervalStartNormalizedTime);
                    else
                        blendWeight = 0.0f;
                    break;
                case BlendType.DOCKING_BLEND_AND_DOCKED_FULL_ON:
                    blendWeight = (normalizedTime - m_intervalStartNormalizedTime)
                        / (m_intervalEndNormalizedTime - m_intervalStartNormalizedTime);
                    blendWeight = Mathf.Clamp01(blendWeight);
                    break;
            }
            return blendWeight;
        }    
   
        private Docking.DockingDriver GetDockingDriver(Animator animator)
        {
            var driver = animator.GetComponent<DockingDriver>();
            if (!driver) Debug.LogError("avatar has no docking drvier!");
            return driver;
        }        

        private void ProcessDockingControlDataFlags(DockingControlData dockingControlData)
        {
            if(m_flags == DockingFlagBits.FLAG_TRANSITION_FULLYDOCKED && m_blendType == BlendType.DOCKED_FULL_ON)
            {
                var alpha = m_fullyNoTransitionTime / 0.2f;
                if (alpha < dockingControlData.m_dockingBlend)
                {
                    //Debug.Log(alpha + " \\\\ " + dockingControlData.m_dockingBlend);
                    dockingControlData.m_dockingBlend = Mathf.Clamp01(alpha);
                }
            }
        }
    }
}

