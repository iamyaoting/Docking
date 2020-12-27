using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Docking
{
    [System.Flags]
    public enum DOCKING_INPUT_LIMIT
    {
        NONE                    = 0,
        HORIZEN_LEFT_FORBIDEN   = 1,
        HORIZEN_RIGHT_FORBIDEN  = 2,
        VERTICAL_DOWN_FORBIDEN  = 4,
        VERTICAL_UP_FORBIDEN    = 8,
        ALL                     = 15
    }

    public abstract class DockingController: Controller
    {        
        protected DockingDriver     m_dockingDriver;

        // docking target 上点的预留信息
        protected DockedVertexStatus m_dockedVertexStatus;
        
        public override void OnInit(ControllerInitContext context)
        {
            base.OnInit(context);
            m_dockingDriver = context.dockingDriver;
        }

        protected override Vector2 HandleInputLimit(Vector2 input)
        {
            var dockingInputLimit = m_dockedVertexStatus.limit;

            // 进行输入进行约束，若位于target边缘，则禁用某些input轴向输入        
            if (0 != (dockingInputLimit & DOCKING_INPUT_LIMIT.HORIZEN_LEFT_FORBIDEN))
            {
                if (input.x < 0) input.x = 0;
            }
            if (0 != (dockingInputLimit & DOCKING_INPUT_LIMIT.HORIZEN_RIGHT_FORBIDEN))
            {
                if (input.x > 0) input.x = 0;
            }
            if (0 != (dockingInputLimit & DOCKING_INPUT_LIMIT.VERTICAL_DOWN_FORBIDEN))
            {
                if (input.y < 0) input.y = 0;
            }
            if (0 != (dockingInputLimit & DOCKING_INPUT_LIMIT.VERTICAL_UP_FORBIDEN))
            {
                if (input.y > 0) input.y = 0;
            }         
            return input;
        }
        public override void OnEnter(ControllerEnterContext context)
        {
            // 向动画图执行命令
            m_animator.SetTrigger("Commit");
            m_dockingDriver.SetDockingTarget(context.dockingtarget);
            m_dockedVertexStatus = context.desiredDockedVertexStatus;
            base.OnEnter(context);
        }

        public override void OnExit()
        {
            // 当控制器离开，若dockingDriver关联的target为临时的，删除
            var target = m_dockingDriver.GetDockingTarget();
            if (target.temporary)
            {
                Object.Destroy(target, 2.0f);
            }
        }

        public override void OnAnimatorMove()
        {
            m_dockingDriver.Dock();
            m_dockedVertexStatus = m_dockingDriver.GetDockedVertexStatus() == null ? 
                m_dockedVertexStatus: m_dockingDriver.GetDockedVertexStatus();
        }
    }
}

