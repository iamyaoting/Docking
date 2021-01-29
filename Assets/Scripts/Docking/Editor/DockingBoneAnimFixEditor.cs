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
                    if (null != boneTransCurves)
                    {
                        DockingAnimationSetInspector.SaveAnimationClip(clip, dockingBonePath, boneTransCurves);
                    }
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
                binding.type = typeof(Transform);
                var curve = AnimationUtility.GetEditorCurve(clip, binding);
                return curve;
            };

            string[] propertyNames =
            {
                "m_LocalPosition.x",
                "m_LocalPosition.y",
                "m_LocalPosition.z",
                "m_LocalRotation.x",
                "m_LocalRotation.y",
                "m_LocalRotation.z",
                "m_LocalRotation.w"
            };
            BoneTransfromCurve boneCurves = new BoneTransfromCurve();
            AnimationCurve[] curves = new AnimationCurve[7];

            for (int i = 0; i < 7; ++i)
            {
                curves[i] = GetCurve(propertyNames[i], clip);
                if (null == curves[i])
                {
                    Debug.LogError("Can not find curve: " + propertyNames[i]);
                    return null;
                }
                var len = curves[i].length;
                if (curves[i].keys[len - 1].time - curves[i].keys[len - 2].time < Utils.GetFloatZeroThreshold())
                {
                    curves[i].RemoveKey(len - 2);
                }
            }

            boneCurves.posX = curves[0];
            boneCurves.posY = curves[1];
            boneCurves.posZ = curves[2];

            boneCurves.quatX = curves[3];
            boneCurves.quatY = curves[4];
            boneCurves.quatZ = curves[5];
            boneCurves.quatW = curves[6];

            return boneCurves;
        }
    }

}
