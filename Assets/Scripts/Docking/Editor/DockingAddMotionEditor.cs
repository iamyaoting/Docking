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
                        EditorUtil.SetTransformCurve(time, motionQTCurve, comp.transform.position, comp.transform.rotation, null, animator.humanScale);
                    }
                    comp.PlayAtTime(clip.length);
                    EditorUtil.SetTransformCurve(clip.length, motionQTCurve, comp.transform.position, comp.transform.rotation, null, animator.humanScale);

                    // ����playablegraph
                    comp.Destory();
                    GameObject.DestroyImmediate(player);

                    //��������б�ʹ켣
                    EditorUtil.SetCurveTangentMode(motionQTCurve);

                    // ����docking bone �켣���ļ�
                    EditorUtil.SaveMotionToAnimationClip(clip, motionQTCurve);
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    

}
