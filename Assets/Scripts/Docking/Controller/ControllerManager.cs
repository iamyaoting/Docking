using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ControllerManager : MonoBehaviour
{
    public Docking.DockingDetector                  m_dockingDetector;
    public bool                                     m_enableDocking = true;

    protected Controller                            m_lastController;
    protected Controller                            m_currentController;
    private   Docking.DockingDriver                 m_dockingDriver;    
    private   Animator                              m_animator;
    private int                                     m_currentStateHash;
    

    // 用于存储涉及到的controller，防止多次 new
    private Dictionary<System.Type, Controller>     m_controllersCache;    

    public Docking.DockingController GetCurrentDockingController()
    {
        if (m_currentController is Docking.DockingController)
            return m_currentController as Docking.DockingController;
        else
            return null;
    }
    public Docking.DockingDriver GetDockingDriver() { return m_dockingDriver; }

    // Start is called before the first frame update
    void Start()
    {
        m_animator = GetComponent<Animator>();
        m_dockingDriver = new Docking.DockingDriver();
        m_dockingDriver.Init(m_animator);
        m_controllersCache = new Dictionary<System.Type, Controller>();
        m_currentController = new IdleController();
        m_currentController.OnInit(GetControllerInitContext());
        m_currentStateHash = m_animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
    }

    // Update is called once per frame
    void Update()
    {
        if (null != m_currentController)
        {
            System.Type conTrollertype = null;
            ControllerEnterContext context = null;
            if (m_currentController.WhetherGoToNextController(out conTrollertype, out context))
            {
                m_lastController = m_currentController;
                if (m_controllersCache.ContainsKey(conTrollertype))
                {
                    m_currentController = m_controllersCache[conTrollertype];
                }
                else
                {
                    m_currentController = ControllerFactory.CreateControolerByType(conTrollertype);
                    m_currentController.OnInit(GetControllerInitContext());
                }
                m_lastController.OnExit();
                m_currentController.OnEnter(context);                
            }
            UpdateControllerActiveState();
            if (m_currentController.active)
            {
                // 如果控制器属于激活状态，进行tick
                m_currentController.Tick(Time.deltaTime);
            }
        }
    }

    // 更新控制器active状态
    private void UpdateControllerActiveState()
    {
        if (m_currentController.active)
        {
            m_currentStateHash = m_animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
            return;
        }

        if (m_animator.GetCurrentAnimatorStateInfo(0).fullPathHash != m_currentStateHash)
        {
            // 此时动画graph的state已经改变了，激活当前的controller
            m_currentStateHash = m_animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
            m_currentController.active = true;
        }
    }

    private void LateUpdate()
    {
        //transform.position += m_animator.deltaPosition;
        //transform.rotation = m_animator.deltaRotation * transform.rotation;
        if (null != m_currentController && m_enableDocking && m_currentController.active)
        {
            m_currentController.OnDockingDriver();
        }
    }
    private ControllerInitContext GetControllerInitContext()
    {
        ControllerInitContext context = new ControllerInitContext();
        context.animator = m_animator;
        context.dockingDetector = m_dockingDetector;
        context.dockingDriver = m_dockingDriver;        
        return context;
    }


    #region gizmos
    [Header("Gizmos")]
    public bool m_dockingDriverGizmos;
    public bool m_dockingDetectorGizmos;
   
    private void OnDrawGizmos()
    {
        if (null != m_dockingDriver && true == m_dockingDriverGizmos)
        {
            m_dockingDriver.DrawGizmos();
        }

        if(null != m_dockingDetector && true == m_dockingDetectorGizmos)
        {
            m_dockingDetector.DrawGizmos();
        }
    }
    #endregion
}
