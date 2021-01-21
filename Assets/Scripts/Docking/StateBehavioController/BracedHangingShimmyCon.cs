using Docking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BracedHangingShimmyCon : BracedHangingConBase
{
    protected override void OnControllerUpdate(int layerIndex)
    {
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
        if (m_animator.IsInTransition(layerIndex)) return;
        if (input.x == 0)
        {
            m_animator.SetTrigger("T_HangingIdle");
        }
    }    
}
