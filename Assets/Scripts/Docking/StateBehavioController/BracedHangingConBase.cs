using Docking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BracedHangingConBase : StateBehavioConBase
{   
    protected bool CrossFadeAnimatorHopState(Vector3 targetPoint)
    {
        targetPoint = Utils.GetDockingBoneTransform(m_animator).InverseTransformPoint(targetPoint);
        Vector2 direction = targetPoint;
        if (direction.magnitude == 0.0) return false;
        float angle = Vector2.SignedAngle(Vector2.up, direction);

        if (angle < 45 && angle > -45)
        {
            //m_animator.SetTrigger("T_HangHopUp");
            m_animator.CrossFadeInFixedTime("Braced Hang Hop Up", 0.2f);
        }
        else if (angle >= 45 && angle <= 135)
        {
            //m_animator.SetTrigger("T_HangHopLeft");
            m_animator.CrossFadeInFixedTime("Braced Hang Hop Left", 0.2f);
        }
        else if (angle > 135 || angle < -135)
        {
            //m_animator.SetTrigger("T_HangHopDown");
            m_animator.CrossFadeInFixedTime("Braced Hang Hop Down", 0.2f);
        }
        else
        {
            //m_animator.SetTrigger("T_HangHopRight");
            m_animator.CrossFadeInFixedTime("Braced Hang Hop Right", 0.2f);
        }
        return true;
    }

    protected bool FindNextDockedHangTarget(Vector2 input)
    {
        var context = m_dockingDetector.GetNearestDockingTargetBySingleType(DetectorType.HangDetector, input, m_dockingDriver.GetDockingTarget());
        if (null != context && context.dockingtarget.m_type == DockingTargetType.BRACED_HANG)
        {
            CrossFadeAnimatorHopState(context.desiredDockedVertex.tr.translation);
            m_dockingDriver.SetDockingNextTarget(context.dockingtarget);
            return true;
        }
        return false;
    }
}
