using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Docking
{
    public class GizmosData
    {
        public Color color;
        public Matrix4x4 mat44;
    }
    public class DockingGizmos
    {
        private static Stack<GizmosData> gizmosStack = new Stack<GizmosData>();

        public static void DrawTR(TR tr, Color color,
            float pointSize,
            float coordinateFrameAxisLength)
        {
            //PushGizmosData();
            Gizmos.color = color;
            Gizmos.DrawSphere(tr.translation, pointSize);
            DrawCoordinateFrame_Impl(tr.translation, tr.rotation, coordinateFrameAxisLength);
            //PopGizmosData();
        }

        public static void DrawCoordinateFrameWS(Vector3 pos,
            Quaternion rot, float coordinateFrameAxisLength = 0.6f)
        {
            DrawCoordinateFrameWS(pos, rot, Color.red, 0.0f, coordinateFrameAxisLength);
        }

        public static void DrawCoordinateFrameWS(TR tr, float coordinateFrameAxisLength = 0.6f)
        {
            DrawCoordinateFrameWS(tr.translation, tr.rotation, Color.red, 0.0f, coordinateFrameAxisLength);
        }
        public static void DrawCoordinateFrameWS(DockingTransform tr, float coordinateFrameAxisLength = 0.6f)
        {
            DrawCoordinateFrameWS(tr.translation, tr.rotation, Color.red, 0.0f, coordinateFrameAxisLength);
        }

        public static void DrawCoordinateFrameWS(Vector3 pos, 
            Quaternion rot, Color originColor,
            float originSize = 0.0f,
            float coordinateFrameAxisLength = 0.6f)
        {
            //PushGizmosData();            
            DrawCoordinateFrame_Impl(pos, rot, coordinateFrameAxisLength);
            if (originSize > 0)
            {
                Gizmos.color = originColor;
                Gizmos.DrawSphere(pos, originSize);
            }
            //PopGizmosData();
        }
                
        public static void DrawLine(Vector3 p1, Vector3 p2, float width, Color color)
        {
            //PushGizmosData();
            Matrix4x4 oldM = Gizmos.matrix;

            Gizmos.color = color;
            Matrix4x4 m = new Matrix4x4();
            Quaternion q = Quaternion.FromToRotation(Vector3.up, p2 - p1);
            m.SetTRS((p1 + p2) / 2.0f, q, new Vector3(width, (p2 - p1).magnitude, width));            
            Gizmos.matrix = Gizmos.matrix * m;
            Gizmos.DrawCube(Vector3.zero, Vector3.one);
            
            Gizmos.matrix = oldM;
            //PopGizmosData();
        }

        // 绘制xz平面的圆盘
        public static void DrawDisc(Vector3 center, float radius)
        {
            var oldMat = Gizmos.matrix;
            var m = new Matrix4x4();
            m.SetTRS(Vector3.zero, Quaternion.identity, new Vector3(1, 0, 1));
            Gizmos.matrix = m;

            Gizmos.DrawWireSphere(center, radius);

            Gizmos.matrix = oldMat;
        }

        public static void DrawLines(List<System.Tuple<Vector3, Vector3>> segments, float width, Color color)
        {
            foreach(var seg in segments)
            {
                DrawLine(seg.Item1, seg.Item2, width, color);
            }            
        }

        public static void DrawQuadPlane(DockingQuadData data, Color color)
        {
            //PushGizmosData();
            Matrix4x4 oldM = Gizmos.matrix;

            Gizmos.color = color;
            Matrix4x4 mat44 = new Matrix4x4();
            Quaternion rot = Quaternion.LookRotation(Vector3.Cross(data.p2 - data.p1, data.normal), data.normal);
            mat44.SetTRS((data.p1 + data.p2) / 2.0f, rot,
                new Vector3((data.p2 - data.p1).magnitude, 0.01f, data.width)
                );            
            Gizmos.matrix = mat44;
            Gizmos.DrawCube(Vector3.zero, Vector3.one);
            
            
            Gizmos.matrix = oldM;
            //PopGizmosData();
        }       

        public static void PushGizmosData()
        {
            GizmosData data = new GizmosData();
            data.color = Gizmos.color;
            data.mat44 = Gizmos.matrix;
            gizmosStack.Push(data);
            return;
        }

        public static void PopGizmosData()
        {
            var data = gizmosStack.Pop();
            Gizmos.color = data.color;
            Gizmos.matrix = data.mat44;
        }

        private static void DrawCoordinateFrame_Impl(Vector3 pos, Quaternion rot, float length)
        {
            Color oldColor = Gizmos.color;

            Gizmos.color = Color.red;
            Gizmos.DrawLine(pos, rot * -Vector3.left * length + pos);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(pos, rot * Vector3.up * length + pos);

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(pos, rot * Vector3.forward * length + pos);

            Gizmos.color = oldColor;
        }
    }
}

