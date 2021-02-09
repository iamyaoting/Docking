using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DockingTargetChecker : MonoBehaviour
{
    public Docking.DockingTarget target;
    public bool left2right = true;
    private float alpha;
    private Docking.DockingTarget curTarget;

    // Start is called before the first frame update
    void Start()
    {
        curTarget = target;   
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDrawGizmos()
    {
        if (null == curTarget) return;
        float step = 0.002f;
        if(left2right)
        {
            alpha += step;
            if(alpha > .95f)
            {
                curTarget = curTarget.m_rightTarget;
                alpha = .0f;
            }
        }
        else
        {
            alpha -= step;
            if(alpha < 0.05f)
            {
                curTarget = curTarget.m_leftTarget;
                alpha = 1.0f;
            }
        }

        var vertex = curTarget.GetDockedWS(alpha);
        Gizmos.DrawSphere(vertex.tr.translation, 0.1f);               
    }
}
