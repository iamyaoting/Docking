using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Docking
{
    // Docking 和 vault 动作相关的特定target
    public class DockingQuadVaultTarget : DockingQuadTarget
    {
        public float m_landPointOffset;  // 着地点与quad边界线距离,最小的约束距离

        private void Start()
        {
            m_type = DockingTargetType.VAULT;
        }

        public override DockedVertexStatus GetDockedLS(TR undockedTRLS, out DockingVertex dockedVertexLS)
        {
            return base.GetDockedLS(undockedTRLS, out dockedVertexLS);
        }

        //// 计算m_landPointOffsetWS再ls内部的长度
        //private float GetLandOffsetLS()
        //{
        //    var dir = Vector3.Cross(m_quadData.normal, m_quadData.p2 - m_quadData.p1).normalized;
        //    dir = dir * m_landPointOffsetWS;
        //    return transform.InverseTransformVector(dir).magnitude;
        //}

        // 根据在ls中的point点计算ls的着地点
        private void GetTwoLandPointLS(Vector3 pointLS, out TR land1LS, out TR land2LS)
        {
            float landPointLS = m_landPointOffset;

            TR tr = new TR();
            tr.translation = pointLS;
            tr.rotation = Quaternion.identity;
            land1LS = new TR();
            land2LS = new TR();

            var originLandPosLS = pointLS;
            float alpha = 0;
            
            Docking.Utils.GetLineSegmentDockedPoint(m_quadData.p1, m_quadData.p2, pointLS, out pointLS, out alpha);
            
            var pointMid = Vector3.Lerp(m_quadData.p1, m_quadData.p2, alpha);     

            var dir = Vector3.Cross(m_quadData.normal, m_quadData.p2 - m_quadData.p1).normalized;

            // 计算动画着地点离quad的距离
            var dist = Vector3.Project(originLandPosLS - pointMid, dir).magnitude;
            dist = Mathf.Max(dist, m_quadData.width / 2 + landPointLS);

            land1LS.translation = pointMid + dist * dir;
            land2LS.translation = pointMid - dist * dir;
            land1LS.translation.y = 0;
            land2LS.translation.y = 0;

            dir = Vector3.ProjectOnPlane(dir, Vector3.up);
            land1LS.rotation = Quaternion.LookRotation(dir, Vector3.up);
            land2LS.rotation = Quaternion.LookRotation(-dir, Vector3.up);
        }

        // 根据当前点计算理想的着地点,不精确，没有射线探测
        public TR GetDesiredLandHintTRWS(Vector3 pointWS, Quaternion rotWS)
        {
            var pointLS = transform.InverseTransformPoint(pointWS);
            TR land1LS = null, land2LS = null;
            GetTwoLandPointLS(pointLS, out land1LS, out land2LS);
            TR landPointWS = null;
            var land1WS = GetTRInWS(land1LS);
            var land2WS = GetTRInWS(land2LS);
            if (Quaternion.Angle(rotWS, land1WS.rotation) > 90)
            {
                landPointWS = land2WS;
            }
            else
            {
                landPointWS = land1WS;
            }
            return landPointWS;
        }

        protected override void DrawGizmos()
        {
            base.DrawGizmos();

            Vector3 midP = (m_quadData.p1 + m_quadData.p2) / 2.0f;
            TR land1LS = null, land2LS = null;
            GetTwoLandPointLS(midP, out land1LS, out land2LS);
            var land1WS = GetTRInWS(land1LS);
            var land2WS = GetTRInWS(land2LS);

            DockingGizmos.DrawCoordinateFrameWS(land1WS);
            DockingGizmos.DrawCoordinateFrameWS(land2WS);
        }
    }

}
