using Docking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BracedHangingIdleCon : BracedHangingConBase
{
    protected override void OnControllerEnter(int layerIndex, AnimatorStateInfo stateInfo)
    {
        Debug.Log("Idle");
        base.OnControllerEnter(layerIndex, stateInfo);
    }

    protected override void OnControllerUpdate(int layerIndex, AnimatorStateInfo stateInfo)
    {        
        if (m_animator.IsInTransition(layerIndex)) return;        
        
        var input = GetRawInput();
        if (input.magnitude == 0) return;

        var limit = m_dockingDriver.GetDockedVertexStatus().limit;
        var input2 = HandleInputLimit(input, limit);
        if (HasEnvCommitAction())
        {
            FindNextDockedHangTarget(input);
        }
        else if (input2.x != 0)
        {            
            m_animator.SetTrigger("T_HangingMove");
            m_animator.SetFloat("Velocity", input2.x > 0 ? 1 : -1);
        }
        else if(HasEnvUnCommitAction())
        {
            SetUnDocking();
        }
    }
}
