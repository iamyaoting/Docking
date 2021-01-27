using Docking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class OutHangingIdleCon : StateBehavioConBase
{
    bool m_isDetectGround = false;

    protected override void OnControllerEnter(int layerIndex, AnimatorStateInfo stateInfo)
    {
        m_isDetectGround = false;
        base.OnControllerEnter(layerIndex, stateInfo);
    }

    protected override void OnControllerUpdate(int layerIndex, AnimatorStateInfo stateInfo)
    {
        if (m_animator.IsInTransition(layerIndex)) return;

        if (m_isDetectGround) return;

        var dockingBoneTrans = Docking.Utils.GetDockingBoneTransform(m_animator);
        var context = CreateFloorVertexTarget(dockingBoneTrans.position, dockingBoneTrans.rotation);
        m_dockingDriver.SetDockingTarget(context.dockingtarget);
        m_isDetectGround = true;      
    }    
}
