using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpWoodenPileConBase : StateBehavioConBase
{
    protected Docking.TR m_targetPoint = new Docking.TR();
    private Docking.DockingVertexTarget m_vertexTarget;
    protected float m_g = 10f;

    protected override void OnControllerEnter(int layerIndex, AnimatorStateInfo stateInfo)
    {
        m_vertexTarget = m_dockingDriver.GetDockingTarget() as Docking.DockingVertexTarget;
        if(null == m_vertexTarget)
        {
            Debug.LogError("Docking vertex target can't be null!");
            return;
        }
        m_targetPoint.translation = m_vertexTarget.transform.TransformPoint(m_vertexTarget.m_desiredVertex.tr.translation);       
    }

    // 计算角色跳多高需要花费的时间
    protected float GetFallingTime(float height)
    {
        return Mathf.Sqrt(height * 2 / m_g);
    }

    // 计算跳上高台所需要的时间,height 可以为负值
    protected float GetTotalTimeAtInitVel_Y(float vel_Y, float height)
    {        
        float preApexTime = vel_Y / m_g;
        float postApexY = preApexTime * preApexTime * m_g / 2 - height;        
        float postApexTime = GetFallingTime(postApexY);
        return preApexTime + postApexTime;
    }
}
