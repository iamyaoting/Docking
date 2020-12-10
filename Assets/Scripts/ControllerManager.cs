using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ControllerManager : MonoBehaviour
{
    protected Docking.DockingController     m_preDockingController;
    protected Docking.DockingController     m_dockingController;
    protected Docking.DockingDriver         m_dockingDriver;
    public    Docking.DockingDetector       m_dockingDetector;
    private Animator                        m_animator;

    #region gizmos
    [Header("Gizmos")]
    public bool m_dockingDriverGizmos;
    #endregion

    public Docking.DockingController GetCurrentDockingController(){ return m_dockingController;}
    public Docking.DockingDriver GetDockingDriver() { return m_dockingDriver; }

    // Start is called before the first frame update
    void Start()
    {
        m_animator = GetComponent<Animator>();
        m_dockingDriver = new Docking.DockingDriver();
        m_dockingDriver.Init(m_animator);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyUp(KeyCode.E))
        {
            Comit();
        }

        if (null != m_dockingController)
        {
            m_dockingController.Tick(Time.deltaTime);
        }
    }

    private void OnAnimatorMove()
    {
        transform.position += m_animator.deltaPosition;
        transform.rotation = m_animator.deltaRotation * transform.rotation;
        if (null != m_dockingController && m_dockingDriver.isActive)
        {
            m_dockingDriver.Dock();
            m_dockingController.dockedVertexStatus = m_dockingDriver.GetDockedVertexStatus();
        }
    }

    // 开始 Act 攀爬
    private void Comit()
    {
        // 寻找最近的target
        Docking.DockingTarget target = null;
        Docking.DockedVertexStatus desiredDockedVertexStatus = null;
        Docking.TR desiredDockedVertex = null;
        if(false == m_dockingDetector.DetectNearestTarget(
            out target, out desiredDockedVertex, out desiredDockedVertexStatus))
        {
            Debug.Log("Can not find docking target, climbing forbiden!");
            return;
        }
        else
        {
            Debug.Log("Find docking target: " + target.m_type.ToString());
        }

        // 向动画图执行命令
        m_animator.SetTrigger("Commit");

        // dockingDriver 设置 target
        m_dockingDriver.SetDockingTarget(target);

        // 更改当前的控制器
        m_preDockingController = m_dockingController;
        m_dockingController = Docking.DockingControllerFactory.CreateDockingControler(target);
        m_dockingController.Init(m_dockingDetector);

        // 调用 Docking Controller Enter回调函数
        m_dockingController.OnEnter(desiredDockedVertex, desiredDockedVertexStatus);
    }

    private void OnDrawGizmos()
    {
        if(null != m_dockingDriver && true == m_dockingDriverGizmos)
        {
            m_dockingDriver.OnDrawGizmos();
        }
    }
       
}
