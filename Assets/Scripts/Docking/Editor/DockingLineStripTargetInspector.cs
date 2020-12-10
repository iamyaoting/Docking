using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Docking
{
    [CustomEditor(typeof(DockingLineStripTarget))]
    public class DockingLineStripTargetInspector : DockingTargetInspector
    {
        private DockingLineStripTarget script { get { return target as DockingLineStripTarget; } }

        private int selectedPoint = -1;

        void OnSceneGUI()
        {
            if (Application.isPlaying) return;
            //if (!Application.isPlaying) script.defaultLocalRotation = script.transform.localRotation;

            //// Quick Editing Tools
            //Handles.BeginGUI();
            //GUILayout.BeginArea(new Rect(10, 10, 200, 50), "Edit Window", "Window");

            //// Rotating display
            //if (GUILayout.Button("Add Vertex"))
            //{
            //    //if (!Application.isPlaying) Undo.RecordObject(script, "Rotate Display");
            //    //script.zeroAxisDisplayOffset += 90;
            //    //if (script.zeroAxisDisplayOffset >= 360) script.zeroAxisDisplayOffset = 0;
            //}

            //GUILayout.EndArea();
            //Handles.EndGUI();

            var vertices = script.m_vertices;         
            for (int i = 0; i < vertices.Length; ++i)
            {
                var positionWS = script.transform.TransformPoint(vertices[i].tr.translation);
                var rotationWS = script.transform.rotation * vertices[i].tr.rotation;
                Handles.color = script.GetGizmosColor();
                if (DotButton(positionWS, rotationWS, 0.04f, 0.08f))
                {
                    selectedPoint = i;
                }
                
                if(selectedPoint == i)
                {   
                    switch(Tools.current)
                    {
                        case Tool.Move:
                            var newpositionWS = Handles.PositionHandle(positionWS, rotationWS);
                            if (newpositionWS != positionWS)
                            {
                                Undo.RecordObject(script, "move point");
                                vertices[i].tr.translation = script.transform.InverseTransformPoint(newpositionWS);
                            }
                            break;
                        case Tool.Rotate:
                            var newRotationWS = Handles.RotationHandle(rotationWS, positionWS);
                            if (newRotationWS != rotationWS)
                            {
                                Undo.RecordObject(script, "rotate point");
                                vertices[i].tr.rotation = Quaternion.Inverse( script.transform.rotation) * newRotationWS;
                            }
                            break;
                    }                   
                }
            }
            
            
        }
    }

}
