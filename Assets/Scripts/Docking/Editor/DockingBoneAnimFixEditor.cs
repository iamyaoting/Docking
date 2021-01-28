using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Docking
{
    public class DockingBoneAnimFixEditor : EditorWindow
    {
        AnimationClip clip;

        // Add menu named "My Window" to the Window menu
        [MenuItem("Docking/Fix DockingBone Anim")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            DockingBoneAnimFixEditor window = (DockingBoneAnimFixEditor)EditorWindow.GetWindow(typeof(DockingBoneAnimFixEditor));
            window.Show();
        }

        void OnGUI()
        {
            GUILayout.Label("Fix Docking Anim", EditorStyles.boldLabel);
            //clip = EditorGUILayout.PropertyField("Text Field", clip);
            clip = EditorGUILayout.ObjectField("Animation clip", clip, typeof(AnimationClip), false) as AnimationClip;

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            if (GUILayout.Button("Fix"))
            {
                if (null != clip)
                {
                    var dockingBonePath = Utils.GetDockingBoneName(); 
                    var boneTransCurves = GetCurvesFromCurve(clip, dockingBonePath);
                    DockingAnimationSetInspector.SaveAnimationClip(clip, dockingBonePath, boneTransCurves);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        BoneTransfromCurve GetCurvesFromCurve(AnimationClip clip, string dockingBonePath)
        {
            System.Func<string, AnimationClip, AnimationCurve> GetCurve = (propertyName, clip) =>
            {
                EditorCurveBinding binding = new EditorCurveBinding();
                binding.path = dockingBonePath;
                binding.propertyName = propertyName;                
                var curve = AnimationUtility.GetEditorCurve(clip, binding);
                return curve;
            };

            string[] propertyNames = 
            {
                "localPosition.x",
                "localPosition.y",
                "localPosition.z",
                "localRotation.x",
                "localRotation.y",
                "localRotation.z",
                "localRotation.w"
            };
            BoneTransfromCurve boneCurves = new BoneTransfromCurve();
            AnimationCurve[] curves =
            {
                boneCurves.posX,
                boneCurves.posY,
                boneCurves.posZ,
                boneCurves.quatX,
                boneCurves.quatY,
                boneCurves.quatZ,
                boneCurves.quatW
            };

            for(int i = 0; i < 7; ++i)
            {
                curves[i] = GetCurve(propertyNames[i], clip);                
            }

            return boneCurves;
        }
    }

}
