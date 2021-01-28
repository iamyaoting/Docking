using Docking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SimpleStartCon : StateBehavioConBase
{
    protected override void OnControllerUpdate(int layerIndex, AnimatorStateInfo stateInfo)
    {                
        if (HasEnvCommitAction())
        {
            SetDockingCommit();
        }
    }    
}
