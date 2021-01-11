using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ControllerEnterContext
{
    // 想要docked的目标点状态, 在ws中
    public Docking.DockedVertexStatus desiredDockedVertexStatus;

    // 想要docked的目标点
    public Docking.DockingVertex desiredDockedVertex;

    // docking target
    public Docking.DockingTarget dockingtarget;
}

public enum AnimatorState
{
    BLEND_IN,
    NOT_TRANSITION,
    BLEND_OUT   
}


public class ControllerInitContext
{
    public Animator                     animator;
    public Docking.DockingDetector      dockingDetector;
    public Docking.DockingDriver        dockingDriver;
}

public abstract class Controller
{
    protected Animator                          m_animator;
    protected Docking.DockingDetector           m_dockingDetector;

    protected bool                              m_enableInput;
    
    protected ControllerEnterContext            m_nextControllerEnterContext;
    protected System.Type                       m_nextControllerType;

    // 该控制器是否有效，在两个state之间的transistion状态下，不进行控制，该值为false
    public bool active { get; set; }   
  

    // 设置是否容许进行控制
    public void SetEnableInput(bool active)
    {
        m_enableInput = active;
    }
    public bool GetEnableInput() { return m_enableInput; }

    protected Vector2 GetRawInput()
    {
        if (false == m_enableInput) return Vector2.zero;

        Vector2 input = new Vector2();
        input.x = Input.GetAxis("Horizontal");
        input.y = Input.GetAxis("Vertical");

        input = HandleInputLimit(input);

        return input;
    } 

    // 用于处理input约束的情况
    protected virtual Vector2 HandleInputLimit(Vector2 input) { return input; }

    public virtual void OnInit(ControllerInitContext context)
    {
        m_animator = context.animator;
        m_dockingDetector = context.dockingDetector;
    }

    public virtual void OnEnter(ControllerEnterContext context)
    {
        // 设置下一个控制器及状态为null
        m_nextControllerEnterContext = null;
        m_nextControllerType = null;
        m_enableInput = true;
        active = false;
        Debug.Log("【Controller: " + GetType().Name + " 】entered!");        
    }

    public abstract void Tick(float deltaTime);
    public virtual void OnExit() 
    {
        active = false;
        Debug.Log("【Controller: " + GetType().Name + " 】exit!");
    }

    // 若rootmotion需要特殊处理，则进行override该函数，进行处理，特指docking
    public virtual void OnDockingDriver() { }    


    // 当绑定该控制器的状态结束时候，会调用该函数
    public virtual void OnFSMStateExit() { }   


    // 该函数处理是否进入下一个控制器的逻辑，返回false表示不进行切换
    public bool WhetherGoToNextController(out System.Type controllerType, out ControllerEnterContext context)
    {
        controllerType = m_nextControllerType;
        context = m_nextControllerEnterContext;
        if (m_nextControllerType == typeof(IdleController)) return true;
        if (null != m_nextControllerEnterContext && null != m_nextControllerType)
        {            
            return true;
        }
        return false;
    }

    // 当前帧是否有 env interactive Event 的用户输入请求，包括键盘，手柄等
    public static bool HasEnvCommitAction()
    {
        //return true;
        if (Input.GetKeyDown(KeyCode.E)) return true;
        return false;
    }

    // 当前是否进行跳下操作请求，进入正常locomotion控制器
    public static bool HasEnvUnCommitAction()
    {
        if (Input.GetKeyDown(KeyCode.B)) return true;
        else return false;
    }

    // 是否按了加速键
    protected bool IsSpeedUpActionPressed()
    {
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            return true;
        else
            return false;
    }

    //  找地面的最近target，由于地面没有target，则使用物理碰撞
    protected ControllerEnterContext CreateFloorVertexTarget(Vector3 pointWSHint, Quaternion rotWS)
    {
        ControllerEnterContext context = new ControllerEnterContext();

        const float maxDist = 100;
        var origin = pointWSHint + Vector3.up * 2;
        RaycastHit hit;
        if(false == Physics.Raycast(origin, -Vector3.up, out hit, maxDist))
        {
            Debug.LogError("探测不到地面,探测距离： " + maxDist + " 探测位置：" + origin);
            return null;
        }

        //var rot = Quaternion.FromToRotation(rotWS * Vector3.up, hit.normal) * rotWS;
        // 人应该始终朝向up方向，狗和蜘蛛另外再说
        var rot = Quaternion.FromToRotation(rotWS * Vector3.up, Vector3.up) * rotWS;
        context.desiredDockedVertex = new Docking.DockingVertex(hit.point, rot, 0);
        context.desiredDockedVertexStatus = new Docking.DockedVertexStatus();

        // 添加DockingVertexTarget临时脚本到碰撞gameobject上
        var target = hit.collider.gameObject.AddComponent<Docking.DockingVertexTarget>();
        target.temporary = true;

        // 计算其局部坐标
        target.m_desiredVertex = new Docking.DockingVertex(
            target.transform.InverseTransformPoint(context.desiredDockedVertex.tr.translation),
            Quaternion.Inverse(target.transform.rotation) * context.desiredDockedVertex.tr.rotation, 
            context.desiredDockedVertex.reserveFloatParam);

        context.dockingtarget = target;

        return context;
    }

}

