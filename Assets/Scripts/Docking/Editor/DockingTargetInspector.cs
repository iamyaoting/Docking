using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Docking
{
    public class DockingTargetInspector : Editor
    {
        protected static bool DotButton(Vector3 position, Quaternion direction, float size, float pickSize)
        {
#if UNITY_5_6_OR_NEWER
            return Handles.Button(position, direction, size, pickSize, Handles.DotHandleCap);
#else
			return Handles.Button(position, direction, size, pickSize, Handles.DotCap);
#endif
        }
    }
}

