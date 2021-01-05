using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TEST : MonoBehaviour
{
    public Mesh m;
    private void Update()
    {

    }

    private void OnDrawGizmos()
    {
        var z = m.uv2;

        //         foreach(var p in z)
        //         {
        //             Gizmos.DrawSphere(new Vector3(p.x, 0, p.y), 0.004f);
        //         }
        var pp = m.triangles.Length;
        for (int j = 0; j < pp; j += 3)
        {
            int a = m.triangles[j];
            int b = m.triangles[j + 1];
            int c = m.triangles[j + 2];
            Gizmos.DrawLine(new Vector3(z[a].x, 0, z[a].y), new Vector3(z[b].x, 0, z[b].y));
            Gizmos.DrawLine(new Vector3(z[a].x, 0, z[a].y), new Vector3(z[c].x, 0, z[c].y));
            Gizmos.DrawLine(new Vector3(z[c].x, 0, z[c].y), new Vector3(z[b].x, 0, z[b].y));
        }

    }
}
