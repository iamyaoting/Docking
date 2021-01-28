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

        t.curve.AddKey(0.1666667f, 2.90f);

        if(GUILayout.Button("test"))
        {
            SaveAnimationClip(t.clip, Docking.Utils.GetDockingBoneName(), t.curve);
        } 

        base.OnInspectorGUI();
    }


    private static void SaveAnimationClip(AnimationClip clip, string dockingBonePath,
            AnimationCurve curve)
    {
        // 先删除原来的curve，因为直接setcurve会实施combine curve行为
        clip.SetCurve(dockingBonePath, typeof(Transform), "localPosition", null);
        clip.SetCurve(dockingBonePath, typeof(Transform), "localRotation", null);

        // 记录docking bone 轨迹到 animation clip
        clip.SetCurve(dockingBonePath, typeof(Transform), "localPosition.x", curve);
        clip.SetCurve(dockingBonePath, typeof(Transform), "localPosition.y", curve);
        clip.SetCurve(dockingBonePath, typeof(Transform), "localPosition.z", curve);

        clip.SetCurve(dockingBonePath, typeof(Transform), "localRotation.x", curve);
        clip.SetCurve(dockingBonePath, typeof(Transform), "localRotation.y", curve);
        clip.SetCurve(dockingBonePath, typeof(Transform), "localRotation.z", curve);
        clip.SetCurve(dockingBonePath, typeof(Transform), "localRotation.w", curve);

        clip.EnsureQuaternionContinuity();

        // 保存文件
        EditorUtility.SetDirty(clip);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
