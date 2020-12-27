using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Rendering;

namespace Docking
{
    public class DockingDetector : MonoBehaviour
    {
        public Vector3 m_biasMS = new Vector3(0.0f, 1.0f, 0.2f);     // 搜索圆锥的顶点与hostplayer节点的偏移

        [Range(10, 80)]
        public float m_fov = 40;
        [Range(3, 6)]
        public float m_maxDist = 5.0f;
        [Range(0.2f, 2)]
        public float m_minDist = 1.0f;
        [Range(-40, 40)]
        public float m_elevationAngleMS = 20;  // 俯仰角  

        public TR GetPlayerTRWS()
        {
            TR tr = new TR();
            tr.translation = transform.position;
            tr.rotation = transform.rotation;
            return tr;
        }
        public bool DetectNearestTarget(out DockingTarget target, 
            out DockingVertex desiredVertex,
            out DockedVertexStatus desiredVertexStatus)
        {
            bool isFound = false;
            desiredVertex = new DockingVertex();
            desiredVertexStatus = null;
            target = null;
            DockedVertexStatus statusTmp = null;
            DockingVertex tmpVertex = null;

            float nearestDist = float.MaxValue;
            var colliders = Physics.OverlapSphere(transform.TransformPoint(m_biasMS), m_maxDist);
            foreach (var c in colliders)
            {
                var targets = c.GetComponentsInChildren<DockingTarget>();
                foreach(var t in targets)
                {                  
                    float dist = float.MaxValue;                                        
                    if(t.IsInDetectorSweepVolume(this, out dist, out tmpVertex, out statusTmp))
                    {
                        if (dist < nearestDist)
                        {
                            nearestDist = dist;
                            target = t;
                            desiredVertexStatus = statusTmp;
                            m_desiredDockedVertex = tmpVertex; // 保存debug信息
                            desiredVertex = tmpVertex;
                            isFound = true;
                        }
                    }                    
                }
            }
            return isFound;
        }

        // 获得圆锥的中心朝向向量
        private Vector3 GetDirectionMS()
        {
            var directionMS = Quaternion.AngleAxis(90 - m_elevationAngleMS, Vector3.right) * Vector3.up;
            return directionMS.normalized;
        }       

        public bool IsPointInDetectorWS(Vector3 pointWS)
        {
            var pointMS = transform.InverseTransformPoint(pointWS);
            var point_center_dir = pointMS - m_biasMS;
            float dist = point_center_dir.magnitude;
            if (dist > m_minDist && dist < m_maxDist
                //&& Vector3.Angle(point_center_dir, GetDirectionMS()) < m_fov
                )
            {
                return true;
            }
            return false;
        }      

        #region gizmos
        private int gizmosAngle = 0;
        private const float gizmosTwistSpeed = 150;
        private DockingVertex m_desiredDockedVertex = null;
               
        public void DrawGizmos()
        {
            gizmosAngle = (gizmosAngle + (int)(Time.deltaTime * gizmosTwistSpeed)) % 360;
            DockingGizmos.PushGizmosData();

            Matrix4x4 mat44 = new Matrix4x4();
            mat44.SetTRS(transform.position, transform.rotation, Vector3.one);
            Gizmos.matrix = mat44;

            var dirMS = GetDirectionMS();
            Vector3 destP = dirMS * m_maxDist + m_biasMS;
            Vector3 n = dirMS;

            // 计算空间一个圆
            float r = m_maxDist * Mathf.Tan(m_fov * Mathf.Deg2Rad);
            Vector3 u = new Vector3(n.y, -n.x, 0);
            Vector3 v = new Vector3(n.x * n.z, n.y * n.z, -n.x * n.x - n.y * n.y);

            Func<float, Vector3> CalCirclePoint = (float angle) =>
            {
                return destP + r * (u.normalized * Mathf.Cos(Mathf.Deg2Rad * angle)
                + v.normalized * Mathf.Sin(Mathf.Deg2Rad * angle));
            };

            var c1 = CalCirclePoint(gizmosAngle);
            var c2 = CalCirclePoint(gizmosAngle + 180);
            float width = 0.03f;
            Color color = Color.green;
            DockingGizmos.DrawLine(m_biasMS, c1, width, color);
            DockingGizmos.DrawLine(m_biasMS, c2, width, color);
            DockingGizmos.DrawLine(m_biasMS, (c1 + c2) / 2, width, color);
            DockingGizmos.DrawLine(c2, c1, width, color);             

            DockingGizmos.PopGizmosData();

            // 绘制detector寻找到的最近点
            if(null != m_desiredDockedVertex)
            {
                var oldColr = Gizmos.color;
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(m_desiredDockedVertex.tr.translation, 0.5f);
                Gizmos.color = oldColr;
            }            
        }
        #endregion
    }

}
