using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Docking
{   
    public class BracedHangController : DockingController
    {
        enum STATE
        {
            IN_BRACEDHANG,
            BRACEDHANG_IDLE,
            BRACEDHANG_SHIMMY,
            BRACEDHANG_HOP,
            OUT_BRACEDHANG,
            ERROR
        }

        private int[] onIdleStateHash;
        private int[] onMoveStateHash;
        private int[] outStateHashes;
        private int[] inStateHashes;
        private int[] hopStateHashes;
       
        private STATE state;

        public override void OnInit(ControllerInitContext context)
        {
            string[] strs = { "BracedHang.Braced Hang Shimmy" };
            onMoveStateHash = Utils.GetStateMachineStateHash(strs);

            strs = new string[] { "BracedHang.Braced Hang Idle" };
            onIdleStateHash = Utils.GetStateMachineStateHash(strs);

            string[] instrs = { "BracedHang.Idle To Braced Hang" };
            inStateHashes = Utils.GetStateMachineStateHash(instrs);

            string[] outstrs = { "BracedHang.Braced Hang To Stand" };
            outStateHashes = Utils.GetStateMachineStateHash(outstrs);

            string[] hopStrs = { "BracedHang.Braced Hang Hop Up", "BracedHang.Braced Hang Hop Down" };
            hopStateHashes = Utils.GetStateMachineStateHash(hopStrs);

            state = STATE.IN_BRACEDHANG;

            base.OnInit(context);
        }

        public override void OnEnter(ControllerEnterContext context)
        {
            // 向动画图执行命令
            m_animator.SetTrigger("Commit");           
            base.OnEnter(context);
            // Debug.Break();
        }

        public override void Tick(float deltaTime)
        {
            if (!active) return;

            var newState = GetState();           
            var input = GetRawInput();
            if (IsSpeedUpActionPressed()) input = 2 * input;
            var vel = Mathf.RoundToInt(input.x);

            if (STATE.BRACEDHANG_IDLE == newState)
            {
                m_animator.ResetTrigger("T_BracedHangIdle");
                m_animator.SetFloat("Velocity", vel);

                if(HasEnvUnCommitAction()) // 跳下至地面
                {
                    m_animator.SetTrigger("UnCommit");                      
                }
                else if(HasEnvCommitAction()) // 跳到下一个target
                {
                    if (input.magnitude != 0)
                    {
                        input.Normalize();
                        m_nextControllerEnterContext = m_dockingDetector.GetNearestDockingTarget_Hanging(input, m_dockingDriver.GetDockingTarget());
                        if (null == m_nextControllerEnterContext) return;
                        m_nextControllerType = Docking.Utils.GetDefaultControllerTypeByTargetType(m_nextControllerEnterContext.dockingtarget.m_type);

                        m_animator.SetFloat("BracedHangHopX", input.x);
                        m_animator.SetFloat("BracedHangHopY", input.y);
                        Debug.Log(input);
                        //Debug.Break();
                    }
                }
                else if (vel != 0) // 切换至hanging move状态
                {
                    m_animator.SetTrigger("T_BracedHangShimmy");
                }                
            }
            
            if(STATE.BRACEDHANG_SHIMMY == newState)
            {
                if (vel == 0)
                {
                    var ntime = Utils.GetCurrentStateNormalizedTime(m_animator, 0);
                    if ((ntime < 0.3f && ntime > 0.25f) || ntime > 0.85f)
                    {
                        //Debug.Log(ntime);
                        m_animator.SetTrigger("T_BracedHangIdle");
                    }
                    m_animator.ResetTrigger("T_BracedHangShimmy");
                }

                // 处理Hand IK行为

            }           

            // 在HandingToStang状态下且UnDocked状态下，进行指定target
            if(STATE.OUT_BRACEDHANG == newState && STATE.BRACEDHANG_IDLE == state)
            {
                var dockingBoneTrans = Docking.Utils.GetDockingBoneTransform(m_animator);
                var context = CreateFloorVertexTarget(dockingBoneTrans.position, dockingBoneTrans.rotation);
                base.OnEnter(context);
                active = true;
            }

            state = newState;
        }

        // 当前State处于哪一个阶段
        private STATE GetState()
        {
            if (Utils.IsCurrentState(m_animator, onIdleStateHash)) return STATE.BRACEDHANG_IDLE;
            if (Utils.IsCurrentState(m_animator, onMoveStateHash)) return STATE.BRACEDHANG_SHIMMY;
            if (Utils.IsCurrentState(m_animator, inStateHashes)) return STATE.IN_BRACEDHANG;
            if (Utils.IsCurrentState(m_animator, outStateHashes)) return STATE.OUT_BRACEDHANG;
            if (Utils.IsCurrentState(m_animator, hopStateHashes)) return STATE.BRACEDHANG_HOP;

            Debug.LogError("Braced Hang Error, Controller and State machine async！");
            return STATE.ERROR;
        }

        // 当前Out Vault动画State已经结束了，该控制器也结束了
        public override void OnFSMStateExit() 
        {
            if(state == STATE.OUT_BRACEDHANG)
            {
                m_nextControllerEnterContext = null;
                m_nextControllerType = typeof(IdleController);
            }            
        }
        protected override void OnDockingTargetUpdate(DockingTarget target, TR tr, DockedVertexStatus status)
        {
            bool handIK = false;
            DockingLineStripTarget lineStripTarget = target as DockingLineStripTarget;
            if (lineStripTarget)
            {
                handIK = lineStripTarget.m_handIK;
            }            
            m_fullBodyIKModifer.SetEnableIK(handIK);
            if (handIK)
            {
                var leftHand = m_animator.GetBoneTransform(HumanBodyBones.LeftHand);
                var rightHand = m_animator.GetBoneTransform(HumanBodyBones.RightHand);

                var leftHandDocked = lineStripTarget.GetDockedPointWS(leftHand.position, leftHand.rotation);
                var rightHandDocked = lineStripTarget.GetDockedPointWS(rightHand.position, rightHand.rotation);

                m_fullBodyIKModifer.leftHandTargetOffset = Vector3.up * (leftHandDocked.translation.y - tr.translation.y);
                m_fullBodyIKModifer.rightHandTargetOffset = Vector3.up * (rightHandDocked.translation.y - tr.translation.y);           
            }
            base.OnDockingTargetUpdate(target, tr, status);
        }
    }

}
