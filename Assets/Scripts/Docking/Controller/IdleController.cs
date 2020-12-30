﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdleController : Controller
{
    public override void Tick(float deltaTime)
    {
        if(HasEnvInteractiveActionUserInput())
        {
            m_nextControllerEnterContext = GetNearestDockingTarget();
            if (null == m_nextControllerEnterContext) return;
            m_nextControllerType = Docking.Utils.GetDefaultControllerTypeByTargetType(m_nextControllerEnterContext.dockingtarget.m_type);
        }
    }
}