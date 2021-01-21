using Docking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BracedHangingIdleCon : BracedHangingConBase
{
    protected override void OnControllerUpdate(int layerIndex)
    {
        if (m_animator.IsInTransition(layerIndex)) return;        
        
        var input = GetRawInput();
        if (HasEnvCommitAction())
        {           
            var context = m_dockingDetector.GetNearestDockingTarget_Hanging(input, m_dockingDriver.GetDockingTarget());
            if (null != context)
            {           
                CrossFadeAnimatorHopState(input);
                m_dockingDriver.SetDockingNextTarget(context.dockingtarget);
            }
        }
        else if (input.x != 0)
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
