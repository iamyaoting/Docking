using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Docking;

public class TakeCoverCon : StateBehavioConBase
{
    private Vector2 m_lastInput;
    private DOCKED_POINT_MOVE_LIMIT m_limit;

    protected override void OnControllerUpdate(int layerIndex)
    {
        if (m_animator.IsInTransition(layerIndex)) return;

        var input = GetRawInput();
        input = HandleInputLimit(input, m_limit);
        input = Vector2.Lerp(m_lastInput, input, .2f);
        m_lastInput = input;
        m_animator.SetFloat("MoveDirection", input.x);

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
}
