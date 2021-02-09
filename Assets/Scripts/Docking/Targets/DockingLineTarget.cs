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

        public override DockedVertexStatus GetDockedLS(TR undockedTRLS, out DockingVertex dockedVertexLS)
        {
            var validAlphaRange = GetMinMaxValidAlhpa(m_leftMargin, m_rightMargin);
            float alpha = GetDockedLS(undockedTRLS);
            float validAlpha = Mathf.Clamp(alpha, validAlphaRange.Item1, validAlphaRange.Item2);
            var dockedVertexSatus = GetDockedLS(validAlpha, out dockedVertexLS);

            // 有两个alpha range，其一是docked的有效区间，其二是输入限制的区间
            var limitAlpha = GetMinMaxValidAlhpa(m_leftMargin + 0.1f, m_rightMargin + 0.1f);
            dockedVertexSatus.limit = GetLimitByAlpha(alpha, limitAlpha);

            if(m_type == DockingTargetType.BEAM) // 如果是beam，则需要进行rotation修正
            {
                Vector3 forward = undockedTRLS.rotation * Vector3.forward;
                if(Mathf.Abs(forward.x) < Mathf.Abs(forward.z))
                {
                    forward = forward.z > 0 ? Vector3.forward : -Vector3.forward;
                }
                else
                {
                    forward = forward.x > 0 ? Vector3.right : -Vector3.right;
                }
                dockedVertexLS.tr.rotation = Quaternion.LookRotation(forward);                
            }
            return dockedVertexSatus;
        }
        
        protected override void DrawGizmos()
        {
            DockingGizmos.PushGizmosData();

            var color = GetGizmosColor();           
            Gizmos.color = color;

            var startTR = GetTRInWS(m_start.tr);
            var endTR = GetTRInWS(m_end.tr);
            var range = GetMinMaxValidAlhpa(m_leftMargin, m_rightMargin);
            var leftEdge = Vector3.Lerp(startTR.translation, endTR.translation, range.Item1);
            var rightEdge = Vector3.Lerp(startTR.translation, endTR.translation, range.Item2);

            // 绘制3条直线，分别为左边、中间有效区域，右边无效区域
            DockingGizmos.DrawLine(startTR.translation, leftEdge, m_lineWidth, Color.gray);
            DockingGizmos.DrawLine(leftEdge, rightEdge, m_lineWidth, color);
            DockingGizmos.DrawLine(rightEdge, endTR.translation, m_lineWidth, Color.gray);

            DockingGizmos.DrawCoordinateFrameWS(startTR);
            DockingGizmos.DrawCoordinateFrameWS(endTR);
           
            DockingGizmos.PopGizmosData();
        }


        public float GetDockedLS(TR undockedTRLS)
        {
            float k = 0;
            Vector3 dockedPoint;
            Utils.GetLineSegmentDockedPoint(m_start.tr.translation, m_end.tr.translation, undockedTRLS.translation,
                out dockedPoint, out k);            
            return k;
        }

        private DockedVertexStatus GetDockedLS(float alpha, out DockingVertex dockedVertexLS)
        {
            dockedVertexLS = DockingVertex.Lerp(m_start, m_end, alpha);

            var dockedVertexSatus = new DockedVertexStatus();
            dockedVertexSatus.alpha = alpha;
            dockedVertexSatus.reserveFloatParam = dockedVertexLS.reserveFloatParam;
            return dockedVertexSatus;
        }


        public static DockedVertexStatus GetDockedLS(TR undockedTRLS, DockingVertex start, DockingVertex end,
            out DockingVertex dockedVertexLS)
        {
            
            float k = 0;
            Vector3 dockedPoint;
            Utils.GetLineSegmentDockedPoint(start.tr.translation, end.tr.translation, undockedTRLS.translation,
                out dockedPoint, out k);           
            
            dockedVertexLS = new DockingVertex();
            dockedVertexLS.tr = TR.Lerp(start.tr, end.tr, k);
            dockedVertexLS.reserveFloatParam = Mathf.Lerp(start.reserveFloatParam,
                end.reserveFloatParam, k);

            var dockedVertexSatus = new DockedVertexStatus();
            dockedVertexSatus.alpha = k;
            dockedVertexSatus.reserveFloatParam = dockedVertexLS.reserveFloatParam;
            
            return dockedVertexSatus;
        }

        // 返回全局长度
        private float GetLengthWS()
        {
            var dir = m_end.tr.translation - m_start.tr.translation;
            return transform.TransformVector(dir).magnitude;
        }
        private System.Tuple<float, float> GetMinMaxValidAlhpa(float leftMargin, float rightMargin)
        {
            var len = GetLengthWS();
            return new System.Tuple<float, float>(leftMargin / len, 1.0f - rightMargin / len);
        }

        private DOCKED_POINT_MOVE_LIMIT GetLimitByAlpha(float alpha, System.Tuple<float, float> validAlpha)
        {
            if(alpha < validAlpha.Item1)
            {
                return DOCKED_POINT_MOVE_LIMIT.HORIZEN_LEFT_FORBIDEN;
            }
            if(alpha > validAlpha.Item2)
            {
                return DOCKED_POINT_MOVE_LIMIT.HORIZEN_RIGHT_FORBIDEN;
            }
            return DOCKED_POINT_MOVE_LIMIT.NONE;
        }

        protected override DockingVertex GetDockedWSImpl(float alpha)
        {
            if (alpha < 0 || alpha > 1) return null;
            DockingVertex vertex = null;
            GetDockedLS(alpha, out vertex);
            vertex.tr = GetTRInWS(vertex.tr);
            return vertex;
        }
    }

}
