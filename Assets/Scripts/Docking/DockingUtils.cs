using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Docking
{
    public enum BLEND_CURVE_TYPE
    {
        SMOOTH_TO_SMOOOTH,
        LINEAR_TO_SMOOTH,
        SMOOTH_TO_LINEAR
    }
    public class DockingTransform
    {
        public Vector3 translation;
        public Vector3 scale;
        public Quaternion rotation;

        public DockingTransform()
        {
            SetIdentity();
        }

        public DockingTransform(DockingTransform other)
        {
            translation = other.translation;
            scale = other.scale;
            rotation = other.rotation;
        }

        public DockingTransform(Transform worldFromThisNoS)
        {
            translation = worldFromThisNoS.position;
            //scale = worldFromThisNoS.lossyScale;
            scale = Vector3.one;
            rotation = worldFromThisNoS.rotation;
        }

        public void ApplyDockingTransformWS(Transform trans)
        {
            var parent = trans.parent;
            trans.parent = null;            
            trans.position = translation;
            trans.rotation = rotation;
            trans.localScale = Vector3.one;
            trans.parent = parent;
            if(!IdentityScale(this))
            {
                Debug.LogError("ApplyDockingTransformWS Scale not identity!");
            }
        }

        public void SetIdentity()
        {
            translation = Vector3.zero;
            rotation = Quaternion.identity;
            scale = Vector3.one;
        }

        public void SetInverse()
        {
            scale = Reciprocal(scale);

            rotation = Quaternion.Inverse(rotation);
            translation = Vector3.Scale(translation, -scale);
            translation = rotation * translation;
        }

        /// <summary>
        /// cTa = bTa * cTb
        /// </summary>
        /// <param name="bTa">transform of space b to space a</param>
        /// <param name="cTb">transform of space c to space b</param>
        /// <returns>cTa, transform of space c to space a</returns>
        public static DockingTransform Multiply(DockingTransform bTa, DockingTransform cTb)
        {
            DockingTransform result = new DockingTransform();
            result.scale = Vector3.Scale(bTa.scale, cTb.scale);
            result.rotation = bTa.rotation * cTb.rotation;
            result.translation = bTa.rotation * Vector3.Scale(bTa.scale, cTb.translation) + bTa.translation;
            return result;
        }
        public static DockingTransform Inverse(DockingTransform t)
        {
            DockingTransform result = new DockingTransform(t);
            result.SetInverse();
            return result;
        }      
        
        public static Vector3 Reciprocal(Vector3 v)
        {
            return new Vector3(1.0f / v.x, 1.0f / v.y, 1.0f / v.z);            
        }

        public static bool IdentityScale(DockingTransform t)
        {
            if((t.scale - Vector3.one).sqrMagnitude > 0.0001)
            {
                return false;
            }
            return true;
        }
    }

    public class Utils
    {
        // Realigns quaternion keys to ensure shortest interpolation paths.
        public static Quaternion EnsureQuaternionContinuity(Quaternion lastQ, Quaternion q)
        {
            Quaternion flipped = new Quaternion(-q.x, -q.y, -q.z, -q.w);

            Quaternion midQ = new Quaternion(
                Mathf.Lerp(lastQ.x, q.x, 0.5f),
                Mathf.Lerp(lastQ.y, q.y, 0.5f),
                Mathf.Lerp(lastQ.z, q.z, 0.5f),
                Mathf.Lerp(lastQ.w, q.w, 0.5f)
                );

            Quaternion midQFlipped = new Quaternion(
                Mathf.Lerp(lastQ.x, flipped.x, 0.5f),
                Mathf.Lerp(lastQ.y, flipped.y, 0.5f),
                Mathf.Lerp(lastQ.z, flipped.z, 0.5f),
                Mathf.Lerp(lastQ.w, flipped.w, 0.5f)
                );

            float angle = Quaternion.Angle(lastQ, midQ);
            float angleFlipped = Quaternion.Angle(lastQ, midQFlipped);

            return angleFlipped < angle ? flipped : q;
        }

        // 用于计算target点在空间trans下的Yaw的角度
        public static float GetYawAngle(Transform trans, Vector3 targetWS)
        {
            var targetLS = trans.InverseTransformPoint(targetWS);
            targetLS.y = 0;
            return Vector3.SignedAngle(Vector3.forward, targetLS, Vector3.up);
        }

        public static DockingVertex DockingTransformToDockingVertex(DockingTransform trans)
        {
            DockingVertex vertex = new DockingVertex();
            vertex.tr = new TR();
            vertex.tr.translation = trans.translation;
            vertex.tr.rotation = trans.rotation;
            vertex.reserveFloatParam = 0.0f;
            return vertex;
        }

        // 用于实时按照预先定义好的曲线进行混合权重计算
        public static float ComputeBlendFraction(BLEND_CURVE_TYPE type, float lastBlend, float curBlend)
        {
            float lastV = EvaluateBlendCurve(type, lastBlend);
            float curV = EvaluateBlendCurve(type, curBlend);

            float oneMinusBlend = 1.0f - lastV;
            float fraction = 0.0f;
            if (oneMinusBlend > 0)
            {
                fraction = (curV - lastV) / oneMinusBlend;
            }
            else
            {
                fraction = 1.0f;
            }
            return fraction;
        }
        public static float EvaluateBlendCurve(BLEND_CURVE_TYPE type, float t)
        {
            float value = 0;
            t = Mathf.Clamp01(t);
            switch(type)
            {
                case BLEND_CURVE_TYPE.LINEAR_TO_SMOOTH:
                    value = -Mathf.Pow(t, 3) + Mathf.Pow(t, 2) + t;
                    break;
                case BLEND_CURVE_TYPE.SMOOTH_TO_LINEAR:
                    value = -Mathf.Pow(t, 3) + 2 * Mathf.Pow(t, 2);
                    break;
                case BLEND_CURVE_TYPE.SMOOTH_TO_SMOOOTH:
                    value = -2 * Mathf.Pow(t, 3) + 3 * Mathf.Pow(t, 2);
                    break;
            }
            return Mathf.Clamp01(value);
        }

        public static Transform GetDockingBoneTransform(Animator animator)
        {
            return animator.transform.Find(Utils.GetDockingBoneName());
        }

        public static string GetDockingBoneName()
        {
            return "DockingBone";
        }

        public static void GetLineSegmentDockedPoint(Vector3 start, Vector3 end, Vector3 unDockedPoint,
            out Vector3 dockedPoint, out float alpha)
        {
            var posMS = unDockedPoint;
            var point_start = posMS - start;
            var end_start = end - start;

            var k = Vector3.Dot(end_start, point_start) / end_start.sqrMagnitude;
            k = Mathf.Clamp01(k);

            dockedPoint = Vector3.Lerp(start, end, k);
            alpha = k;            
        }

        public static int[] GetStateMachineStateHash(string[] names)
        {
            int[] hashes = new int[names.Length];
            for(int i = 0; i < names.Length; ++i)
            {
                hashes[i] = Animator.StringToHash("Base Layer." + names[i]);
            }
            return hashes;
        }

        public static bool IsCurrentState(Animator animator, int[] hashes, int layer = 0)
        {
            var curHash = animator.GetCurrentAnimatorStateInfo(layer).fullPathHash;
            foreach (var hash in hashes)
            {
                if (curHash == hash) return true;
            }
            return false;
        }

        public static float GetCurrentStateNormalizedTime(Animator animator, int layer = 0)
        {
            var normalizedTime = animator.GetCurrentAnimatorStateInfo(layer).normalizedTime;
            normalizedTime = normalizedTime - Mathf.Floor(normalizedTime);
            return normalizedTime;
        }
    }

}

