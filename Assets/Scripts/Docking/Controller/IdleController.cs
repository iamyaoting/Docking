using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleController : Controller
{
    protected override void Tick(float deltaTime)
    {
        if(HasEnvCommitAction())
        {
            m_nextControllerEnterContext = m_dockingDetector.GetNearestDockingTarget_Locomotion();
            if (null == m_nextControllerEnterContext) return;
            m_nextControllerType = Docking.Utils.GetDefaultControllerTypeByTargetType(m_nextControllerEnterContext.dockingtarget.m_type);
        }
    }
}