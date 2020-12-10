using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Docking
{
    public class DockingControllerFactory
    {
        public static DockingController CreateDockingControler(DockingTarget target)
        {
            if (target.m_type == DockingTargetType.TAKE_COVER)
                return new TakeCoverController();

            return null;
        }
    }
}

