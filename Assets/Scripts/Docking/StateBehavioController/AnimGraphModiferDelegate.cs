using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AnimGraphModiferDelegate : MonoBehaviour
{
    public delegate void PostEvaluateFuncDelegate();

    public PostEvaluateFuncDelegate PostEvalateFunc;
    public Vector3 velocity { get;  set; }

    private Vector3                 m_lastPos;


    private void Start()
    {
        PostEvalateFunc = null;
        velocity = Vector3.zero;
    }

    private void LateUpdate()
    {
        if (null != PostEvalateFunc) PostEvalateFunc();

        velocity = (transform.position - m_lastPos) / Time.deltaTime;
        m_lastPos = transform.position;

    }
}
