using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpBeamCon : StateBehavioConBase
{
    bool m_firstUpdate = true;

    protected override void OnControllerEnter(int layerIndex, AnimatorStateInfo stateInfo)
    {
        m_firstUpdate = true;
        base.OnControllerEnter(layerIndex, stateInfo);
    }

    protected override void OnControllerUpdate(int layerIndex, AnimatorStateInfo stateInfo)
    {
        if (m_animator.IsInTransition(layerIndex)) return;

        if (m_firstUpdate)
        {
            m_dockingDriver.SwitchToNextDockingTarget();
        }

        //if(HasEnvCommitAction())
        {
            ControllerEnterContext context = m_dockingDetector.GetNearestDockingTarget(Docking.DetectorType.LowDetector, GetRawInput(), m_dockingDriver.GetDockingTarget());
            if (null != context && context.dockingtarget.m_type == Docking.DockingTargetType.BEAM)
            {
                m_dockingDriver.SetDockingNextTarget(context.dockingtarget);
                SetDockingCommit();
            }
        }

        m_firstUpdate = false;
    }
}
