using Docking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BracedHangingShimmyCon : BracedHangingConBase
{
    private bool m_canIdle = true;
    protected override void OnControllerEnter(int layerIndex, AnimatorStateInfo stateInfo)
    {
        m_canIdle = true;
        m_dockingDriver.SwitchToNextDockingTarget();        
        base.OnControllerEnter(layerIndex, stateInfo);
        //Debug.Log("Shimmy Enter");
    }

    protected override void OnControllerUpdate(int layerIndex, AnimatorStateInfo stateInfo)
    {        
        var input = GetRawInput();
        if (HasEnvCommitAction())
        {
            var dir = input.normalized;
            FindNextDockedHangTarget(input, false);
        }
        else if (Input.GetKeyDown(KeyCode.B))
        {
            FindNextDockedHangTarget(new Vector2(0, -1), true);
        }

        if (m_animator.IsInTransition(layerIndex)) return;

        if(true == m_canIdle)
        {
            var limit = m_dockingDriver.GetDockedVertexStatus().limit;
            //var input2 = HandleInputLimit(input, limit);

            if (input.x == 0)
            {
                m_animator.SetTrigger("T_HangingIdle");
                //Debug.Log("Transistion To idle!" + input.x);
            }
        }   
        else
        {
            m_canIdle = true;
        }
    }
    protected override void OnDockingTargetMargin(DockingTarget target, TR tr, DockedVertexStatus status)
    {
        var input = GetRawInput();
        if (status.alpha > 0.8f && input.x > 0.5f && target.m_rightTarget != null)
        {      
            m_dockingDriver.SwitchNextTargetinplace(target.m_rightTarget);
            m_canIdle = false;
        }
        if (status.alpha < 0.2f && input.x < -0.5f && target.m_leftTarget != null)
        { 
            m_dockingDriver.SwitchNextTargetinplace(target.m_leftTarget);
            m_canIdle = false;
        }
        base.OnDockingTargetMargin(target, tr, status);        
    }
}
