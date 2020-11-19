using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Docking
{
    // Docking Generator 提供给上层的runtime状态数据
    public class DockingControlData
    {
        public float m_dockingBlend         = 0;
        public float m_previousDockingBlend = 0;
        public float m_timeOffset           = 0;
        public HumanBodyBones m_dockingBone = HumanBodyBones.LastBone;
        public DockingTransform m_targetOffsetMS;
    }

    [RequireComponent(typeof(Animator))]
    public class DockingDriver : MonoBehaviour
    {
        private Animator m_animator;
        private DockingControlData m_dockingControlData;

        private DockingTransform m_worldFromNewReference = new DockingTransform();
        private DockingTransform m_worldFromOldReference = new DockingTransform();
        private DockingTransform m_worldFromLastTarget = new DockingTransform();
        private DockingTransform m_worldFromLastDesiredTarget = new DockingTransform();

        float m_lastBlend;

        // first init information
        private DockingTransform m_worldFromFirstTarget = new DockingTransform();
        
        private void Start()
        {
            m_animator = GetComponent<Animator>();
            if (m_animator.avatar == null) Debug.LogError("No avatar in docking driver");
        }

        private void OnAnimatorMove()
        {
            DockingDrive();
        }

        private void DockingDrive()
        {            
            // 总体思想，先将更新后的model再old reference 空间中进行docking解算
            // old reference 更新到 new reference
            // 并带动model进行变换（考虑到载具，docking target 可能会移动）
            DockingTransform worldFromModel = new DockingTransform(m_animator.transform);
            DockingTransform oldReferenceFromModel = DockingTransform.Multiply(
                DockingTransform.Inverse(m_worldFromOldReference), worldFromModel);

            if (null == m_dockingControlData)
            {
                Debug.LogError("Docking Control data can not be none!");
                SetDefaultValue();
            }

            if (m_dockingControlData.m_dockingBlend > 0.0f)
            {
                DockingTransform oldReferenceFromTarget = new DockingTransform();
                DockingTransform worldFromTarget = new DockingTransform();
                DockingTransform modelFromTarget = new DockingTransform();
                DockingTransform worldFromDockingBone = new DockingTransform(GetWorldFromTargetTransform());
                DockingTransform modelFromDockingbone = new DockingTransform();

                modelFromDockingbone = DockingTransform.Multiply(
                    DockingTransform.Inverse(worldFromModel), worldFromDockingBone);
                modelFromTarget = DockingTransform.Multiply(
                    m_dockingControlData.m_targetOffsetMS, modelFromDockingbone);
                oldReferenceFromTarget = DockingTransform.Multiply(
                    oldReferenceFromModel, modelFromTarget);

                //get the desired target, i.e. reference from desired target
                DockingTransform referenceFromDesiredTarget = new DockingTransform();

                // get desired target from dockingTarget
                


                if(-1.0f == m_lastBlend)
                {
                    m_worldFromFirstTarget = m_worldFromLastTarget;
                }
                m_lastBlend = m_dockingControlData.m_dockingBlend;
            }
            else
            {
                SetDefaultValue();
            }

        }

        public void Notify(DockingControlData data)
        {
            m_dockingControlData = data;
        }

        private void SetDefaultValue()
        {
            m_worldFromFirstTarget.SetIdentity();
            m_worldFromLastTarget.SetIdentity();
            m_worldFromLastDesiredTarget.SetIdentity();
            m_lastBlend = -1.0f;
        }
        private Transform GetWorldFromTargetTransform()
        {
            return m_animator.transform.Find(DockingGenerator.GetDockingBoneName());
        }
    }
}

