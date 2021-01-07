﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Docking
{   
    public class HangingController : DockingController
    {
        enum STATE
        {
            IN_HANGING,
            HANGING_IDLE,
            HANGING_MOVE,
            OUT_HANGING,
            ERROR
        }

        private int[] onIdleStateHash;
        private int[] onMoveStateHash;
        private int[] outStateHashes;
        private int[] inStateHashes;
       
        private STATE state;

        public override void OnInit(ControllerInitContext context)
        {
            string[] strs = { "Hanging.Hanging"};
            onMoveStateHash = Utils.GetStateMachineStateHash(strs);

            strs = new string[] {"Hanging.HangingIdle" };
            onIdleStateHash = Utils.GetStateMachineStateHash(strs);

            string[] instrs = { "Hanging.Stand To Hanging" };
            inStateHashes = Utils.GetStateMachineStateHash(instrs);

            string[] outstrs = { "Hanging.Hanging To Stand" };
            outStateHashes = Utils.GetStateMachineStateHash(outstrs);

            state = STATE.IN_HANGING;

            base.OnInit(context);
        }

        public override void OnEnter(ControllerEnterContext context)
        {
            // 向动画图执行命令
            m_animator.SetTrigger("Commit");           
            base.OnEnter(context);          
        }

        public override void Tick(float deltaTime)
        {
            if (!active) return;

            var newState = GetState();

            if(STATE.HANGING_IDLE == state || STATE.HANGING_MOVE == state)
            {
                var input = GetRawInput();
                if (IsSpeedUpActionPressed()) input = 2 * input;
                var vel = Mathf.RoundToInt(input.x);
                if (STATE.HANGING_IDLE == state)
                {
                    m_animator.ResetTrigger("T_HangingIdle");
                    m_animator.SetFloat("Velocity", vel);
                    if(vel != 0)
                        m_animator.SetTrigger("T_HangingMove");
                }
                if(vel == 0  && STATE.HANGING_MOVE == state)
                {
                    m_animator.SetTrigger("T_HangingIdle");
                    m_animator.ResetTrigger("T_HangingMove");
                }
            }
            state = newState;
        }

        // 当前State处于哪一个阶段
        private STATE GetState()
        {
            if (Utils.IsCurrentState(m_animator, onIdleStateHash)) return STATE.HANGING_IDLE;
            if (Utils.IsCurrentState(m_animator, onMoveStateHash)) return STATE.HANGING_MOVE;
            if (Utils.IsCurrentState(m_animator, inStateHashes)) return STATE.IN_HANGING;
            if (Utils.IsCurrentState(m_animator, outStateHashes)) return STATE.OUT_HANGING;

            Debug.LogError("Hanging Error, Controller and State machine async！");
            return STATE.ERROR;
        }

        // 当前Out Vault动画State已经结束了，该控制器也结束了
        public override void OnFSMStateExit() 
        {
            if(state == STATE.OUT_HANGING)
            {
                m_nextControllerEnterContext = null;
                m_nextControllerType = typeof(IdleController);
            }            
        }
    }

}
