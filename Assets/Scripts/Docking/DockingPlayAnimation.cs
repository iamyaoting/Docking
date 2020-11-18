using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace Docking
{
    public class DockingPlayAnimation : MonoBehaviour
    {
        AnimationClip clip;        
        PlayableGraph playableGraph;
        AnimationClipPlayable playableClip;
        Animator animator;
        //float preTime = 0;

        public void Create(AnimationClip animclip)
        {
            clip = animclip;
            animator = GetComponent<Animator>();
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            

            playableGraph = PlayableGraph.Create();
            var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", GetComponent<Animator>());
            // Wrap the clip in a playable
            playableClip = AnimationClipPlayable.Create(playableGraph, clip);
            // Connect the Playable to an output
            playableOutput.SetSourcePlayable(playableClip);
            // Plays the Graph.
            playableGraph.Play();
            // Stops time from progressing automatically.
            playableClip.Pause();           
            
            //preTime = 0.0f;
            playableClip.SetTime(0.0f);
        }

        public void PlayAtTime(float time)
        {
            animator.enabled = true;

            playableClip.SetTime(time);
            playableGraph.Evaluate();
            //animator.Update(0);

            animator.enabled = false;
        }
        public void Destory()
        {
            // Destroys all Playables and Outputs created by the graph.
            if (playableGraph.IsValid())
            {
                playableGraph.Destroy();
            }
        }

        //public float time;
        //private void Start()
        //{
        //    Create(clip);
        //}
        //private void Update()
        //{
        //    Simulate(time);
        //}
    }
}

