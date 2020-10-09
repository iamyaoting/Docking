using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Docking
{
    public class DockingLineTarget : DockingTarget
    {
        public bool             m_constrainRotation = true;
        public DockingVertex    m_start;
        public DockingVertex    m_end;

        public override DockingVertex GetDcokedVertex(Transform unDockedTrans)
        {
            return new DockingVertex();
        }

        private void OnDrawGizmos()
        {
            var oldColor = Gizmos.color;
            Gizmos.color = colors[m_type];

            var startPosWS = transform.TransformPoint(m_start.position);
            var endPosWS = transform.TransformPoint(m_end.position);
            var startRotWS = transform.rotation * m_start.rotation;
            var endRotWS = transform.rotation * m_end.rotation;
            
            Gizmos.DrawLine(startPosWS, endPosWS);
            Gizmos.DrawSphere(startPosWS, sphereSize);
            Gizmos.DrawSphere(endPosWS, sphereSize);

            DockingGizmos.DrawCoordinateFrame(startPosWS, startRotWS, sphereSize * 5);
            DockingGizmos.DrawCoordinateFrame(endPosWS, endRotWS, sphereSize * 5);

            Gizmos.color = oldColor;
        }
    }

}
