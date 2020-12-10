using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Docking
{
    public abstract class Controller
    {
        protected Animator                          m_animator;
        protected bool                              m_enableInput;
        private Vector2                             m_preMoveInput;
        protected System.Func<Vector2, Vector2>     HandleInputFunc;
        
        // 设置是否容许进行控制
        public void SetEnableInput(bool active)
        {
            m_enableInput = active;
        }
        public bool GetEnableInput() { return m_enableInput; }

        protected virtual Vector2 GetInput()
        {
            if (false == m_enableInput) return Vector2.zero;

            Vector2 input = new Vector2();
            input.x = Input.GetAxis("Horizontal");
            input.y = Input.GetAxis("Vertical");

            if(null != HandleInputFunc)
            {
                input = HandleInputFunc(input);
            }

            input = Vector2.Lerp(m_preMoveInput, input, .2f);
            m_preMoveInput = input;

            return input;
        }

        public abstract void Tick(float deltaTime);
    }
}

