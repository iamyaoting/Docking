using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Docking;

public class SwingCon : StateBehavioConBase
{
    protected override void OnControllerExit(int layerIndex, AnimatorStateInfo stateInfo)
    {                
        var context = m_dockingDetector.GetNearestDockingTargetBySingleType(DetectorType.ForwardBottomDetector, new Vector2(1, -1), m_dockingDriver.GetDockingTarget());
        m_dockingDriver.SetDockingTarget(context.dockingtarget); 
    }    
}
