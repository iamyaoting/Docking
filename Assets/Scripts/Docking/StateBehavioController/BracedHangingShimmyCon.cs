using Docking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BracedHangingShimmyCon : BracedHangingConBase
{
    protected override void OnControllerUpdate(int layerIndex, AnimatorStateInfo stateInfo)
    {
        var input = GetRawInput();
        if (HasEnvCommitAction())
        {
            var context = m_dockingDetector.GetNearestDockingTargetBySingleType(DetectorType.HangDetector, input, m_dockingDriver.GetDockingTarget());
            if (null != context)
            {
                CrossFadeAnimatorHopState(context.desiredDockedVertex.tr.translation);
                m_dockingDriver.SetDockingNextTarget(context.dockingtarget);
            }
        }
        if (m_animator.IsInTransition(layerIndex)) return;

        var limit = m_dockingDriver.GetDockedVertexStatus().limit;
        input = HandleInputLimit(input, limit);
        
        if (input.x == 0)
        {
            m_animator.SetTrigger("T_HangingIdle");
        }
    }    
}
