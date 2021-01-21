using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Rendering;

namespace Docking
{
    [System.Flags]
    public enum DetectorType
    {
        None = 0x00,
        LowDetector = 0x1,
        HighDetector = 0x2,
        HangDetector = 0x4,
        HangBackDetector = 0x8
    }

    public class DockingDetector : MonoBehaviour
    {
        public Vector3 m_biasMS = new Vector3(0.0f, 1.0f, 0.2f);     // 搜索圆锥的顶点与hostplayer节点的偏移

        [Range(10, 80)]
        public float m_fov = 40;
        [Range(3, 6)]
        public float m_maxDist = 5.0f;
        [Range(0.2f, 2)]
        public float m_minDist = 1.0f;
        
        public float m_phi = 20;  // 纬度
        public float m_lamda = 0; // 经度

        public ControllerEnterContext GetNearestDockingTarget(DetectorType type, Vector2 dir, DockingTarget target)
        {            
            ControllerEnterContext context = null;
            if ((type & DetectorType.LowDetector) != DetectorType.None)
            {
                context = GetNearestDockingTarget_Locomotion_Low(target);
            }
            if ((type & DetectorType.HighDetector) != DetectorType.None)
            {
                context = GetNearestDockingTarget_Locomotion_High(target);
            }
            if ((type & DetectorType.HangDetector) != DetectorType.None)
            {
                context = GetNearestDockingTarget_Hanging(dir, target);
            }
            if ((type & DetectorType.HangBackDetector) != DetectorType.None)
            {
                context = GetNearestDockingTarget_Hanging(new Vector2(0, 1), null);
            }
            return context;
        }


        // 利用docking detector 寻找最近的target, Locomotion Detector
        public ControllerEnterContext GetNearestDockingTarget_Locomotion_High(DockingTarget target)
        {
            SetLocomoitionHighDetector();
            ControllerEnterContext context = new ControllerEnterContext();
            if (false == DetectNearestTarget(target,
                out context.dockingtarget, out context.desiredDockedVertex, out context.desiredDockedVertexStatus))
            {
                Debug.Log("Can not find docking target, dock forbidden!");
                return null;
            }
            else
            {
                Debug.Log("Find docking target: " + context.dockingtarget.name);
                return context;
            }
        }

        public ControllerEnterContext GetNearestDockingTarget_Locomotion_Low(DockingTarget target)
        {
            SetLocomoitionLowDetector();
            ControllerEnterContext context = new ControllerEnterContext();
            if (false == DetectNearestTarget(target,
                out context.dockingtarget, out context.desiredDockedVertex, out context.desiredDockedVertexStatus))
            {
                Debug.Log("Can not find docking target, dock forbidden!");
                return null;
            }
            else
            {
                Debug.Log("Find docking target: " + context.dockingtarget.name);
                return context;
            }
        }

        // 利用docking detector 寻找最近的target, Hanging Detector
        public ControllerEnterContext GetNearestDockingTarget_Hanging(Vector2 moveDir, DockingTarget currentTarget)
        {
            var moveDirMS = Quaternion.FromToRotation(Vector3.up, transform.up) * moveDir;
            SetHangingDetector(moveDirMS);
            ControllerEnterContext context = new ControllerEnterContext();
            if (false == DetectNearestTarget(currentTarget,
                out context.dockingtarget, out context.desiredDockedVertex, out context.desiredDockedVertexStatus))
            {
                Debug.Log("Can not find docking target, dock forbidden!");
                return null;
            }
            else
            {
                Debug.Log("Find docking target: " + context.dockingtarget.name);
                return context;
            }
        }

        // 设置Detector为Locomotion类型
        private void SetLocomoitionHighDetector()
        {
            m_biasMS = new Vector3(0.0f, 1.0f, 0.2f);
            m_fov = 40;
            m_maxDist = 5.0f;
            m_minDist = 1.0f;
            m_phi = 20;
            m_lamda = 0;
        }
        private void SetLocomoitionLowDetector()
        {
            m_biasMS = new Vector3(0.0f, 0.0f, 0.2f);
            m_fov = 40;
            m_maxDist = 5.0f;
            m_minDist = 1.0f;
            m_phi = 20;
            m_lamda = 0;
        }


        // 设置Detector为Hang的2D模式，在立面进行搜索
        private void SetHangingDetector(Vector2 moveDir)
        {
            m_biasMS = transform.InverseTransformPoint(Utils.GetDockingBoneTransform(GetComponent<Animator>()).position);             
            m_fov = 20;
            m_maxDist = 7.0f;
            m_minDist = 0.01f;
                        
            if(moveDir.magnitude == 0)
            {
                Debug.LogError("Detector Hang dir Error!");
            }
            moveDir.Normalize();

            if(0 == moveDir.x)
            {
                m_lamda = 0;
                if (moveDir.y > 0) m_phi = 90;
                else m_phi = -90;               
            }
            else if(moveDir.x > 0)
            {
                m_phi = Vector2.SignedAngle(Vector2.right, moveDir);
                m_lamda = 90;
            }
            else
            {
                m_phi = Vector2.SignedAngle(moveDir, -Vector2.right);
                m_lamda = -90;
            }
        }

        // 设置Detector为背面搜索模式，主要用于往背面跳跃
        private void SetHangingBackDetector()
        {
            m_biasMS = new Vector3(0.0f, 1.0f, -0.2f);
            m_fov = 40;
            m_maxDist = 5.0f;
            m_minDist = 1.0f;
            m_phi = 0;
            m_lamda = 180;
        }        

