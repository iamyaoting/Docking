using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Docking
{
    public class DockingLineTarget : DockingTarget
    {
        public bool     m_constrainRotation = true;
        public TR       m_start;
        public TR       m_end;

        public override TR GetDcokedVertex(Transform unDockedTrans)
        {
            var posMS = transform.InverseTransformPoint(unDockedTrans.position);
            var point_start = posMS - m_start.translation;
            var end_start = m_end.translation - m_start.translation;

            var k = Vector3.Dot(end_start, point_start) / end_start.magnitude;
            k = Mathf.Clamp01(k);

            return GetTRInWS(TR.Lerp(m_start, m_end, k));
        }

        protected override void DrawGizmos()
        {
            DockingGizmos.PushGizmosData();

            var color = GetGizmosColor();           
            Gizmos.color = color;

            var startTR = GetTRInWS(m_start);
            var endTR = GetTRInWS(m_end);           

            DockingGizmos.DrawCoordinateFrame(startTR, m_coordinateFrameAsixLength);
            DockingGizmos.DrawCoordinateFrame(endTR, m_coordinateFrameAsixLength);
            Gizmos.DrawSphere(startTR.translation, m_vertexCubeSize);
            Gizmos.DrawSphere(endTR.translation, m_vertexCubeSize);

            DockingGizmos.DrawLine(startTR.translation, endTR.translation, m_lineWidth, color);

            DockingGizmos.PopGizmosData();
        }
    }

}
