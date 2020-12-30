﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Docking
{
    [System.Serializable]
    public class DockingQuadData
    {
        public Vector3 p1 = Vector3.zero;
        public Vector3 p2 = Vector3.zero;
        public Vector3 normal = Vector3.up;
        public float width = 0;

        public DockedVertexStatus GetDockedLS(TR undockedTRLS, out DockingVertex dockedVertexLS)
        {
            // 先算纵向的
            Vector3 dockedPoint;
            float k1 = 0;
            Docking.Utils.GetLineSegmentDockedPoint(p1, p2, undockedTRLS.translation, out dockedPoint, out k1);

            // 再算横向的
            float k2 = 0;
            Vector3 dir = Vector3.Cross(p2 - p1, normal).normalized;
            Vector3 p3 = dockedPoint - dir * width / 2;
            Vector3 p4 = dockedPoint + dir * width / 2;
            Docking.Utils.GetLineSegmentDockedPoint(p3, p4, undockedTRLS.translation, out dockedPoint, out k2);

            dockedVertexLS = new DockingVertex();
            dockedVertexLS.tr.translation = dockedPoint;
            dockedVertexLS.tr.rotation = Quaternion.FromToRotation(undockedTRLS.rotation * Vector3.up, normal) * undockedTRLS.rotation;
            dockedVertexLS.reserveFloatParam = 0;

            var dockedVertexSatus = new DockedVertexStatus();
            dockedVertexSatus.alpha = k1;
            dockedVertexSatus.reserveFloatParam = k2;

            return dockedVertexSatus;
        }
    }
    public class DockingQuadTarget : DockingTarget
    {
        public DockingQuadData m_quadData;

        public override DockedVertexStatus GetDockedLS(TR undockedTRLS, out DockingVertex dockedVertexLS)
        {
            return m_quadData.GetDockedLS(undockedTRLS, out dockedVertexLS);
        }

        protected override void DrawGizmos()
        {
            DockingGizmos.PushGizmosData();
            var color = GetGizmosColor();
            //var oldColor = Gizmos.color;
            Gizmos.color = color;

            DockingQuadData quadDataWS = GetQuadDataWS(m_quadData, transform);
                 
            DockingGizmos.DrawQuadPlane(quadDataWS, color);
            //DrawGizmosQuadAxis(quadDataWS.p1, quadDataWS.p2, quadDataWS.normal);

            DockingGizmos.DrawCoordinateFrameWS(quadDataWS.p1, transform.rotation);
            DockingGizmos.DrawCoordinateFrameWS(quadDataWS.p2, transform.rotation);

            //Gizmos.color = oldColor;
            DockingGizmos.PopGizmosData();
        }

        // 添加坐标轴，包括偏移，防止重叠覆盖
        private static void DrawGizmosQuadAxis(Vector3 p1, Vector3 p2, Vector3 up)
        {
            DockingGizmos.PushGizmosData();

            p1 += up * 0.05f;
            p2 += up * 0.05f;

            var length = 0.6f;

            Vector3 center = (p1 + p2) / 2.0f;
            Vector3 dir = (p2 - p1).normalized;

            Gizmos.color = Color.red;
            Gizmos.DrawLine(center - dir * length, center + dir * length);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(center, up.normalized * length + center);

            //Gizmos.color = Color.blue;
            //Gizmos.DrawLine(center, up * length + center);

            DockingGizmos.PopGizmosData();
        }

        protected static DockingQuadData GetQuadDataWS(DockingQuadData dataMS, Transform trs)
        {            
            DockingQuadData dataWS = new DockingQuadData();
            dataWS.p1 = trs.TransformPoint(dataMS.p1);
            dataWS.p2 = trs.TransformPoint(dataMS.p2);
            dataWS.normal = trs.TransformVector(dataMS.normal).normalized;

            Vector3 dir = dataMS.width * Vector3.Cross(dataMS.p2 - dataMS.p1, dataMS.normal).normalized;
            dataWS.width = trs.TransformVector(dir).magnitude;

            return dataWS;
        }
    }

}
