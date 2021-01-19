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
            var validAlphaRange = GetMinMaxValidAlhpa();
            float alpha = GetDockedLS(undockedTRLS);
            float validAlpha = Mathf.Clamp(alpha, validAlphaRange.Item1, validAlphaRange.Item2);
            var dockedVertexSatus = GetDockedLS(validAlpha, out dockedVertexLS);
            dockedVertexSatus.limit = GetLimitByAlpha(alpha, validAlphaRange);

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

            DockingGizmos.DrawLine(startTR.translation, endTR.translation, m_lineWidth, color);

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

        public DockedVertexStatus GetDockedLS(float alpha, out DockingVertex dockedVertexLS)
        {
            dockedVertexLS = new DockingVertex();
            dockedVertexLS.tr = TR.Lerp(m_start.tr, m_end.tr, alpha);
            dockedVertexLS.reserveFloatParam = Mathf.Lerp(m_start.reserveFloatParam,
                m_end.reserveFloatParam, alpha);

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
            
            //// 判断原始的undocked点是否在直线的背面
            //var dir = undockedTRLS.translation - dockedVertexLS.tr.translation;
            //if(dir.magnitude > 0.1 && Vector3.Angle(dockedVertexLS.tr.rotation * Vector3.forward, dir) > 100)
            //{
            //    //Debug.Log(Vector3.Angle(dockedVertexLS.tr.rotation * Vector3.forward, dir) + "..." + k);
            //    // 在背面，返回null
            //    //Debug.LogWarning("At the back of the line!");
            //    dockedVertexLS = null;
            //}

            return dockedVertexSatus;
        }

        // 返回全局长度
        private float GetLengthWS()
        {
            var dir = m_end.tr.translation - m_start.tr.translation;
            return transform.TransformVector(dir).magnitude;
        }
        private System.Tuple<float, float> GetMinMaxValidAlhpa()
        {
            var len = GetLengthWS();
            return new System.Tuple<float, float>(marginDist / len, 1.0f - marginDist / len);
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
    }

}
