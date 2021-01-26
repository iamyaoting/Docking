using Docking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class HangingIdleCon : StateBehavioConBase
{
    protected override void OnControllerUpdate(int layerIndex, AnimatorStateInfo stateInfo)
    {
        if (m_animator.IsInTransition(layerIndex)) return;        
        
        var input = GetRawInput();
        if(input.x != 0)
        {
            m_animator.SetTrigger("T_HangingMove");
            m_animator.SetFloat("Velocity", input.x > 0 ? 1 : -1);
        }
        else if(HasEnvUnCommitAction())
        {
            SetUnDocking();
        }
    }    
}
