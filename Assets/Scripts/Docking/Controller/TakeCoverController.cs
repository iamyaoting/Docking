using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Docking
{
    public class TakeCoverController : DockingController
    {
        public override void OnEnter(ControllerEnterContext context)
        {
            // 向动画图执行命令
            m_animator.SetTrigger("Commit");

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
            base.OnEnter(context);
        }

        protected override void OnDockingTargetUpdate(DockingTarget target, TR tr, DockedVertexStatus status)
        {
            if (null != status)
            {
                m_animator.SetFloat("Height", status.reserveFloatParam);
            }
            base.OnDockingTargetUpdate(target, tr, status);
        }

        public override void Tick(float deltaTime)
        {
            var moveDir = GetInput();
            m_animator.SetFloat("MoveDirection", moveDir.x);            
        }
    }

}
