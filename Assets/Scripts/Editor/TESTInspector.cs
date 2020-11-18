using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TEST))]
public class TESTInspector : Editor
{
    public override void OnInspectorGUI()
    {
        TEST t = target as TEST;
        

        if(GUILayout.Button("test"))
        {
            //var path = AssetDatabase.GetAssetPath(t.clip.GetInstanceID());
            //var import = (ModelImporter)AssetImporter.GetAtPath(path);

            ////Debug.Log(t.clip);
            ////Debug.Log(t.clip.GetInstanceID());
            ////Debug.Log(path);

            //var anims = import.defaultClipAnimations;
            ////anims[0].takeName = "xxxx";
            //anims[0].name = "yyyy";


            //import.clipAnimations = anims;

            ////EditorUtility.SetDirty(import);
            ////import.SaveAndReimport();
            ////AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            ////AssetDatabase.Refresh();//刷新显示

            ////AssetDatabase.WriteImportSettingsIfDirty(path);

            //AssetDatabase.SaveAssets();
            //AssetDatabase.Refresh();

            EditorCurveBinding binding = new EditorCurveBinding();
            binding.type = typeof(Transform);
            binding.path = "DockingBone";

            //binding.propertyName = "localRotation.x";
            //AnimationUtility.SetEditorCurve(t.clip, binding, t.curve);
            //binding.propertyName = "localRotation.y";
            //AnimationUtility.SetEditorCurve(t.clip, binding, t.curve);
            //binding.propertyName = "localRotation.z";
            //AnimationUtility.SetEditorCurve(t.clip, binding, t.curve);
            //binding.propertyName = "localRotation.w";
            //AnimationUtility.SetEditorCurve(t.clip, binding, t.curve);

            //binding.propertyName = "localPosition.x";
            //AnimationUtility.SetEditorCurve(t.clip, binding, t.curve);
            //binding.propertyName = "localPosition.y";
            //AnimationUtility.SetEditorCurve(t.clip, binding, t.curve);
            //binding.propertyName = "localPosition.z";
            //AnimationUtility.SetEditorCurve(t.clip, binding, t.curve);

            var relativePath = "DD";
            var clip = t.clip;
            clip.SetCurve(relativePath, typeof(Transform), "localPosition.x", t.curve);
            clip.SetCurve(relativePath, typeof(Transform), "localPosition.y", t.curve);
            clip.SetCurve(relativePath, typeof(Transform), "localPosition.z", t.curve);

            t.clip.EnsureQuaternionContinuity();
           
            EditorUtility.SetDirty(t.clip);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        base.OnInspectorGUI();
    }
}
