using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Docking
{
    public class DockingLineStripTarget : DockingTarget
    {
        public DockingVertex[] m_vertices;          // 顶点集合
        public bool m_loop;                         // 是否循环
        public bool m_handIK = false;               // 是否需要开启Hand IK

        //public float y = 2;
        //public float radius = 5.2f;
        //public int n = 20;
        //public float angleOffset = 30.0f;

        //private void OnEnable()
        //{
        //    System.Action<DockingVertex, float, float> calc = (DockingVertex vertex, float r, float angle) =>
        //    {
        //        vertex.tr = new TR();
        //        vertex.tr.translation = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
        //        vertex.tr.rotation = Quaternion.LookRotation(-vertex.tr.translation.normalized, Vector3.up);
        //        vertex.tr.translation.y = y;
        //        vertex.reserveFloatParam = 1.0f;
        //        return;
        //    };

        //    m_vertices = new DockingVertex[n];
        //    for (int i = 0; i < n; ++i)
        //    {
        //        m_vertices[i] = new DockingVertex();
        //        float angle = 2 * Mathf.PI / n * i + angleOffset * Mathf.Deg2Rad;
        //        m_vertices[i] = new DockingVertex();
        //        calc(m_vertices[i], radius, angle);
        //    }
        //}

        //private void OnEnable()
        //{
        //    int n = m_vertices.Length;
        //    for (int i = 0; i < n; ++i)
        //    {
        //        var p = m_vertices[i].tr.translation;
        //        m_vertices[i].tr.rotation = Quaternion.LookRotation(new Vector3(-p.x, 0, -p.z));
        //    }
        //}

        public override DockedVertexStatus GetDockedLS(TR undockedTRLS, out DockingVertex dockedVertexLS)
        {
            DockedVertexStatus dockedVertexSatus = null;
            DockingVertex dockedVertexTMP = null;
            DockingVertex startV, endV;

            int idx = 0;
            dockedVertexLS = null;
            var minValue = float.MaxValue;
            var count = m_vertices.Length;
            for (int i = 0; i < count; ++i)
            {
                if (!GetLineSegment(i, out startV, out endV)) break;
                var tmpR = DockingLineTarget.GetDockedLS(undockedTRLS, startV, endV, out dockedVertexTMP);
                if (null == dockedVertexTMP) continue;
                var value = (dockedVertexTMP.tr.translation - undockedTRLS.translation).magnitude;
                if (value < minValue)
                {
                    idx = i;
                    dockedVertexSatus = tmpR;
                    minValue = value;
                    dockedVertexLS = dockedVertexTMP;
                }
            }
            dockedVertexSatus.limit = GetLimitByAlpha(dockedVertexSatus.alpha, dockedVertexLS.tr, idx);
            return dockedVertexSatus;
        }

        private DOCKED_POINT_MOVE_LIMIT GetLimitByAlpha(float alpha, TR tr, int idx)
        {
            if (m_loop) return DOCKED_POINT_MOVE_LIMIT.NONE;
            
            var count = m_vertices.Length;
            if (idx < count - 2 && idx > 0 && count > 1) return DOCKED_POINT_MOVE_LIMIT.NONE;

            if (alpha < 0.5f && 0 == idx)
            {
                var dist = transform.TransformVector(tr.translation - m_vertices[0].tr.translation).magnitude;
                if (dist < marginDist)
                {                    
                    return DOCKED_POINT_MOVE_LIMIT.HORIZEN_LEFT_FORBIDEN;
                }
            }
            if (alpha >= 0.5f && idx == count - 2)
            {
                var dist = transform.TransformVector(tr.translation - m_vertices[count - 1].tr.translation).magnitude;
                if (dist < marginDist)
                {                 
                    return DOCKED_POINT_MOVE_LIMIT.HORIZEN_RIGHT_FORBIDEN;
                }
            }
            return DOCKED_POINT_MOVE_LIMIT.NONE;
        }

        private bool GetLineSegment(int idx, out DockingVertex start, out DockingVertex end)
        {            
            var count = m_vertices.Length;
            if(idx < count - 1)
            {
                start = m_vertices[idx];
                end = m_vertices[idx + 1];
                return true;
            }
            // 考虑是否是起始点和终结点连在一起
            if (idx == count - 1 && m_loop)
            {
                start = m_vertices[idx];
                end = m_vertices[0];
                return true;
            }
            start = null;
            end = null;
            return false;
        }

        protected override void DrawGizmos()
        {
            DockingGizmos.PushGizmosData();

            var color = GetGizmosColor();
            Gizmos.color = color;

            DockingVertex startV, endV;

            if (null != m_vertices)
            {
                for (int i = 0; i < m_vertices.Length; ++i)
                {
                    var startTR = GetTRInWS(m_vertices[i].tr);
                    DockingGizmos.DrawCoordinateFrameWS(startTR.translation + Vector3.up * 0.01f, startTR.rotation);
                    if (!GetLineSegment(i, out startV, out endV)) break;
                    startTR = GetTRInWS(startV.tr);
                    var endTR = GetTRInWS(endV.tr);
                    DockingGizmos.DrawLine(startTR.translation, endTR.translation, m_lineWidth, color);
                }
            }

            DockingGizmos.PopGizmosData();
        }
    }
}

