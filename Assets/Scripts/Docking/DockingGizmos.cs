using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Docking
{
    public class DockingGizmos
    {
        public static void DrawCoordinateFrame(Vector3 pos, Quaternion rot, float length)
        {
            pos += rot * Vector3.forward * 0.02f;

            var oldColor = Gizmos.color;

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

