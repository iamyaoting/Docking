using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Docking
{
    [CustomEditor(typeof(DockingGenerator))]
    public class DockingGeneratorInspector : Editor
    {
        private DockingGenerator script { get { return target as DockingGenerator; } }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            script.m_flags = (DockingFlagBits)EditorGUILayout.EnumFlagsField("Flags", script.m_flags);
        }
    }
}
