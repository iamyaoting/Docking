using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Docking;


public class ControllerEnterContext
{
    // ��Ҫdocked��Ŀ���״̬, ��ws��
    public Docking.DockedVertexStatus desiredDockedVertexStatus;

    // ��Ҫdocked��Ŀ���
    public Docking.DockingVertex desiredDockedVertex;

    // docking target
    public Docking.DockingTarget dockingtarget;
}

public abstract class StateBehavioConBase : StateMachineBehaviour
{
    protected Animator m_animator;
    protected Docking.DockingDetector m_dockingDetector;
    protected Docking.DockingDriver m_dockingDriver;

   

    protected virtual void OnControllerUpdate(int layerIndex, AnimatorStateInfo stateInfo) { }
    protected virtual void OnControllerEnter(int layerIndex, AnimatorStateInfo stateInfo) { }
    protected virtual void OnControllerExit(int layerIndex, AnimatorStateInfo stateInfo) { }
    protected virtual void OnDockingTargetUpdate(DockingTarget target, TR tr, DockedVertexStatus status)
    {}


    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        m_animator = animator;
        m_dockingDetector = animator.GetComponent<Docking.DockingDetector>();
        m_dockingDriver = animator.GetComponent<Docking.DockingDriver>();
        OnControllerEnter(layerIndex, stateInfo);
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //ResetAnimatorTriggers();        
        {
            OnControllerUpdate(layerIndex, stateInfo);
            if(m_dockingDriver.valid)
            {
                OnDockingTargetUpdate(m_dockingDriver.GetDockingTarget(), m_dockingDriver.GetDockedVertexWS(), m_dockingDriver.GetDockedVertexStatus());
            }
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {        
        OnControllerExit(layerIndex, stateInfo);
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}

    // ��ǰ֡�Ƿ��� env interactive Event ���û��������󣬰������̣��ֱ���
    public static bool HasEnvCommitAction()
    {
        //return true;
        if (Input.GetKeyDown(KeyCode.E)) return true;
        return false;
    }

    // ��ǰ�Ƿ�������²������󣬽�������locomotion������
    public static bool HasEnvUnCommitAction()
    {
        if (Input.GetKeyDown(KeyCode.B)) return true;
        else return false;
    }

    private void ResetAnimatorTriggers()
    {
        foreach (var par in m_animator.parameters)
        {
            if (par.type == AnimatorControllerParameterType.Trigger)
            {
                m_animator.ResetTrigger(par.nameHash);
            }
        }
    }
    protected void SetDockingCommit()
    {
        m_animator.SetTrigger("Commit");
    }
    protected void SetUnDocking()
    {
        m_animator.SetTrigger("UnCommit");
    }

    protected Vector2 GetRawInput()
    {
        Vector2 input = new Vector2();
        input.x = Input.GetAxis("Horizontal");
        input.y = Input.GetAxis("Vertical");
        return input;
    }

    // ���ڴ���inputԼ�������
    protected Vector2 HandleInputLimit(Vector2 input, DOCKED_POINT_MOVE_LIMIT dockingInputLimit)
    {
        // �����������Լ������λ��target��Ե�������ĳЩinput��������        
        if (0 != (dockingInputLimit & DOCKED_POINT_MOVE_LIMIT.HORIZEN_LEFT_FORBIDEN))
        {
            if (input.x < 0) input.x = 0;
        }
        if (0 != (dockingInputLimit & DOCKED_POINT_MOVE_LIMIT.HORIZEN_RIGHT_FORBIDEN))
        {
            if (input.x > 0) input.x = 0;
        }
        if (0 != (dockingInputLimit & DOCKED_POINT_MOVE_LIMIT.VERTICAL_DOWN_FORBIDEN))
        {
            if (input.y < 0) input.y = 0;
        }
        if (0 != (dockingInputLimit & DOCKED_POINT_MOVE_LIMIT.VERTICAL_UP_FORBIDEN))
        {
            if (input.y > 0) input.y = 0;
        }
        return input;
    }
    //  �ҵ�������target�����ڵ���û��target����ʹ��������ײ
    protected ControllerEnterContext CreateFloorVertexTarget(Vector3 pointWSHint, Quaternion rotWS)
    {
        ControllerEnterContext context = new ControllerEnterContext();

        const float maxDist = 100;
        var origin = pointWSHint + Vector3.up * 2;
        RaycastHit hit;
        if (false == Physics.Raycast(origin, -Vector3.up, out hit, maxDist))
        {
            Debug.LogError("̽�ⲻ������,̽����룺 " + maxDist + " ̽��λ�ã�" + origin);
            return null;
        }

        //var rot = Quaternion.FromToRotation(rotWS * Vector3.up, hit.normal) * rotWS;
        // ��Ӧ��ʼ�ճ���up���򣬹���֩��������˵
        var rot = Quaternion.FromToRotation(rotWS * Vector3.up, Vector3.up) * rotWS;
        context.desiredDockedVertex = new Docking.DockingVertex(hit.point, rot, 0);
        context.desiredDockedVertexStatus = new Docking.DockedVertexStatus();

        // ���DockingVertexTarget��ʱ�ű�����ײgameobject��
        var target = hit.collider.gameObject.AddComponent<Docking.DockingVertexTarget>();
        target.temporary = true;

        // ������ֲ�����
        target.m_desiredVertex = new Docking.DockingVertex(
            target.transform.InverseTransformPoint(context.desiredDockedVertex.tr.translation),
            Quaternion.Inverse(target.transform.rotation) * context.desiredDockedVertex.tr.rotation,
            context.desiredDockedVertex.reserveFloatParam);
        target.m_constrainRotation = true;

        context.dockingtarget = target;

        return context;
    }

}
