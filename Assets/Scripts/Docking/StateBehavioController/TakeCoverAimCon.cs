using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Docking;

public class TakeCoverAimCon : StateBehavioConBase
{
    private Vector2 m_lastInput;
    private DOCKED_POINT_MOVE_LIMIT m_limit;

    protected override void OnControllerUpdate(int layerIndex, AnimatorStateInfo stateInfo)
    {
        if (m_animator.IsInTransition(layerIndex)) return;

        if(!Input.GetKey(KeyCode.Mouse0))
        {
            SetUnDocking();
        }
    }    
}
