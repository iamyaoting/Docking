using Docking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BracedHangingIdleCon : BracedHangingConBase
{
    protected override void OnControllerUpdate(int layerIndex, AnimatorStateInfo stateInfo)
    {
        if (m_animator.IsInTransition(layerIndex)) return;        
        
        var input = GetRawInput();
        if (input.magnitude == 0) return;

        var limit = m_dockingDriver.GetDockedVertexStatus().limit;
        var input2 = HandleInputLimit(input, limit);
        if (HasEnvCommitAction())
        {           
            var context = m_dockingDetector.GetNearestDockingTargetBySingleType(DetectorType.HangDetector, input, m_dockingDriver.GetDockingTarget());
            if (null != context)
            {
                CrossFadeAnimatorHopState(context.desiredDockedVertex.tr.translation);
                m_dockingDriver.SetDockingNextTarget(context.dockingtarget);
            }
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
