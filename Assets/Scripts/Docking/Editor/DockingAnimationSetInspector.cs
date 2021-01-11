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

        private void DoSimulateAnimationSegment(GameObject hostplayer, DockingAnimSegment da, 
            BoneTransfromCurve dockingBoneTransCurve, DockingPlayAnimation comp, AnimationClip clip)
        {
            float startTime = da.startNormalizedTime * clip.length;
            float endTime = da.endNormalizedTime * clip.length;
            //float timeSegLen = endTime - startTime;
            float intervalFrameTime = 1.0f / 30.0f;
            Vector3 posWS = new Vector3();
            Quaternion quatWS = new Quaternion();
            Transform root = comp.transform;
            Vector3 ledgeStart = Vector3.zero, ledgeEnd = Vector3.zero;

            // 若只需要记录固定事件下的docking bone 的信息，则提前播放到指定时刻，先进行记录
            if (da.dockingTimeType == DockingTimeType.DOCKING_FIXED_TIME)
            {
                comp.PlayAtTime(da.dockedFixedNormalizedTime * clip.length);
                GetDockingBoneWS(root, da, out posWS, out quatWS);               
            }
            else if(da.dockingTimeType == DockingTimeType.DOCKED)
            {
                // 若双手沿着Ledge边缘移动，且默认Ledge为一条直线
                if(da.isCenterofHands) 
                {
                    // Ledge 开始位置
                    comp.PlayAtTime(da.startNormalizedTime * clip.length);
                    GetDockingBoneWS(root, da, out posWS, out quatWS);
                    ledgeStart = posWS;

                    // Ledge 结束位置
                    comp.PlayAtTime(da.endNormalizedTime * clip.length);
                    GetDockingBoneWS(root, da, out posWS, out quatWS);
                    ledgeEnd = posWS;                    
                }
            }

            float time = startTime;
            while (time < endTime + intervalFrameTime)
            {
                time = Mathf.Clamp(time, 0, endTime);
                comp.PlayAtTime(time);
                if (da.dockingTimeType == DockingTimeType.DOCKED)
                {
                    GetDockingBoneWS(root, da, out posWS, out quatWS);                    
                    if(da.isCenterofHands) // 将当前的点投影到ledgeStart---ledgeEnd直线上
                    {
                        posWS = ProjectLineSegment(posWS, ledgeStart, ledgeEnd);
                    }
                }
                SetTransformCurve(time, dockingBoneTransCurve, posWS, quatWS, comp.transform);
                if (time == endTime) break;
                time += intervalFrameTime;
            }
        }

        private static Vector3 ProjectLineSegment(Vector3 point, Vector3 start, Vector3 end)
        {
            var dir = end - start;
            if(dir.magnitude == 0.0f)
            {
                return start;
            }
            dir.Normalize();

            Vector3 dockedPoint;
            float alpha = 0;
            Utils.GetLineSegmentDockedPoint(start, end, point, out dockedPoint, out alpha);
            return dockedPoint;
        }

        private void DoSimulateAnimation(GameObject hostplayer, DockingAnimation da)
        {           
            // 创建playable graph 进行后处理
            var clip = da.clip;
            var player = GameObject.Instantiate(hostplayer);
            var comp = player.AddComponent<DockingPlayAnimation>();
            comp.Create(clip);

            BoneTransfromCurve dockingBoneTransCurve = new BoneTransfromCurve();
            
            for(int i = 0; i < da.segments.Length; ++i)
            {
                DoSimulateAnimationSegment(hostplayer, da.segments[i], dockingBoneTransCurve, comp, da.clip);
            }

            // 清理playablegraph
            comp.Destory();
            GameObject.DestroyImmediate(player);

            //处理曲线斜率轨迹
            SetCurveTangentMode(dockingBoneTransCurve);

            // 保存docking bone 轨迹到文件
            SaveAnimationClip(da.clip, Utils.GetDockingBoneName(), dockingBoneTransCurve);           
        }

        private static void SetCurveTangentMode(BoneTransfromCurve boneTransfromCurve)
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

            foreach(var curve in curves)
            {
                int count = curve.length;
                for(int i = 0; i < count; ++i)
                {
                    AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
                    AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.ClampedAuto);
                }
            }
        }

        private static void GetDockingBoneWS(Transform root, DockingAnimSegment da, 
            out Vector3 posWS, out Quaternion quatWS)
        {
            Animator animator = root.gameObject.GetComponent<Animator>();
            if (animator == null) Debug.LogError("No animator");
            Avatar avatar = animator.avatar;
            if (avatar == null) Debug.LogError("No avatar");
            if (!avatar.isHuman) Debug.LogError("Not human");

            // 如果是双手中间的化，需要特殊处理
            if(da.isCenterofHands)
            {
                var leftHandPosWS = animator.GetBoneTransform(HumanBodyBones.LeftHand).position;
                var rightHandPosWS = animator.GetBoneTransform(HumanBodyBones.RightHand).position;
                var point = (leftHandPosWS + rightHandPosWS) / 2.0f;
                var dir = point - root.position;
                var projdir = Vector3.ProjectOnPlane(dir, root.right);
                posWS = root.position + projdir;                
                quatWS = root.rotation;
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
            //quatWS = animator.GetBoneTransform(da.dockingBone).rotation;
            quatWS = root.rotation;
            return;
        }

        private static void SaveAnimationClip(AnimationClip clip, string dockingBonePath, 
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

        private static void SetTransformCurve(float time, BoneTransfromCurve transCurve, 
            Vector3 posWS, Quaternion quatWS, Transform root)
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
