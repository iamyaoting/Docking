using Docking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class IdleCon : StateBehavioConBase
{
    public DetectorType m_detectorType;

    protected override void OnControllerUpdate(int layerIndex)
    {
        if (m_animator.IsInTransition(layerIndex)) return;
        
        if (HasEnvCommitAction())
        {            
            ControllerEnterContext context = m_dockingDetector.GetNearestDockingTarget(m_detectorType, GetRawInput(), null);            
            if(null != context)
            {
                m_dockingDriver.SetDockingTarget(context.dockingtarget);
                SetDockingCommit();
            }
        }
    }    
}
