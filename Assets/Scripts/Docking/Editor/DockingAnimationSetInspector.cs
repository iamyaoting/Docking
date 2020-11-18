using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Docking
{
    class BoneTransfromCurve
    {
        public AnimationCurve posX = new AnimationCurve();
        public AnimationCurve posY = new AnimationCurve();
        public AnimationCurve posZ = new AnimationCurve();
        public AnimationCurve quatX = new AnimationCurve();
        public AnimationCurve quatY = new AnimationCurve();
        public AnimationCurve quatZ = new AnimationCurve();
        public AnimationCurve quatW = new AnimationCurve();

        public Quaternion LastQuaternion()
        {
            if (posX.length == 0) return new Quaternion(0, 0, 0, 1);
            System.Func<AnimationCurve, float> lastValue = curve =>
            {
                return curve.keys[curve.length - 1].value;
            };
            return new Quaternion(lastValue(quatX), lastValue(quatY), lastValue(quatZ), lastValue(quatW));            
        }

        public void Evaluate(float time, out Vector3 localPos, out Quaternion localQuat)
        {
            localPos.x = posX.Evaluate(time);
            localPos.y = posY.Evaluate(time);
            localPos.z = posZ.Evaluate(time);

            localQuat.x = quatX.Evaluate(time);
            localQuat.y = quatY.Evaluate(time);
            localQuat.z = quatZ.Evaluate(time);
            localQuat.w = quatW.Evaluate(time);
            localQuat.Normalize();
        }
    }


    [CustomEditor(typeof(DockingAnimationSet))]
    public class DockingAnimationSetInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var script = target as DockingAnimationSet;
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if(GUILayout.Button("simulate and save"))
            {
                foreach(var c in script.anmis)
                {
                    DoSimulateAnimation(script.character, c);
                }                
            }
            GUILayout.EndHorizontal();
        }

        private void DoSimulateAnimation(GameObject hostplayer, DockingAnimation da)
        {
            float intervalFrameTime = 1.0f / 30.0f;
            
            // 创建playable graph进行后处理
            var clip = da.clip;
            var player = GameObject.Instantiate(hostplayer);
            var comp = player.AddComponent<DockingPlayAnimation>();
            comp.Create(clip);

            BoneTransfromCurve dockingBoneTransCurve = new BoneTransfromCurve();
            Vector3 posWS = new Vector3();
            Quaternion quatWS = new Quaternion();
            Transform root = comp.transform;


            // 若只需要记录固定事件下的docking bone的信息，则提前播放到指定时刻，先进行记录
            if (da.dockingTimeType == DockingTimeType.DOCKING_FIXED_TIME)
            {
                comp.PlayAtTime(da.dockedFixedNormalizedTime * da.clip.length);
                GetDockingBoneWS(root, da, out posWS, out quatWS);
            }

            float time = 0;
            while (time < clip.length + intervalFrameTime)
            {
                time = Mathf.Clamp(time, 0, clip.length);
                comp.PlayAtTime(time);

                if (da.dockingTimeType == DockingTimeType.DOCKING_ALL_TIME)
                {
                    GetDockingBoneWS(root, da, out posWS, out quatWS);
                }
                SetTransformCurve(time, dockingBoneTransCurve, posWS, quatWS, comp.transform);

                if (time == clip.length) break;

                time += intervalFrameTime;
            }

            // 清理playablegraph
            comp.Destory();
            GameObject.DestroyImmediate(player);

            // 保存docking bone轨迹到文件
            SaveAnimationClip(da.clip, "DockingBone", dockingBoneTransCurve);           
        }

        private static void GetDockingBoneWS(Transform root, DockingAnimation da, out Vector3 posWS, out Quaternion quatWS)
        {
            Animator animator = root.gameObject.GetComponent<Animator>();
            if (animator == null) Debug.LogError("No animator");
            Avatar avatar = animator.avatar;
            if (avatar == null) Debug.LogError("No avatar");
            if (!avatar.isHuman) Debug.LogError("Not human");

            // 如果是双手中间的化，需要特殊处理
            if(da.isCenterofHands)
            {
                posWS = Vector3.zero;
                quatWS = Quaternion.identity;
                return;
            }

            // 其余情况就是单根骨骼
            // 若是根骨骼
            if(da.dockingBone == HumanBodyBones.LastBone)
            {
                posWS = root.position;
                quatWS = root.rotation;
                return;
            }

            // 其余直接记录
            posWS = animator.GetBoneTransform(da.dockingBone).position;
            quatWS = animator.GetBoneTransform(da.dockingBone).rotation;
            return;
        }

        private static void SaveAnimationClip(AnimationClip clip, string dockingBonePath, BoneTransfromCurve boneTransfromCurve)
        {
            // 记录docking bone 轨迹到 animationclip
            clip.SetCurve(dockingBonePath, typeof(Transform), "localPosition.x", boneTransfromCurve.posX);
            clip.SetCurve(dockingBonePath, typeof(Transform), "localPosition.y", boneTransfromCurve.posY);
            clip.SetCurve(dockingBonePath, typeof(Transform), "localPosition.z", boneTransfromCurve.posZ);

            clip.SetCurve(dockingBonePath, typeof(Transform), "localRotation.x", boneTransfromCurve.quatX);
            clip.SetCurve(dockingBonePath, typeof(Transform), "localRotation.y", boneTransfromCurve.quatY);
            clip.SetCurve(dockingBonePath, typeof(Transform), "localRotation.z", boneTransfromCurve.quatZ);
            clip.SetCurve(dockingBonePath, typeof(Transform), "localRotation.w", boneTransfromCurve.quatW);

            clip.EnsureQuaternionContinuity();

            // 保存文件
            EditorUtility.SetDirty(clip);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void SetTransformCurve(float time, BoneTransfromCurve transCurve, Vector3 posWS, Quaternion quatWS, Transform root)
        {
            Vector3 lcoalPos = root.InverseTransformPoint(posWS);
            Quaternion localQuat = Quaternion.Inverse(root.rotation) * quatWS;
            localQuat = Utils.EnsureQuaternionContinuity(transCurve.LastQuaternion(), localQuat);

            transCurve.posX.AddKey(time, lcoalPos.x);
            transCurve.posY.AddKey(time, lcoalPos.y);
            transCurve.posZ.AddKey(time, lcoalPos.z);          

            transCurve.quatX.AddKey(time, localQuat.x);
            transCurve.quatY.AddKey(time, localQuat.y);
            transCurve.quatZ.AddKey(time, localQuat.z);
            transCurve.quatW.AddKey(time, localQuat.w);
        }
    }

}
