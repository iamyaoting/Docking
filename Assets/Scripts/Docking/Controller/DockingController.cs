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
        protected DockingDetector   m_dockingDetector;        

        // docking target上点的预留信息
        public DockedVertexStatus  dockedVertexStatus { get; set; } 

        public void Init(DockingDetector detector)
        {
            m_dockingDetector = detector;
            m_animator = m_dockingDetector.hostPlayer.GetComponent<Animator>();
            HandleInputFunc = HandleInputLimit;
        }

        public abstract void OnEnter(TR desiredDockedVertex, DockedVertexStatus desiredDockedVertexStatus);

        protected override Vector2 GetInput()
        {
            var input = base.GetInput();            
            return input;
        }

        private Vector2 HandleInputLimit(Vector2 input)
        {
            var dockingInputLimit = dockedVertexStatus.limit;

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
    }
}

