using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Docking
{
    public class Utils
    {
        public static void GetBlend(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            var graph = animator.playableGraph;
            for(int i = 0; i < graph.GetOutputCountByType<AnimationPlayableOutput>(); ++i)
            {
                var output = (AnimationPlayableOutput)(animator.playableGraph.GetOutputByType<AnimationPlayableOutput>(i));
                var source = PlayableOutputExtensions.GetSourcePlayable(output);
                Debug.Log(source.GetPlayableType().Name);
            }
        }


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

