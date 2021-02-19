using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Docking
{
    public class DockingAddMotionEditor : EditorWindow
    {
        GameObject character;
        AnimationClip clip;
        Vector3 initPos;

        // Add menu named "My Window" to the Window menu
        [MenuItem("Docking/Add Motion")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            DockingAddMotionEditor window = (DockingAddMotionEditor)EditorWindow.GetWindow(typeof(DockingAddMotionEditor));
            window.Show();
        }

        void OnGUI()
        {
            GUILayout.Label("Add Animation motion", EditorStyles.boldLabel);

            character = EditorGUILayout.ObjectField("Character", character, typeof(GameObject), false) as GameObject;
            clip = EditorGUILayout.ObjectField("Animation clip", clip, typeof(AnimationClip), false) as AnimationClip;

            initPos = EditorGUILayout.Vector3Field("Init Pos", initPos);


            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();

            if (GUILayout.Button("Add Motion"))
            {
                if (null != clip)
                {
                    var player = GameObject.Instantiate(character);
                    var comp = player.AddComponent<DockingPlayAnimation>();
                    comp.Create(clip);

                    Animator animator = comp.GetComponent<Animator>();

                    BoneTransfromCurve motionQTCurve = new BoneTransfromCurve();

                    int frames = Mathf.FloorToInt(clip.length * 30);

                    for (int i = 0; i < frames; ++i)
                    {
                        float time = i / 30.0f;
                        comp.PlayAtTime(time);
                        EditorUtil.SetTransformCurve(time, motionQTCurve, comp.transform.position + initPos, comp.transform.rotation, null, animator.humanScale);
                    }
                    comp.PlayAtTime(clip.length);
                    EditorUtil.SetTransformCurve(clip.length, motionQTCurve, comp.transform.position + initPos, comp.transform.rotation, null, animator.humanScale);

                    // 清理playablegraph
                    comp.Destory();
                    GameObject.DestroyImmediate(player);

                    //处理曲线斜率轨迹
                    EditorUtil.SetCurveTangentMode(motionQTCurve);

                    // 保存docking bone 轨迹到文件
                    EditorUtil.SaveMotionToAnimationClip(clip, motionQTCurve);
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    

}
