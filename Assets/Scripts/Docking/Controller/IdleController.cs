using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleController : Controller
{
    protected override void Tick(float deltaTime)
    {
        if(HasEnvCommitAction())
        {
            if (null == m_nextControllerEnterContext)
            {
                m_nextControllerEnterContext = m_dockingDetector.GetNearestDockingTarget_Locomotion_High();
            }
            if (null == m_nextControllerEnterContext)
            {
                m_nextControllerEnterContext = m_dockingDetector.GetNearestDockingTarget_Locomotion_Low();
            }
            if (null == m_nextControllerEnterContext)// 往上寻找target
            {
                m_nextControllerEnterContext = m_dockingDetector.GetNearestDockingTarget_Hanging(new Vector2(0, 1), null);
            }
            if (null != m_nextControllerEnterContext)
            {
                m_nextControllerType = Docking.Utils.GetDefaultControllerTypeByTargetType(m_nextControllerEnterContext.dockingtarget.m_type);
            }
        }
    }
}