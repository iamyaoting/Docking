using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Docking
{
    public class DockingLineTarget : DockingTarget
    {
        public bool                 m_constrainRotation = true;
        public DockingVertex        m_start;
        public DockingVertex        m_end;

        public override DockingVertex GetDcokedVertex(Transform unDockedTrans)
        {
            var posMS = transform.InverseTransformPoint(unDockedTrans.position);
            var point_start = posMS - m_start.tr.translation;
            var end_start = m_end.tr.translation - m_start.tr.translation;

            var k = Vector3.Dot(end_start, point_start) / end_start.magnitude;
            k = Mathf.Clamp01(k);
            DockingVertex vertex = new DockingVertex();
            vertex.tr = GetTRInWS(TR.Lerp(m_start.tr, m_end.tr, k));
            return vertex;
        }
        public override bool IsInDetector(DockingDetector detector, out float dist)
        {
            var vertex = GetDcokedVertex(detector.hostPlayer);
            var tr = vertex.tr;
            dist = (tr.translation - detector.hostPlayer.position).magnitude;



            return detector.IsPointInDetectorWS(tr.translation);
        }

        protected override void DrawGizmos()
        {
            DockingGizmos.PushGizmosData();

            var color = GetGizmosColor();           
            Gizmos.color = color;

            var startTR = GetTRInWS(m_start.tr);
            var endTR = GetTRInWS(m_end.tr);           

            DockingGizmos.DrawCoordinateFrame(startTR, m_coordinateFrameAsixLength);
            DockingGizmos.DrawCoordinateFrame(endTR, m_coordinateFrameAsixLength);
            Gizmos.DrawSphere(startTR.translation, m_vertexCubeSize);
            Gizmos.DrawSphere(endTR.translation, m_vertexCubeSize);

            DockingGizmos.DrawLine(startTR.translation, endTR.translation, m_lineWidth, color);

            DockingGizmos.PopGizmosData();
        }
    }

}
