﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class ControllerManager : MonoBehaviour
{
    public Docking.DockingDetector                  m_dockingDetector;
    protected Controller                            m_lastController;
    protected Controller                            m_currentController;
    private   Docking.DockingDriver                 m_dockingDriver;    
    private   Animator                              m_animator;

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
                m_currentController.OnEnter(context);
                m_lastController.OnExit();
            }
            m_currentController.Tick(Time.deltaTime);           
        }
    }


    private void OnAnimatorMove()
    {
        transform.position += m_animator.deltaPosition;
        transform.rotation = m_animator.deltaRotation * transform.rotation;
        if (null != m_currentController)
        {
            m_currentController.OnAnimatorMove();            
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
