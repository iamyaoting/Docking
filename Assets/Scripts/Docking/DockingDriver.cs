using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Docking
{
    public class DockingControlData
    {
        public float m_dockingBlend         = 0;
        public float m_previousDockingBlend = 0;
        public float m_timeOffset           = 0;
        public HumanBodyBones m_dockingBone = HumanBodyBones.LastBone;
        //qstransform m_modelSpaceTarget;
    }

    public class DockingDriver : MonoBehaviour
    {


        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}

