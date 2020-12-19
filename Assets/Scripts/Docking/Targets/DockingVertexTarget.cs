using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Docking
{
    public class DockingVertexTarget : DockingTarget
    {
        public bool                 m_constrainRotation = true;
        public DockingVertex        m_desiredVertex;        

        public override DockedVertexStatus GetDockedLS(TR undockedTRLS, out DockingVertex dockedVertexLS)
        {
            dockedVertexLS = new DockingVertex(m_desiredVertex);
            return null;
        }
        
        protected override void DrawGizmos()
        {
            DockingGizmos.PushGizmosData();

            var color = GetGizmosColor();           
            Gizmos.color = color;

            var tr = GetTRInWS(m_desiredVertex.tr);                        
            DockingGizmos.DrawCoordinateFrameWS(tr);           
            DockingGizmos.PopGizmosData();
        }
    }

}
