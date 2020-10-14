using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Rendering;

namespace Docking
{
    public class DockingDetector : MonoBehaviour
    {
        [Header("Init information")]
        public Transform hostPlayer;
        
        public Vector3 biasMS = new Vector3(0.2f, 1.0f, 0);     // 搜索圆锥的顶点与hostplayer节点的偏移

        [Range(10, 80)]
        public float fov = 40;
        [Range(3, 6)]
        public float maxDist = 5.0f;
        [Range(0.2f, 2)]
        public float minDist = 1.0f;
        [Range(-40, 40)]
        public float elevationAngleMS = 20;  // 俯仰角        

        // 获得圆锥的中心朝向向量
        private Vector3 GetDirectionMS()
        {
            var directionMS = Quaternion.AngleAxis(90 - elevationAngleMS, Vector3.right) * Vector3.up;
            return directionMS.normalized;
        }

        //private List<Vector3> DetectDockingPoints()
        //{
        //    List<Vector3> ret = new List<Vector3>();
        //    var colliders = Physics.OverlapSphere(coneTrans.position, maxDist);
        //    foreach (var c in colliders)
        //    {
        //        ret.Add(c.bounds.center);
        //    }
        //    return ret;
        //}

        #region gizmos

        // gizmos variables
        private int gizmosAngle = 0;
        private const float gizmosTwistSpeed = 150;

        private void OnDrawGizmos()
        {
            UpdateDetecterGizmos();
        }

        private void OnGUI()
        {
            GUI.color = Color.red;
            GUI.Label(new Rect(25, 25, 100, 30), gizmosAngle.ToString());
        }

        private void UpdateDetecterGizmos()
        {
            gizmosAngle = (gizmosAngle + (int)(Time.deltaTime * gizmosTwistSpeed)) % 360;
            DockingGizmos.PushGizmosData();

            Matrix4x4 mat44 = new Matrix4x4();
            mat44.SetTRS(hostPlayer.position, hostPlayer.rotation, Vector3.one);
            Gizmos.matrix = mat44;

            var dirMS = GetDirectionMS();
            Vector3 destP = dirMS * maxDist + biasMS;
            Vector3 n = dirMS;

            // 计算空间一个圆
            float r = maxDist * Mathf.Tan(fov * Mathf.Deg2Rad);
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
            DockingGizmos.DrawLine(biasMS, c1, width, color);
            DockingGizmos.DrawLine(biasMS, c2, width, color);
            DockingGizmos.DrawLine(biasMS, (c1 + c2) / 2, width, color);
            DockingGizmos.DrawLine(c2, c1, width, color);         

            DockingGizmos.PopGizmosData();
        }
        #endregion
    }

}
