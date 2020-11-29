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
        // 这个变量表征将来timeOffset时间的位置为docking Bone要靠在target位置上
        // 需要去采样clip获得骨骼模型空间的位置，unity不让这么干，提前存储到文件中
        // 叫docking Bone
        // public float m_timeOffset           = 0; 
        // public HumanBodyBones m_dockingBone = HumanBodyBones.LastBone;
        public DockingTransform m_targetOffsetMS;
    }

    [RequireComponent(typeof(Animator))]
    public class DockingDriver : MonoBehaviour
    {
        private Animator m_animator;
        private DockingControlData m_dockingControlData;
        private DockingTarget m_dockingTarget;

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
            
            if(null == transform.Find(Utils.GetDockingBoneName()))
            {
                GameObject dockingBone = new GameObject(Utils.GetDockingBoneName());
                dockingBone.transform.parent = transform;
                dockingBone.transform.localPosition = Vector3.zero;
                dockingBone.transform.localRotation = Quaternion.identity;
                dockingBone.transform.localScale = Vector3.one;
            }
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
                Debug.LogWarning("Docking Control data can not be none!");
                SetDefaultValue();
                return;
            }
            if (m_dockingControlData.m_dockingBlend < 0.0f || m_dockingControlData.m_dockingBlend > 1.0f)
            {
                Debug.LogError("Docking blend value error! --->" + m_dockingControlData.m_dockingBlend);
                return;
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
                if (null != m_dockingTarget) 
                { 
                    // calculate the target
                    //m_dockingTarget.

                    // blend
                    if(1.0 == m_dockingControlData.m_dockingBlend)
                    {
                        oldReferenceFromModel = DockingTransform.Multiply(
                            referenceFromDesiredTarget, DockingTransform.Inverse(modelFromTarget));
                    }
                    else
                    {
                        // blend and get error
                        DockingTransform error;
                        GetError(referenceFromDesiredTarget, oldReferenceFromTarget, 
                            m_dockingControlData.m_previousDockingBlend, 
                            m_dockingControlData.m_dockingBlend, out error);
                        oldReferenceFromModel.translation += error.translation;
                        oldReferenceFromModel.rotation = error.rotation * oldReferenceFromModel.rotation;
                    }
                }
                else
                {
                    referenceFromDesiredTarget = oldReferenceFromTarget;
                }

                // save the debug information
                m_worldFromLastTarget = DockingTransform.Multiply(m_worldFromNewReference, oldReferenceFromTarget);
                m_worldFromLastDesiredTarget = DockingTransform.Multiply(m_worldFromNewReference, referenceFromDesiredTarget);

                if (-1.0f == m_lastBlend)
                {
                    m_worldFromFirstTarget = m_worldFromLastTarget;                    
                }
                m_lastBlend = m_dockingControlData.m_dockingBlend;
                
                m_dockingTarget.selected = true;
            }
            else
            {
                SetDefaultValue();
            }

            worldFromModel = DockingTransform.Multiply(m_worldFromNewReference, oldReferenceFromModel);
            worldFromModel.ApplyDockingTransformWS(m_animator.transform);
            m_worldFromOldReference = m_worldFromNewReference;
        }

        public void Notify(DockingControlData data)
        {
            m_dockingControlData = data;
        }

        public void SetDockingTarget(DockingTarget target)
        {
            m_dockingTarget = target;
            SetWorldFromReference(target);
        }

        // 消除 Docking Target 非等比缩放，重构WorldFromReferenceTransform
        private void SetWorldFromReference(DockingTarget target)
        {
            m_worldFromOldReference.SetIdentity();
            m_worldFromOldReference.translation = target.transform.position;
            m_worldFromOldReference.rotation = target.transform.rotation;

            m_worldFromNewReference.SetIdentity();
            m_worldFromNewReference.translation = target.transform.position;
            m_worldFromNewReference.rotation = target.transform.rotation;
        }

        // 消除 Docking Target 非等比缩放，重构WorldFromReferenceTransform
        // 在Docking Target不是static时候，调用该函数
        private void UpdateWorldFromReference(DockingTarget target)
        {
            m_worldFromOldReference = m_worldFromNewReference;

            m_worldFromNewReference.SetIdentity();
            m_worldFromNewReference.translation = target.transform.position;
            m_worldFromNewReference.rotation = target.transform.rotation;
        }

        private void GetError(DockingTransform t1, DockingTransform t2,
           float lastBlend, float blend, out DockingTransform error)
        {
            error = new DockingTransform();

            var fraction = Utils.ComputeBlendFraction(BLEND_CURVE_TYPE.SMOOTH_TO_SMOOOTH, lastBlend, blend);
            if(fraction > 0.0f)
            {
                error.translation = t2.translation - t1.translation;
                error.rotation = t2.rotation * Quaternion.Inverse(t1.rotation);
                if(fraction < 1.0f)
                {
                    error.translation = Vector3.Lerp(Vector3.zero, error.translation, fraction);
                    error.rotation = Quaternion.Slerp(Quaternion.identity, error.rotation, fraction);
                }
                else
                {
                    // 设置全部偏移，上面已经设置了
                }
            }
            else
            {
                // 设置单位变化，开始的时候已经设置
            }
        }

        private void SetDefaultValue()
        {
            m_worldFromFirstTarget.SetIdentity();
            m_worldFromLastTarget.SetIdentity();
            m_worldFromLastDesiredTarget.SetIdentity();
            m_lastBlend = -1.0f;
        }
        
        // 获得角色Docking Bone的Transform
        private Transform GetWorldFromTargetTransform()
        {
            return m_animator.transform.Find(Utils.GetDockingBoneName());
        }

        private void OnDrawGizmos()       
        {
            DockingGizmos.PushGizmosData();            

            DockingGizmos.DrawCoordinateFrameWS(m_worldFromLastTarget);
            DockingGizmos.DrawCoordinateFrameWS(m_worldFromLastDesiredTarget);

            DockingGizmos.PopGizmosData();
        }
    }
}

