using Docking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AutoComitCon : StateBehavioConBase
{   
    public DetectorType m_detectorType;

    private bool m_isCommit = false;

    protected override void OnControllerEnter(int layerIndex, AnimatorStateInfo stateInfo)
    {
        m_dockingDetector.GetNearestDockingTarget(m_detectorType, GetRawInput(), null);
        base.OnControllerEnter(layerIndex, stateInfo);
    }

    protected override void OnControllerUpdate(int layerIndex, AnimatorStateInfo stateInfo)
    {
        if (m_animator.IsInTransition(layerIndex)) return;
        
        //if (HasEnvCommitAction())
        if(!m_isCommit)
        {            
            ControllerEnterContext context = m_dockingDetector.GetNearestDockingTarget(m_detectorType, GetRawInput(), null);            
            if(null != context)
            {
                m_dockingDriver.SetDockingTarget(context.dockingtarget);
                m_dockingDriver.SetDockingNextTarget(context.dockingtarget);
                SetDockingCommit();
                m_isCommit = true;
            }
        }
    }    
}
