using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpWoodenPileLoopCon : JumpWoodenPileConBase
{    
    private Vector3             m_curVel;
    private float               m_totalNormalizedTime = -1;    
    private float               m_previousDockngBlend = 0;
    
    private float               m_animLen = 0;
    private float               m_lastNormalizedTime = 0;
    private float               m_curNormalizedTime = 0;


    protected override void OnControllerEnter(int layerIndex, AnimatorStateInfo stateInfo)
    {
        base.OnControllerEnter(layerIndex, stateInfo);

        m_curVel = Vector3.zero;
        m_totalNormalizedTime = -1;
        m_previousDockngBlend = 0;
        
        m_animLen = stateInfo.length;
        m_lastNormalizedTime = 0;
        m_curNormalizedTime = 0;
        
        var postModifer = m_animator.GetComponent<AnimGraphModiferDelegate>();
        postModifer.PostEvalateFunc += PostEvaluate;

        // 计算整个跳跃过程所花费的时间，以y轴为准
        m_curVel = postModifer.velocity;
        float totaltime = GetTotalTimeAtInitVel_Y(m_curVel.y, m_targetPoint.translation.y - m_animator.rootPosition.y);
        m_totalNormalizedTime = Mathf.Floor((totaltime / stateInfo.length));

        Debug.Log("Len = " + m_totalNormalizedTime * stateInfo.length + "   vel=" + m_curVel);
   
        //m_animator.applyRootMotion = false;
    }

    protected override void OnControllerUpdate(int layerIndex, AnimatorStateInfo stateInfo)
    {
        if (m_animator.IsInTransition(layerIndex)) return;        

        // 更改docking driver controller data数据
        m_animator.ResetTrigger("Commit");
        m_dockingDriver.GetDockingControllerData().m_previousDockingBlend = m_previousDockngBlend;
        var curDockingBlend = stateInfo.normalizedTime / m_totalNormalizedTime;
        m_dockingDriver.GetDockingControllerData().m_dockingBlend = curDockingBlend;
        m_previousDockngBlend = curDockingBlend;

        if(stateInfo.normalizedTime + 1 > m_totalNormalizedTime)
        {
            SetDockingCommit();
            Debug.Break();
        }

        m_lastNormalizedTime = m_curNormalizedTime;
        m_curNormalizedTime = stateInfo.normalizedTime;
        
        base.OnControllerUpdate(layerIndex, stateInfo);
    }

    private void PostEvaluate()
    {
        if (m_animator.IsInTransition(0)) return;

        // 物理模拟处理root节点
        float deltaTime = (m_curNormalizedTime - m_lastNormalizedTime) * m_animLen;
        var nextStepV = m_curVel - m_g * Vector3.up * deltaTime;
        m_animator.transform.position = m_animator.transform.position + 0.5f * deltaTime *(m_curVel + nextStepV);
        m_curVel = nextStepV;

        // 计算合理的docking Bone 信息
        var pos = GetDesiredPoint(m_curNormalizedTime);
        var rotation = m_animator.rootRotation;
        var boneTrans = Docking.Utils.GetDockingBoneTransform(m_animator);
        boneTrans.position = pos;
        boneTrans.rotation = rotation;        
    }

    private Vector3 GetDesiredPoint(float curNormalizedTime)
    {
        var time = (m_totalNormalizedTime - curNormalizedTime) * m_animLen;
        var pos = m_animator.transform.position;       
        var vel = m_curVel;
        float vel_y = m_curVel.y;
        vel.y = 0;
        pos += vel * time;
        pos.y += vel_y * time - 0.5f * m_g * time * time;
        return pos;
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_animator.GetComponent<AnimGraphModiferDelegate>().PostEvalateFunc -= PostEvaluate;
        m_animator.applyRootMotion = true;
        base.OnStateExit(animator, stateInfo, layerIndex);
    }
}
