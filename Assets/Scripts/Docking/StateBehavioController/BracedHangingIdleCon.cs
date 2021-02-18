using Docking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BracedHangingIdleCon : BracedHangingConBase
{
    protected override void OnControllerEnter(int layerIndex, AnimatorStateInfo stateInfo)
    {
        //Debug.Log("Idle");
        base.OnControllerEnter(layerIndex, stateInfo);
    }

    protected override void OnControllerUpdate(int layerIndex, AnimatorStateInfo stateInfo)
    {        
        if (m_animator.IsInTransition(layerIndex)) return;        
        
        var input = GetRawInput();
        if (HasEnvCommitAction())
        {            
            var dir = input.normalized;        
            FindNextDockedHangTarget(input, false);
        }
        else if(Input.GetKeyDown(KeyCode.B))
        {
            FindNextDockedHangTarget(new Vector2(0, -1), true);
        }
        else if (Mathf.Abs(input.x) > 0.9f)
        {
            Debug.Log(input * 100);
            m_animator.SetTrigger("T_HangingMove");
            m_animator.SetFloat("Velocity", input.x > 0 ? 1 : -1);
        }
    }
}
