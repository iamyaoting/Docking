using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectContactPoints : MonoBehaviour
{
    private List<Vector3> points = new List<Vector3>();

    public List<Vector3> GetPoints()
    {
        return points;
    }

    private void OnCollisionStay(Collision collision)
    {
        points.Clear();
        foreach(var c in collision.contacts)
        {
            points.Add(c.point);
        }
    }
}
