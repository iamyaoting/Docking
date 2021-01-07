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
            var dockedVertexSatus = GetDockedLS(undockedTRLS, m_start, m_end, out dockedVertexLS);
            dockedVertexSatus.limit = GetLimitByAlpha(dockedVertexSatus.alpha, dockedVertexLS.tr);
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

        private DOCKED_POINT_MOVE_LIMIT GetLimitByAlpha(float alpha, TR tr)
        {
            if (alpha < 0.5f)
            {
                var dist = transform.TransformVector(tr.translation - m_start.tr.translation).magnitude;
                if (dist < SMALL_DISTANCE)
                {
                    return DOCKED_POINT_MOVE_LIMIT.HORIZEN_LEFT_FORBIDEN;
                }
            }
            if (alpha >= 0.5f)
            {
                var dist = transform.TransformVector(tr.translation - m_end.tr.translation).magnitude;
                if (dist < SMALL_DISTANCE)
                {
                    return DOCKED_POINT_MOVE_LIMIT.HORIZEN_RIGHT_FORBIDEN;
                }               
            }
            return DOCKED_POINT_MOVE_LIMIT.NONE;
        }
    }

}
