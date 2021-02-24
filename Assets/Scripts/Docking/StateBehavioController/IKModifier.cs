using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullBodyIKModifier
{
    public Vector3 lHandOffset;
    public Vector3 rHandOffset;
    public Vector3 lFootOffset;
    public Vector3 rFootOffset;

    // weight的blend的混合时间
    public float blendTime { get; set; } = .5f;

    private RootMotion.FinalIK.FullBodyBipedIK m_ik;    
    
    private float m_weight = 0.0f;
    private float m_remainingTime = 0.0f;
    private bool m_enable = false;
    protected RootMotion.FinalIK.IKEffector lHandEff { get; private set; }
    protected RootMotion.FinalIK.IKEffector rHandEff { get; private set; }
    protected RootMotion.FinalIK.IKEffector lFootEff { get; private set; }
    protected RootMotion.FinalIK.IKEffector rFootEff { get; private set; }


    public FullBodyIKModifier(RootMotion.SolverManager ik)
    {
        if(null == ik)
        {
            Debug.LogError("Can not find the Full Body IK!!");
        }
        m_ik = ik as RootMotion.FinalIK.FullBodyBipedIK;

        lHandEff = m_ik.solver.leftHandEffector;     
        rHandEff = m_ik.solver.rightHandEffector;
        lFootEff = m_ik.solver.leftFootEffector;
        rFootEff = m_ik.solver.rightFootEffector;

        // 这个权重针对IKPosition定点求解
        //lHandEff.positionWeight = 1.0f;
        //rHandEff.positionWeight = 1.0f;
        //lFootEff.positionWeight = 1.0f;
        //rFootEff.positionWeight = 1.0f;

        //lHandEff.rotationWeight = 1.0f;
        //rHandEff.rotationWeight = 1.0f;
        //lFootEff.rotationWeight = 1.0f;
        //rFootEff.rotationWeight = 1.0f;

        m_ik.solver.IKPositionWeight = 0.0f;
    }

    public void CalcFootIK(Docking.TR dockedTR)
    {
        var animator = m_ik.GetComponent<Animator>();
        if (animator == null) return;
    
        var rootTrans = animator.transform;
        var lFootTrans = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
        var rFootTrans = animator.GetBoneTransform(HumanBodyBones.RightFoot);
        

        Vector3 lFootPoint, rFootPoint;
        if (Docking.Utils.DetectHangingFootContactPoint(rootTrans, lFootTrans, out lFootPoint)
            && Docking.Utils.DetectHangingFootContactPoint(rootTrans, rFootTrans, out rFootPoint))
        {
            var hipTrans = animator.GetBoneTransform(HumanBodyBones.Hips);
            CalcuRotatePitchPlane(lFootPoint, rFootPoint, dockedTR.translation,
                dockedTR.rotation * Vector3.forward * -1, hipTrans, m_weight);
        }
                  
    }


    // 为攀爬Foot IK服务的平面计算，且该平面通过原有平面的点
    public void CalcuRotatePitchPlane(Vector3 leftFootPoint, Vector3 rightFootPoint, Vector3 dockedPoint, Vector3 normalAxis, Transform hip, float weight)
    {
        Vector3 pitchAxis = Vector3.Cross(normalAxis, Vector3.up);
        var lFootBaseLinePoint = Docking.Utils.ProjectPointToLine(leftFootPoint, pitchAxis, dockedPoint);
        var lFootDir = leftFootPoint - lFootBaseLinePoint;
        var rFootBaseLinePoint = Docking.Utils.ProjectPointToLine(rightFootPoint, pitchAxis, dockedPoint);
        var rFootDir = rightFootPoint - rFootBaseLinePoint;

        var lFootDirOnPlane = Vector3.ProjectOnPlane(lFootDir, normalAxis);
        var rFootDirOnPlane = Vector3.ProjectOnPlane(rFootDir, normalAxis);        

        var lAngle = Vector3.Angle(lFootDir, lFootDirOnPlane);
        var rAngle = Vector3.Angle(rFootDir, rFootDirOnPlane);
        var pitchQuat = Quaternion.AngleAxis(Mathf.Max(lAngle, rAngle) * weight, pitchAxis);

        hip.rotation = pitchQuat * hip.rotation;
        var hipBasePoint = Docking.Utils.ProjectPointToLine(hip.position, pitchAxis, dockedPoint);
        var hipDir = hip.position - hipBasePoint;
        hipDir = pitchQuat * hipDir;
        hip.position = hipBasePoint + hipDir;

        var lFootPointProjectPlane = pitchQuat * lFootDirOnPlane + lFootBaseLinePoint;
        var rFootPointProjectPlane = pitchQuat * rFootDirOnPlane + rFootBaseLinePoint;

        lFootOffset = leftFootPoint - lFootPointProjectPlane;
        rFootOffset = rightFootPoint - rFootPointProjectPlane;

        return;
    }
    public void SetEnableIK(bool enable)
    {
        if (m_enable == enable) return;
        m_enable = enable;
        m_remainingTime = blendTime;
    }

    public bool NeedSolveIK()
    {
        if (m_weight == 0.0f && m_enable == false) return false;
        return true;
    }

    public void OnIKUpdate(float deltaTime)
    {
        UpdateBlendWeight(deltaTime);

        
        // full body ik 每帧都会清零position offset
        lHandEff.positionOffset = lHandOffset;
        rHandEff.positionOffset = rHandOffset;
        lFootEff.positionOffset = lFootOffset;
        rFootEff.positionOffset = rFootOffset;

        //var lHandOffset = leftHandTargetOffset.y;
        //var rHandOffset = rightHandTargetOffset.y;
        //var hipOffset = Mathf.Max(lHandOffset, rHandOffset) * 1.2f;
        //leftHandTargetOffset -= hipOffset * Vector3.up;
        //rightHandTargetOffset -= hipOffset * Vector3.up;
        //hipTargetOffset = hipOffset * Vector3.up;

        //Debug.Log(hipOffset);
        //Debug.Log(leftHandTargetOffset);
        //Debug.Log(rightHandTargetOffset);
        //Debug.Log(lHandOffset);

        //lHandEff.position = leftHandTarget;
        //rHandEff.position = rightHandTarget;
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

        m_ik.solver.IKPositionWeight = m_weight;
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
}
