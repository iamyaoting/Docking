using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Docking
{
    public class OutValutController : DockingController
    {
        public override void OnEnter(ControllerEnterContext context)
        {            
            base.OnEnter(context);
        }

        public override void Tick(float deltaTime)
        {            
        }

        public override void OnFSMStateExit()
        {
            m_nextControllerEnterContext = null;
            m_nextControllerType = typeof(IdleController);
            //Time.timeScale = 0;
        }
    }

}
