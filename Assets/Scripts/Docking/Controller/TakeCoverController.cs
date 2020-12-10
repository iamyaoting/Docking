using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Docking
{
    public class TakeCoverController : DockingController
    {
        public override void OnEnter(TR desiredDockedVertex, DockedVertexStatus desiredDockedVertexStatus)
        {
            dockedVertexStatus = desiredDockedVertexStatus;

            var angle = Utils.GetYawAngle(m_animator.transform, desiredDockedVertex.translation);

            if (angle < 0)
            {
                Debug.Log(angle);
                m_animator.SetFloat("LeftRightSelctor", 1);
            }
            else
            {
                m_animator.SetFloat("LeftRightSelctor", 0);
            }
        }

        public override void Tick(float deltaTime)
        {            
            var moveDir = GetInput();
            m_animator.SetFloat("MoveDirection", moveDir.x);

            m_animator.SetFloat("Height", dockedVertexStatus.reserveFloatParam);
        }
    }

}
