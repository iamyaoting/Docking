using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Docking
{   
    public class JumpController : DockingController
    {
        enum STATE
        {
            JUMP,
            IDLE,
            ERROR
        }

        private int[] onIdleStateHash;
        private int[] onJumpStateHash;
       
        private STATE state;

        public override void OnInit(ControllerInitContext context)
        {
            string[] strs = { "JumpOnBeam_SingleFoot.JumpOnBeamRight", "JumpOnBeam_SingleFoot.JumpOnBeamLeft" };
            onJumpStateHash = Utils.GetStateMachineStateHash(strs);

            strs = new string[] { "JumpOnBeam_SingleFoot.Idle" };
            onIdleStateHash = Utils.GetStateMachineStateHash(strs);           

            state = STATE.IDLE;

            base.OnInit(context);
        }

        public override void OnEnter(ControllerEnterContext context)
        {
            // 向动画图执行命令
            m_animator.SetTrigger("Commit");           
            base.OnEnter(context);
            // Debug.Break();
        }

        protected override void Tick(float deltaTime)
        {
            if (!active) return;

            var newState = GetState();

            if(STATE.JUMP == newState)
            {
                if(m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.5f && m_enableInput)
                {
                    m_animator.SetTrigger("Commit");
                    var context = m_dockingDetector.GetNearestDockingTarget_Locomotion_Low(null);
                    OnEnter(context);
                    active = true;
                }                
            }
            state = newState;
        }

        // 当前State处于哪一个阶段
        private STATE GetState()
        {
            if (Utils.IsCurrentState(m_animator, onIdleStateHash))
            {
                return STATE.IDLE;
            }
            if (Utils.IsCurrentState(m_animator, onJumpStateHash))
            {
                return STATE.JUMP;
            }

            Debug.LogError("Jump Error, Controller and State machine async！");
            return STATE.ERROR;
        }

        //public override void OnFSMStateExit()
        //{
        //    if (STATE.JUMP == state)
        //    {
        //        m_nextControllerEnterContext = null;
        //        m_nextControllerType = typeof(IdleController);
        //    }
            
        //}
    }

}
