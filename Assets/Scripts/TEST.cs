using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TEST : MonoBehaviour
{
    public Transform t1, t2;
    public AnimationCurve curve;
    public float time = 2.0f;
    public float lastBlend = 0.0f;
    public float curBlend = 0.0f;

    private void Start()
    {
        
    }

    private void Update()
    {
        curBlend = lastBlend + Time.deltaTime / time;

        var fraction = Docking.Utils.ComputeBlendFraction(
            Docking.BLEND_CURVE_TYPE.SMOOTH_TO_SMOOOTH,
            lastBlend, curBlend
            ) ;

        t1.position = Vector3.Lerp(t1.position, t2.position, fraction);


        lastBlend = curBlend;
    }
}
