using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullBodyIKModifier
{
    public Vector3 leftFootTargetOffset { get; set; }
    public Vector3 rightFootTargetOffset { get; set; }
    public Vector3 leftHandTargetOffset { get; set; }
    public Vector3 rightHandTargetOffset { get; set; }

    // weight的blend的混合时间
    public float blendTime { get; set; } = .5f;

    private RootMotion.FinalIK.BipedIK m_ik;    
    private AvatarIKGoal[] m_IKGoals = { AvatarIKGoal.LeftHand, AvatarIKGoal.RightHand, 
        AvatarIKGoal.LeftFoot, AvatarIKGoal.RightFoot };
    
    private float m_weight = 0.0f;
    private float m_remainingTime = 0.0f;
    private bool m_enable = false;
    
    public FullBodyIKModifier(RootMotion.SolverManager ik)
    {
        if(null == ik)
        {
            Debug.LogError("Can not find the Full Body IK!!");
        }
        m_ik = ik as RootMotion.FinalIK.BipedIK;
        for(int i = 0; i < m_IKGoals.Length; ++i)
        {
            GetIKEffector(m_IKGoals[i]).SetIKPositionWeight(0.0f);
        }
    }

    public void SetEnableIK(bool enable)
    {
        if (m_enable == enable) return;
        m_enable = enable;
        m_remainingTime = blendTime;
    }

    public void OnSolver(float deltaTime)
    {
        UpdateBlendWeight(deltaTime);
        for(int i = 0; i < m_IKGoals.Length; ++i)
        {
            var ikGoal = m_IKGoals[i];
            GetIKEffector(ikGoal).SetIKPosition(GetTransform(ikGoal).position + GetIKOffset(ikGoal));
            GetIKEffector(ikGoal).SetIKPositionWeight(m_weight);
            GetIKEffector(ikGoal).maintainRotationWeight = 1.0f;
        }
    }

    private void UpdateBlendWeight(float deltaTime)
    {
        m_remainingTime -= deltaTime;
        m_remainingTime = Mathf.Clamp(m_remainingTime, 0, m_remainingTime);
        if(m_enable)
        {
            m_weight = 1.0f - m_remainingTime / blendTime;
        }
        else
        {
            m_weight = m_remainingTime / blendTime;
        }
    }
    private Transform GetTransform(AvatarIKGoal goal)
    {
        switch (goal)
        {
            case AvatarIKGoal.LeftHand:
                return m_ik.references.leftHand;
            case AvatarIKGoal.RightHand:
                return m_ik.references.rightHand;
            case AvatarIKGoal.LeftFoot:
                return m_ik.references.leftFoot;
            case AvatarIKGoal.RightFoot:
                return m_ik.references.rightFoot;
        }
        return null;
    }

    private Vector3 GetIKOffset(AvatarIKGoal goal)
    {
        switch (goal)
        {
            case AvatarIKGoal.LeftHand:
                return leftHandTargetOffset;
            case AvatarIKGoal.RightHand:
                return rightHandTargetOffset;
            case AvatarIKGoal.LeftFoot:
                return leftFootTargetOffset;
            case AvatarIKGoal.RightFoot:
                return rightFootTargetOffset;
        }
        return Vector3.zero;
    }
    private RootMotion.FinalIK.IKSolverLimb GetIKEffector(AvatarIKGoal goal)
    {
        switch (goal)
        {
            case AvatarIKGoal.LeftHand:
                return m_ik.solvers.leftHand;
            case AvatarIKGoal.RightHand:
                return m_ik.solvers.rightHand;
            case AvatarIKGoal.LeftFoot:
                return m_ik.solvers.leftFoot;
            case AvatarIKGoal.RightFoot:
                return m_ik.solvers.rightFoot;
        }
        return null;
    }
}
