using Docking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BracedHangingShimmyCon : BracedHangingConBase
{
    private bool m_alreadyProcessed = false;
    protected override void OnControllerEnter(int layerIndex, AnimatorStateInfo stateInfo)
    {        
        m_dockingDriver.SwitchToNextDockingTarget();        
        base.OnControllerEnter(layerIndex, stateInfo);
        Debug.Log("Shimmy Enter");
    }

    protected override void OnControllerUpdate(int layerIndex, AnimatorStateInfo stateInfo)
    {
        m_alreadyProcessed = false;
        var input = GetRawInput();
        if (HasEnvCommitAction())
        {
            FindNextDockedHangTarget(input);
        }
        if (m_animator.IsInTransition(layerIndex)) return;

        if(false == m_alreadyProcessed)
        {
            var limit = m_dockingDriver.GetDockedVertexStatus().limit;
            input = HandleInputLimit(input, limit);

            if (input.x == 0)
            {
                m_animator.SetTrigger("T_HangingIdle");
            }
        }       
    }
    protected override void OnDockingTargetMargin(DockingTarget target, TR tr, DockedVertexStatus status)
    {
        if (status.alpha > 0.8f)
        {
            var time = m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            m_animator.CrossFade("BracedHang.Braced Hang Shimmy", 0.0f, 0, time);
            m_dockingDriver.SetDockingNextTarget(target.m_rightTarget);
            m_alreadyProcessed = true;
        }
        if (status.alpha < 0.2f)
        {
            var time = m_animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            m_animator.CrossFade("BracedHang.Braced Hang Shimmy", 0.0f, 0, time);
            m_dockingDriver.SetDockingNextTarget(target.m_leftTarget);
            m_alreadyProcessed = true;
        }
        base.OnDockingTargetMargin(target, tr, status);
        
    }
}
