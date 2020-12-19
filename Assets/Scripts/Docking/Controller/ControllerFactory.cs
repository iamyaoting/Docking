using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Docking;

public class ControllerFactory
{
    public static Controller CreateControolerByType(System.Type type)
    {
        var obj = type.Assembly.CreateInstance(type.FullName);
        if (obj is Controller) return obj as Controller;
        Debug.LogError("不能生成Controller对象，type: " + type.Name + " obj: " + obj.ToString());
        return null;
    }

    public static DockingController CreateDockingControlerByTarget(DockingTarget target)
    {
        var type = Docking.Utils.GetDefaultControllerTypeByTargetType(target.m_type);
        var dockingCon = CreateControolerByType(type);
        if (dockingCon is DockingController) return dockingCon as DockingController;
        Debug.LogError("不能生成DockingController对象，type: " + type.Name + " obj: " + target.ToString());
        return null;
    }
}


