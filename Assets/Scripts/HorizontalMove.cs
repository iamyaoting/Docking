using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorizontalMove : MonoBehaviour
{
    public float dist = 2;
    public float cycleTime = 10;
    public Vector3 axis;

    private float phase;
    private Vector3 startPos;

    // Start is called before the first frame update
    void Start()
    {
        axis.Normalize();
        phase = 0;
        startPos = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        phase += Time.deltaTime / cycleTime;
        if (phase > 1) phase -= 1;

        transform.localPosition = Mathf.Sin(phase * Mathf.PI * 2) * dist * axis + startPos;
    }
}
