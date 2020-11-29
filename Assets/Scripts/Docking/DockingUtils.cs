﻿using System.Collections;
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

        public DockingTransform(Transform worldFromThis)
        {
            translation = worldFromThis.position;
            scale = worldFromThis.lossyScale;
            rotation = worldFromThis.rotation;
        }

        public void ApplyDockingTransformWS(Transform trans)
        {
            var parent = trans.parent;
            trans.parent = null;
            
            trans.position = translation;
            trans.rotation = rotation;
            trans.localScale = scale;

            trans.parent = parent;
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
    }

    public class Utils
    {
        //public static void GetBlend(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    var graph = animator.playableGraph;
        //    for(int i = 0; i < graph.GetOutputCountByType<AnimationPlayableOutput>(); ++i)
        //    {
        //        var output = (AnimationPlayableOutput)(animator.playableGraph.GetOutputByType<AnimationPlayableOutput>(i));
        //        var source = PlayableOutputExtensions.GetSourcePlayable(output);
        //        Debug.Log(source.GetPlayableType().Name);
        //    }
        //}

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

        public static string GetDockingBoneName()
        {
            return "DockingBone";
        }
    }

}

