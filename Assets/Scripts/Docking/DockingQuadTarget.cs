using System.Collections;
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
    }
    public class DockingQuadTarget : DockingTarget
    {
        public DockingQuadData m_quadData;

        public override TR GetDcokedVertex(Transform unDockedTrans)
        {
            return new TR();
        }

        protected override void DrawGizmos()
        {            
            var color = GetGizmosColor();
            var oldColor = Gizmos.color;
            Gizmos.color = color;

            DockingQuadData quadDataWS = GetQuadDataWS(m_quadData, transform);
                 
            DockingGizmos.DrawQuadPlane(quadDataWS, color);
            DrawGizmosQuadAxis(quadDataWS.p1, quadDataWS.p2,
               quadDataWS.normal, m_coordinateFrameAsixLength);

            Gizmos.color = oldColor;
        }

        // 添加坐标轴，包括偏移，防止重叠覆盖
        private static void DrawGizmosQuadAxis(Vector3 p1, Vector3 p2, Vector3 up, float length)
        {
            DockingGizmos.PushGizmosData();

            p1 += up * 0.05f;
            p2 += up * 0.05f;

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

        private static DockingQuadData GetQuadDataWS(DockingQuadData dataMS, Transform trs)
        {            
            DockingQuadData dataWS = new DockingQuadData();
            dataWS.p1 = trs.TransformPoint(dataMS.p1);
            dataWS.p2 = trs.TransformPoint(dataMS.p2);
            dataWS.normal = trs.TransformVector(dataMS.normal).normalized;

            Vector3 dir = dataMS.width * Vector3.Cross(dataMS.p2 - dataMS.p1, dataMS.normal);
            dataWS.width = trs.TransformVector(dir).magnitude;

            return dataWS;
        }
    }

}
