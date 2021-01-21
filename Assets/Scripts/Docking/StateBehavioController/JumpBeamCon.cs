using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpBeamCon : StateBehavioConBase
{
    bool m_firstUpdate = true;

    protected override void OnControllerEnter(int layerIndex)
    {
        m_firstUpdate = true;
        base.OnControllerEnter(layerIndex);
    }

    protected override void OnControllerUpdate(int layerIndex)
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
