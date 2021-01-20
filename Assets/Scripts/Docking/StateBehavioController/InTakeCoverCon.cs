using Docking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InTakeCoverCon : StateBehavioConBase
{
    public DetectorType m_detectorType;

    protected override void OnControllerUpdate(int layerIndex)
    {
        if (m_animator.IsInTransition(layerIndex)) return;

        if (HasEnvCommitAction())
        {
            ControllerEnterContext context = m_dockingDetector.GetNearestDockingTarget(m_detectorType, GetRawInput(), null);
            if (null != context)
            {
                m_dockingDriver.SetDockingTarget(context.dockingtarget);
                var desiredDockedVertex = context.desiredDockedVertex;
                var angle = Utils.GetYawAngle(m_animator.transform, desiredDockedVertex.tr.translation);

                if (angle < 0)
                {
                    m_animator.SetFloat("LeftRightSelctor", 1);
                }
                else
                {
                    m_animator.SetFloat("LeftRightSelctor", 0);
                }

                SetDockingCommit();
            }
        }
    }

}