        private bool DetectNearestTarget(DockingTarget currentTarget,
            out DockingTarget target, 
            out DockingVertex desiredVertex,
            out DockedVertexStatus desiredVertexStatus)
        {
            bool isFound = false;
            desiredVertex = new DockingVertex();
            desiredVertexStatus = null;
            target = null;
            DockedVertexStatus statusTmp = null;            
            TR dockedTR = null;

            float nearestDist = float.MaxValue;
            var colliders = Physics.OverlapSphere(transform.TransformPoint(m_biasMS), m_maxDist);
            foreach (var c in colliders)
            {
                var targets = c.GetComponentsInChildren<DockingTarget>();
                foreach(var t in targets)
                {
                    if (!t.m_active || !t.enabled) continue;

                    // 如果是当前的target，忽略它
                    if (t == currentTarget) continue;  

                    // 在世界空间获得detector的docked点
                    float dist = float.MaxValue;
                    t.GetDockedPointWS(transform.position, transform.rotation, out dockedTR, out statusTmp);
                    if (IsPointInDetectorWS(t, dockedTR, out dist))
                    {
                        if (dist < nearestDist)
                        {
                            nearestDist = dist;
                            target = t;
                            desiredVertexStatus = statusTmp;                            
                            desiredVertex = new DockingVertex(dockedTR.translation, dockedTR.rotation, statusTmp.reserveFloatParam);
                            m_desiredDockedVertex = new DockingVertex(dockedTR.translation, dockedTR.rotation, statusTmp.reserveFloatParam); // 保存debug信息
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
            Vector3 pos = new Vector3(
                Mathf.Cos(m_phi * Mathf.Deg2Rad) * Mathf.Sin(m_lamda * Mathf.Deg2Rad), 
                Mathf.Sin(m_phi * Mathf.Deg2Rad),
                Mathf.Cos(m_phi * Mathf.Deg2Rad) * Mathf.Cos(m_lamda * Mathf.Deg2Rad)
                );
            return pos;
        }       

        public bool IsPointInDetectorWS(DockingTarget target, TR dockedPointTRWS, out float dist)
        {
            var pointMS = transform.InverseTransformPoint(dockedPointTRWS.translation);            
            var point_center_dir = pointMS - m_biasMS;
            var dir = GetDirectionMS();
            dist = point_center_dir.magnitude;
            var quatDiff = Quaternion.Angle(dockedPointTRWS.rotation, transform.rotation);


            // docked 点距离探测圆锥中心之间的角度
            float fovDiff = Vector3.Angle(point_center_dir, dir);

            if (dist > m_minDist && dist < m_maxDist)
            {                  
                if (fovDiff < m_fov && quatDiff < 90)
                {
                    return true;
                }             
            }
            Debug.Log("Detect Docking Target:" + target.name + "type: " + target.m_type + ";  dist=" + dist +
                ",  angle=" + fovDiff + "  orientation Diff =" + quatDiff);
            return false;
        }


        #region gizmos
        private int gizmosAngle = 0;
        private const float gizmosTwistSpeed = 150;
        private DockingVertex m_desiredDockedVertex = null;

        private void OnDrawGizmos()
        {
            DrawGizmos();
        }
        public void DrawGizmos()
        {
            gizmosAngle = (gizmosAngle + (int)(Time.deltaTime * gizmosTwistSpeed)) % 360;
            DockingGizmos.PushGizmosData();

            Matrix4x4 mat44 = new Matrix4x4();
            mat44.SetTRS(transform.position, transform.rotation, Vector3.one);
            Gizmos.matrix = mat44;

            var dirMS = GetDirectionMS();

            float width = 0.03f;
            Color color = Color.green;

            // 计算圆锥末端圆上两个相互垂直的向量
            var tmp = dirMS + Vector3.one;
            if (Vector3.Angle(tmp, dirMS) == 0) tmp += Vector3.up;
            tmp.Normalize();
            Vector3 u = Vector3.Cross(tmp, dirMS);
            Vector3 v = Vector3.Cross(dirMS, u);

            // 给定一个距离和角度，获得圆锥上圆的点
            Func<float, float, Vector3> CalCirclePoint = (float dist, float angle) =>
            {
                Vector3 destP = dirMS * dist + m_biasMS;                              
                float r = dist * Mathf.Tan(m_fov * Mathf.Deg2Rad);                
                return destP + r * (u * Mathf.Cos(Mathf.Deg2Rad * angle)
                + v * Mathf.Sin(Mathf.Deg2Rad * angle));
            };

            var c1 = CalCirclePoint(m_maxDist, gizmosAngle);
            var c2 = CalCirclePoint(m_maxDist, gizmosAngle + 180);

            var d1 = CalCirclePoint(m_minDist, gizmosAngle);
            var d2 = CalCirclePoint(m_minDist, gizmosAngle + 180);
            var dcenter = dirMS * m_minDist + m_biasMS;

            DockingGizmos.DrawLine(m_biasMS, c1, width, color);
            DockingGizmos.DrawLine(m_biasMS, c2, width, color);
            DockingGizmos.DrawLine(dcenter, (c1 + c2) / 2, width, color);
            DockingGizmos.DrawLine(dcenter, d1, width, color);
            DockingGizmos.DrawLine(dcenter, d2, width, color);
            DockingGizmos.DrawLine(c2, c1, width, color);            

            DockingGizmos.PopGizmosData();            
            
            // 绘制detector寻找到的最近点，候选点
            if(null != m_desiredDockedVertex)
            {
                var oldColr = Gizmos.color;
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(m_desiredDockedVertex.tr.translation, 0.2f);
                //Debug.Log(m_desiredDockedVertex.tr.translation);
                Gizmos.color = oldColr;
            }            
        }
        #endregion
    }

}
