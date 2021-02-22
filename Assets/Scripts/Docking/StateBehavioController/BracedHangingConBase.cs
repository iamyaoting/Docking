using Docking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BracedHangingConBase : StateBehavioConBase
{   
    private bool CrossFadeAnimatorHopState(Vector3 targetPoint, Quaternion quat)
    {
        targetPoint = Utils.GetDockingBoneTransform(m_animator).InverseTransformPoint(targetPoint);
        
        float pivotAngle = Vector2.SignedAngle(Vector2.up, new Vector2(targetPoint.x, targetPoint.y));
        float backAngle = Vector3.Angle(targetPoint, -Vector3.forward);           
        float orientDiff = Quaternion.Angle(Utils.GetDockingBoneTransform(m_animator).rotation, quat);

        Debug.Log("pivotAngle, backAngle, orientDiff: " + pivotAngle + ", " + backAngle + ", " + orientDiff);

        if (backAngle > 70 && backAngle < 110)
        {
            if(orientDiff < 120) // 朝向不要超过120
            {
                if (pivotAngle < 45 && pivotAngle > -45)
                {
                    m_animator.CrossFadeInFixedTime("Braced Hang Hop Up", 0.2f);
                }
                else if (pivotAngle >= 45 && pivotAngle <= 135)
                {
                    m_animator.CrossFadeInFixedTime("Braced Hang Hop Left", 0.2f);
                }
                else if (pivotAngle > 135 || pivotAngle < -135)
                {
                    m_animator.CrossFadeInFixedTime("Braced Hang Hop Down", 0.2f);
                }
                else
                {
                    m_animator.CrossFadeInFixedTime("Braced Hang Hop Right", 0.2f);
                }
            }            
        }
        else if(backAngle < 60)
        {
            if(orientDiff > 80)
            {
                if (targetPoint.x > 0)
                {
                    m_animator.CrossFadeInFixedTime("Braced Hang Hop Back Right", 0.15f);
                }
                else
                {
                    m_animator.CrossFadeInFixedTime("Braced Hang Hop Back Left", 0.15f);
                }
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
        return true;
    }

    protected bool FindNextDockedHangTarget(Vector2 input, bool findBackTarget)
    {
        ControllerEnterContext context = null;        
        if (findBackTarget)
        {
            context = m_dockingDetector.GetNearestDockingTargetByMultiTypes(DetectorType.HangBackDetector, input, m_dockingDriver.GetDockingTarget());
        }
        else
        {
            context = m_dockingDetector.GetNearestDockingTargetBySingleType(DetectorType.HangDetector, input, m_dockingDriver.GetDockingTarget());
        }

        if (null != context && context.dockingtarget.m_type == DockingTargetType.BRACED_HANG)
        {
            if (CrossFadeAnimatorHopState(context.desiredDockedVertex.tr.translation, context.desiredDockedVertex.tr.rotation))
            {
                //m_dockingDriver.SetDockingNextTarget(context.dockingtarget);
                m_dockingDriver.SetDockingNextTargetFixedFuturePoint(context.dockingtarget, context.desiredDockedVertex, context.desiredDockedVertexStatus);
            }
            return true;
        }
        return false;
    }
}
