using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Docking
{
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

        public void SetIdentity()
        {
            translation = Vector3.zero;
            rotation = Quaternion.identity;
            scale = Vector3.one;
        }

        public void SetInverse()
        {            
            scale.x = 1.0f / scale.x;
            scale.y = 1.0f / scale.y;
            scale.z = 1.0f / scale.z;

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
    }
}

