using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Docking;

public class TakeCoverCon : StateBehavioConBase
{
    private Vector2 m_lastInput;
    private DOCKED_POINT_MOVE_LIMIT m_limit;

    protected override void OnControllerUpdate(int layerIndex, AnimatorStateInfo stateInfo)
    {
        if (m_animator.IsInTransition(layerIndex)) return;

        var input = GetRawInput();
        input = HandleInputLimit(input, m_limit);
        input = Vector2.Lerp(m_lastInput, input, .2f);
        m_lastInput = input;
        m_animator.SetFloat("Velocity", input.x);

        m_limit = DOCKED_POINT_MOVE_LIMIT.NONE;
    }

    protected override void OnDockingTargetUpdate(DockingTarget target, TR tr, DockedVertexStatus status)
    {
        if (null != status)
        {
            m_animator.SetFloat("Height", status.reserveFloatParam);
            m_limit = status.limit;
        }
        base.OnDockingTargetUpdate(target, tr, status);
    }

    // 边缘处响应函数
    protected override void OnDockingTargetMargin(DockingTarget target, TR tr, DockedVertexStatus status)
    {
        if(Input.GetKey(KeyCode.Mouse0)) // 鼠标左键按住进行切换aim动作
        {
            if((status.limit & DOCKED_POINT_MOVE_LIMIT.HORIZEN_LEFT_FORBIDEN) != DOCKED_POINT_MOVE_LIMIT.NONE)
            {
                m_animator.SetFloat("LeftRightSelctor", -1);
            }
            if ((status.limit & DOCKED_POINT_MOVE_LIMIT.HORIZEN_RIGHT_FORBIDEN) != DOCKED_POINT_MOVE_LIMIT.NONE)
            {
                m_animator.SetFloat("LeftRightSelctor", 1);
            }
            m_animator.CrossFade("TakeCover.Cover2Aim", 0.2f);
        }

        base.OnDockingTargetMargin(target, tr, status);
    }
}
