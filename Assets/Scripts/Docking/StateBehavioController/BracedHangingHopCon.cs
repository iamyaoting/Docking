using Docking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BracedHangingHopCon : BracedHangingConBase
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

        if(m_firstUpdate)
        {
            m_dockingDriver.SwitchToNextDockingTarget();
        }

        m_firstUpdate = false;
    }    

}
