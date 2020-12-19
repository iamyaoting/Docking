using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BakeAnimation : StateMachineBehaviour
{
    public bool startBakeAtEnter = false;
    public bool stopBakeAtEnter = false;

    public bool startBakeAtExit = false; 
    public bool stopBakeAtExit = false;

    private Animator animator;
    private Docking.HumanoidBaker humanBaker;
    //OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator _animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator = _animator;
        humanBaker = animator.GetComponent<Docking.HumanoidBaker>();

        if (startBakeAtEnter) StartBake();
        if (stopBakeAtEnter) StopBake();
        Debug.Log(stateInfo.normalizedTime);
    }

    //OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{

    //}

    //OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(startBakeAtExit) StartBake();
        if (stopBakeAtExit) StopBake();
    }

    //OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    //OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}

    private void StartBake()
    {
        animator.transform.position = Vector3.zero;
        animator.transform.rotation = Quaternion.identity;
        humanBaker.StartBaking();
    }

    private void StopBake()
    {
        humanBaker.StopBaking();
    }
}
