using Docking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BracedHangingHopCon : BracedHangingConBase
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

        if(m_firstUpdate)
        {
            m_dockingDriver.SwitchToNextDockingTarget();
        }

        m_firstUpdate = false;
    }    

}
