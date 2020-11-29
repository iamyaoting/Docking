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

        public override void GetDockedLS(TR undockedTRLS, out DockingVertex dockedVertexLS)
        {            
            var posMS = undockedTRLS.translation;
            var point_start = posMS - m_start.tr.translation;
            var end_start = m_end.tr.translation - m_start.tr.translation;

            var k = Vector3.Dot(end_start, point_start) / end_start.magnitude;
            k = Mathf.Clamp01(k);
            
            dockedVertexLS = new DockingVertex();
            dockedVertexLS.tr = TR.Lerp(m_start.tr, m_end.tr, k);
            dockedVertexLS.reserveFloatParam = Mathf.Lerp(m_start.reserveFloatParam, 
                m_end.reserveFloatParam, k);           
        }
        protected override void DrawGizmos()
        {
            DockingGizmos.PushGizmosData();

            var color = GetGizmosColor();           
            Gizmos.color = color;

            var startTR = GetTRInWS(m_start.tr);
            var endTR = GetTRInWS(m_end.tr);

            DockingGizmos.DrawLine(startTR.translation, endTR.translation, m_lineWidth, color);

            DockingGizmos.DrawCoordinateFrameWS(startTR);
            DockingGizmos.DrawCoordinateFrameWS(endTR);
            //Gizmos.DrawSphere(startTR.translation, m_vertexCubeSize);
            //Gizmos.DrawSphere(endTR.translation, m_vertexCubeSize);            

            DockingGizmos.PopGizmosData();
        }
    }

}
