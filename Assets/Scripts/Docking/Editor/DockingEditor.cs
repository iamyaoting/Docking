using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace Docking
{
    public class DockingEditor : EditorWindow
    {
        //string myString = "Hello World";
        //bool groupEnabled;
        //bool myBool = true;
        //float myFloat = 1.23f;
        RuntimeAnimatorController animController;
        AnimatorController s;
        AnimationClip c;

        [MenuItem("Window/DockingEditor")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            DockingEditor window = (DockingEditor)EditorWindow.GetWindow(typeof(DockingEditor));
            window.Show();
        }

        void DoSimulate()
        {
            
        }

        void OnGUI()
        {
            EditorGUILayout.ObjectField("animator controller", animController, animController.GetType(), false);
            //GUILayout.Label("Base Settings", EditorStyles.boldLabel);
            //myString = EditorGUILayout.TextField("Text Field", myString);

            //groupEnabled = EditorGUILayout.BeginToggleGroup("Optional Settings", groupEnabled);
            //myBool = EditorGUILayout.Toggle("Toggle", myBool);
            //myFloat = EditorGUILayout.Slider("Slider", myFloat, -3, 3);
            //EditorGUILayout.EndToggleGroup();
        }
    }
}

