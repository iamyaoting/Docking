using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateWindMill : MonoBehaviour
{
    // angular velocity deg/s
    public float angularVelocity = 10;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.localRotation *= Quaternion.AngleAxis(angularVelocity * Time.deltaTime, Vector3.forward);
    }
}
