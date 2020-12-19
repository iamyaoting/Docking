using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Docking
{
    public class InValutController : DockingController
    {
        public override void OnEnter(ControllerEnterContext context)
        {            
            //var desiredDockedVertex = context.desiredDockedVertex; 

            //var angle = Utils.GetYawAngle(m_animator.transform, desiredDockedVertex.tr.translation);

            //if (angle < 0)
            //{               
            //    m_animator.SetFloat("LeftRightSelctor", 1);
            //}
            //else
            //{
            //    m_animator.SetFloat("LeftRightSelctor", 0);
            //}
            base.OnEnter(context);
        }

        public override void Tick(float deltaTime)
        {        
        }

        public override void OnFSMStateExit() 
        {
            var dockingBoneTrans = Utils.GetDockingBoneTransform(m_animator);
            m_nextControllerEnterContext = GetNearestFloorVertexTarget(dockingBoneTrans);
            m_nextControllerType = typeof(OutValutController);
        }
    }

}
