using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Docking;

public class VaultCon : StateBehavioConBase
{
    private bool m_isDetectGround = false;

    protected override void OnControllerUpdate(int layerIndex, AnimatorStateInfo stateInfo)
    {
        if (m_animator.IsInTransition(layerIndex)) return;

        if (m_isDetectGround) return;

        var dockingBoneTrans = Docking.Utils.GetDockingBoneTransform(m_animator);
        var target = m_dockingDriver.GetDockingTarget() as DockingQuadVaultTarget;
        var landPoint = target.GetDesiredLandHintTRWS(dockingBoneTrans.position, dockingBoneTrans.rotation);
        var context = CreateFloorVertexTarget(landPoint.translation, landPoint.rotation);
        m_dockingDriver.SetDockingTarget(context.dockingtarget);
        m_isDetectGround = true;
       
    }    
}
