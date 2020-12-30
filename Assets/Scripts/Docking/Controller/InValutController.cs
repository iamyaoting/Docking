using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Docking
{
    public class InValutController : DockingController
    {
        private DockingQuadVaultTarget target;
        public override void OnEnter(ControllerEnterContext context)
        {
            // 向动画图执行命令
            m_animator.SetTrigger("Commit");

            target = (DockingQuadVaultTarget)context.dockingtarget;
            base.OnEnter(context);
        }

        public override void Tick(float deltaTime)
        {        
        }

        public override void OnFSMStateExit() 
        {
            var dockingBoneTrans = Docking.Utils.GetDockingBoneTransform(m_animator);
            var landPoint = target.GetDesiredLandHintTRWS(dockingBoneTrans.position, dockingBoneTrans.rotation);

            m_nextControllerEnterContext = CreateFloorVertexTarget(landPoint.translation, landPoint.rotation);
            m_nextControllerType = typeof(OutValutController);
            //Time.timeScale = 0;
        }
    }

}
