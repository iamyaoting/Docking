using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Docking
{
    public class EditorUtil
    {
        public static void SaveBoneTransToAnimationClip(AnimationClip clip, string dockingBonePath,
            BoneTransfromCurve boneTransfromCurve)
        {
            // 先删除原来的curve，因为直接setcurve会实施combine curve行为
            clip.SetCurve(dockingBonePath, typeof(Transform), "localPosition", null);
            clip.SetCurve(dockingBonePath, typeof(Transform), "localRotation", null);

            // 记录docking bone 轨迹到 animation clip
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

        public static void SaveMotionToAnimationClip(AnimationClip clip, BoneTransfromCurve motionQT)
        {
            // 先删除原来的curve，因为直接setcurve会实施combine curve行为
            clip.SetCurve("", typeof(Animator), "MotionT.x", null);
            clip.SetCurve("", typeof(Animator), "MotionT.y", null);
            clip.SetCurve("", typeof(Animator), "MotionT.z", null);

            clip.SetCurve("", typeof(Animator), "MotionQ.x", null);
            clip.SetCurve("", typeof(Animator), "MotionQ.y", null);
            clip.SetCurve("", typeof(Animator), "MotionQ.z", null);
            clip.SetCurve("", typeof(Animator), "MotionQ.w", null);

            // 记录docking bone 轨迹到 animation clip
            clip.SetCurve("", typeof(Animator), "MotionT.x", motionQT.posX);
            clip.SetCurve("", typeof(Animator), "MotionT.y", motionQT.posY);
            clip.SetCurve("", typeof(Animator), "MotionT.z", motionQT.posZ);

            clip.SetCurve("", typeof(Animator), "MotionQ.x", motionQT.quatX);
            clip.SetCurve("", typeof(Animator), "MotionQ.y", motionQT.quatY);
            clip.SetCurve("", typeof(Animator), "MotionQ.z", motionQT.quatZ);
            clip.SetCurve("", typeof(Animator), "MotionQ.w", motionQT.quatW);

            clip.EnsureQuaternionContinuity();

            // 保存文件
            EditorUtility.SetDirty(clip);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void SetCurveTangentMode(BoneTransfromCurve boneTransfromCurve)
        {
            AnimationCurve[] curves = new AnimationCurve[7] {
            boneTransfromCurve.posX,
            boneTransfromCurve.posY,
            boneTransfromCurve.posZ,
            boneTransfromCurve.quatX,
            boneTransfromCurve.quatY,
            boneTransfromCurve.quatZ,
            boneTransfromCurve.quatW
            };

            foreach (var curve in curves)
            {
                int count = curve.length;
                for (int i = 0; i < count; ++i)
                {
                    AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
                    AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
                }
            }
        }       

        public static void SetTransformCurve(float time, BoneTransfromCurve transCurve,
           Vector3 posWS, Quaternion quatWS, Transform root, float humanoidScale = 1.0f)
        {
            if (transCurve.posX.keys.Length > 0)  // 如果两帧靠的太近，选择覆盖
            {
                var len = transCurve.posX.keys.Length;
                float lastTime = transCurve.posX.keys[len - 1].time;
                if (time - lastTime < Utils.GetFloatZeroThreshold())
                {
                    Debug.LogWarning("Two Frame's time closely: " + time + "\\" + lastTime);

                    transCurve.posX.RemoveKey(len - 1);
                    transCurve.posY.RemoveKey(len - 1);
                    transCurve.posZ.RemoveKey(len - 1);
                    transCurve.quatX.RemoveKey(len - 1);
                    transCurve.quatY.RemoveKey(len - 1);
                    transCurve.quatZ.RemoveKey(len - 1);
                    transCurve.quatW.RemoveKey(len - 1);
                }
            }

            Vector3 lcoalPos = posWS;
            Quaternion localQuat = quatWS;
            if (null != root)  //如果是非motion数据，求解local Trans
            {
                lcoalPos = root.InverseTransformPoint(posWS);
                localQuat = Quaternion.Inverse(root.rotation) * quatWS;
            }   
            else
            {
                lcoalPos = lcoalPos / humanoidScale;
            }
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
