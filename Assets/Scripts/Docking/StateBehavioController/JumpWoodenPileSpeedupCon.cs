using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpWoodenPileSpeedupCon : JumpWoodenPileConBase
{
    public float m_startSpeedupNormalizedTime = 0;
    public float m_endSpeedupNormalizedTime = 1;

    private Vector3 m_desiredInitVel;
    private float m_jumpVelYAmplifyRatio = 1.5f;
    private float m_jumpMinVelY = 3.0f;
    private float m_jumpMaxVelY = 8.0f;
    private Vector3 m_curVel;

    protected override void OnControllerEnter(int layerIndex, AnimatorStateInfo stateInfo)
    {
        base.OnControllerEnter(layerIndex, stateInfo);

        m_desiredInitVel = CalcDesiredVelocity();      

        m_animator.applyRootMotion = false;

        //Debug.Break();
    }


    protected override void OnControllerUpdate(int layerIndex, AnimatorStateInfo stateInfo)
    {
        

        float nt = stateInfo.normalizedTime;
        if(nt <= m_endSpeedupNormalizedTime)
        {
            //float alpha = (nt - m_startSpeedupNormalizedTime) / (m_endSpeedupNormalizedTime - m_startSpeedupNormalizedTime);
            //m_animator.transform.position = m_animator.transform.position + Vector3.Lerp(Vector3.zero, m_desiredInitVel, alpha) * Time.deltaTime;
            m_desiredInitVel = CalcDesiredVelocity();
            m_curVel = m_desiredInitVel;
        }
        else
        {
            m_curVel += -m_g * Time.deltaTime * Vector3.up;
            m_animator.transform.position = m_animator.transform.position +  m_curVel * Time.deltaTime;
        }       

        base.OnControllerUpdate(layerIndex, stateInfo);
    }

    protected Vector3 CalcDesiredVelocity()
    {
        // 跳跃的最高值
        float maxJumpHeight = m_jumpMaxVelY * m_jumpMaxVelY / 2 / m_g;
        var pileDir = m_targetPoint.translation - m_animator.transform.position;
        if (pileDir.y > maxJumpHeight)
        {
            Debug.LogError("Can not jump so high !   " + pileDir.y + " < " + maxJumpHeight);
        }
        // 计算该有的速度_Y
        var distanceXZ = pileDir;
        distanceXZ.y = 0;
        float velY = Mathf.Sqrt(2 * Mathf.Abs(m_targetPoint.translation.y - m_animator.transform.position.y) * m_g);
        velY = Mathf.Clamp(velY * m_jumpVelYAmplifyRatio, m_jumpMinVelY, m_jumpMaxVelY);
        float time = GetTotalTimeAtInitVel_Y(velY, pileDir.y);

        var desiredInitVel = pileDir / time;
        desiredInitVel.y = velY;
        Debug.Log("Jump Init Vel= " + desiredInitVel);
        return desiredInitVel;
    }
}
