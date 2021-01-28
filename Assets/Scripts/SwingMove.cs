using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingMove : MonoBehaviour
{
    public float radius = 5;
    public float angle = 20;
    public float cycleTime = 5;
    public Vector3 normalAxis = Vector3.forward;

    private float phase;
    private Vector3 startPos;

    // Start is called before the first frame update
    void Start()
    {
        phase = 0;
        startPos = transform.localPosition;
        normalAxis.Normalize();
    }

    // Update is called once per frame
    void Update()
    {
        phase += Time.deltaTime / cycleTime;
        if (phase > 1) phase -= 1;
        float curAngle = Mathf.Sin(phase * Mathf.PI * 2) * angle * Mathf.Deg2Rad;


        float heightOffset = radius * (1 - Mathf.Cos(curAngle));
        float horizentalOffset = radius * Mathf.Sin(curAngle);

        Vector3 horizentalAxis = Vector3.Cross(Vector3.up, normalAxis);

        transform.localPosition = startPos + horizentalAxis * horizentalOffset + heightOffset * Vector3.up;
    }
}
