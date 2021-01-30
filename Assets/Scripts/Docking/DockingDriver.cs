﻿using System.Collections;
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
    public class DockingDriver : MonoBehaviour
    {
        public bool m_adjustPlayBackSpeed = false;

        private Animator m_animator;
        private FullBodyIKModifier m_fullBodyIK;

        private DockingTransform m_worldFromNewReference = new DockingTransform();
        private DockingTransform m_worldFromOldReference = new DockingTransform();
        private DockingTransform m_worldFromLastTarget = new DockingTransform();
        private DockingTransform m_worldFromLastDesiredTarget = new DockingTransform();

        float m_lastBlend;

        // first init information
        private DockingTransform m_worldFromFirstTarget = new DockingTransform();

        // DockingDriver 修正所需要的参数数据，由Docking Generator 上传提供
        private DockingControlData m_dockingControlData;

        // DockingDriver 所需要的target标记线，由Docking Controller 设置提供        
        private DockingTarget m_dockingTarget;

        // DockingDriver 所需要的target标记线
        private DockingTarget m_dockingNextTarget;

        // Docking 的顶点信息
        private DockedVertexStatus m_dockedVertexStatus;

        // docking Bone 的transform
        private Transform m_dockingBone;

        // animation playback speed
        private float m_desiredPlaybackSpeed = 1.0f;

        // 数据是否有效
        public bool valid { get; private set; } = false;

        private void Start()
        {
            Init(GetComponent<Animator>());

        }
        public void Init(Animator animator)
        {
            m_animator = animator;
            if (m_animator.avatar == null)
            {
                Debug.LogError("No avatar in docking driver");
            }

            m_dockingBone = Utils.GetDockingBoneTransform(m_animator);

            if (null == m_dockingBone)
            {
                Debug.LogError("No Docking Bone, Please add in advance!");
            }
            m_fullBodyIK = new FullBodyIKModifier(m_animator.GetComponent<RootMotion.FinalIK.BipedIK>());
        }

        private void LateUpdate()
        {
            valid = DockDriver();
        }

        public DockedVertexStatus GetDockedVertexStatus() { return m_dockedVertexStatus; }
        public TR GetDockedVertexWS() 
        {
            var tr = new TR();
            tr.translation = m_worldFromLastDesiredTarget.translation;
            tr.rotation = m_worldFromLastDesiredTarget.rotation;
            return tr;
        }

        public bool DockDriver()
        {          
            m_animator.speed = Mathf.Lerp(m_animator.speed, m_desiredPlaybackSpeed, 1);            
            m_desiredPlaybackSpeed = 1.0f;   // 首先设置默认值

            //Debug.Log("Dock");
            UpdateWorldFromReference(m_dockingTarget);
            // 更新ik控制器
            m_fullBodyIK.OnIKUpdate(Time.deltaTime);

            // 总体思想，先将更新后的model再old reference 空间中进行docking解算
            // old reference 更新到 new reference
            // 并带动model进行变换（考虑到载具，docking target 可能会移动）
            DockingTransform worldFromModel = new DockingTransform(m_animator.transform);
            DockingTransform oldReferenceFromModel = DockingTransform.Multiply(
                DockingTransform.Inverse(m_worldFromOldReference), worldFromModel);

            if (null == m_dockingControlData)
            {
                //Debug.LogWarning("Docking Control data can not be none!");
                SetDefaultValue();
                return false;
            }
            if (m_dockingControlData.m_dockingBlend < 0.0f || m_dockingControlData.m_dockingBlend > 1.0f)
            {
                Debug.LogError("Docking blend value error! --->" + m_dockingControlData.m_dockingBlend);
                return false;
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
                    m_dockingTarget.GetDcokedTransfrom(oldReferenceFromTarget, out referenceFromDesiredTarget, 
                        out m_dockedVertexStatus);

                    // blend
                    if (1.0 == m_dockingControlData.m_dockingBlend)
                    {
                        oldReferenceFromModel = DockingTransform.Multiply(
                            referenceFromDesiredTarget, DockingTransform.Inverse(modelFromTarget));
                    }
                    else
                    {
                        // blend and get error
                        DockingTransform error;
                        GetError(oldReferenceFromTarget, referenceFromDesiredTarget,
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

                worldFromModel = DockingTransform.Multiply(m_worldFromNewReference, oldReferenceFromModel);
                worldFromModel.ApplyDockingTransformWS(m_animator.transform);

                //m_worldFromOldReference = m_worldFromNewReference;
                DynamicAdjustPlaybackSpeed();
                // 处理手部IK
                SolveHandIK(m_dockingTarget, GetDockedVertexWS(), GetDockedVertexStatus());

            }
            else
            {
                // 处理手部IK
                SolveHandIK(m_dockingTarget, GetDockedVertexWS(), GetDockedVertexStatus());

                SetDefaultValue();
                m_dockingControlData = null;
                return false;
            }            
            
            m_dockingControlData = null;
            return true;
        }

        public void Notify(DockingControlData data)
        {
            m_dockingControlData = data;
        }
        public DockingControlData GetDockingControllerData()
        {
            return m_dockingControlData;
        }

        public void SetDockingTarget(DockingTarget target)
        {
            Debug.Log("DockingDriver: Set the docking target " + target.gameObject.name);
            m_dockingTarget = target;
            m_dockingNextTarget = null;
            SetWorldFromReference(target);
        }
        public void SetDockingNextTarget(DockingTarget target)
        {
            Debug.Log("DockingDriver: Set the docking next target " + target.gameObject.name);
            m_dockingNextTarget = target;            
        }
        public void SwitchToNextDockingTarget()
        {
            m_dockingTarget = m_dockingNextTarget;
            m_dockingNextTarget = null;
            SetWorldFromReference(m_dockingTarget);
        }

        public DockingTarget GetDockingTarget()
        {
            return m_dockingTarget;
        }

        // 消除 Docking Target 非等比缩放，重构WorldFromReferenceTransform
        private void SetWorldFromReference(DockingTarget target)
        {
            m_worldFromOldReference = target.GetWorldFromReference();
            m_worldFromNewReference = target.GetWorldFromReference();
        }

        // 消除 Docking Target 非等比缩放，重构WorldFromReferenceTransform
        // 在Docking Target 不是static时候，调用该函数
        private void UpdateWorldFromReference(DockingTarget target)
        {
            if (null == target) return;
            m_worldFromOldReference = m_worldFromNewReference;
            m_worldFromNewReference = target.GetWorldFromReference();
        }

        private void GetError(DockingTransform t1, DockingTransform t2,
           float lastBlend, float blend, out DockingTransform error)
        {
            error = new DockingTransform();

            var fraction = Utils.ComputeBlendFraction(BLEND_CURVE_TYPE.SMOOTH_TO_SMOOOTH, lastBlend, blend);
            if (fraction > 0.0f)
            {
                
                error.translation = t2.translation - t1.translation;
                error.rotation = t2.rotation * Quaternion.Inverse(t1.rotation);
                if (fraction < 1.0f)
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
            return m_dockingBone;
        }

        // 处理手部IK
        protected void SolveHandIK(DockingTarget target, TR tr, DockedVertexStatus status)
        {
            bool handIK = false;
            DockingLineStripTarget lineStripTarget = target as DockingLineStripTarget;
            if (lineStripTarget)
            {
                handIK = lineStripTarget.m_handIK;
                handIK = handIK & (m_lastBlend == 1.0f);

            }
            m_fullBodyIK.SetEnableIK(handIK);
            if (lineStripTarget)
            {
                var leftHand = m_animator.GetBoneTransform(HumanBodyBones.LeftHand);
                var rightHand = m_animator.GetBoneTransform(HumanBodyBones.RightHand);

                var leftHandDocked = lineStripTarget.GetDockedPointWS(leftHand.position, m_animator.rootRotation);
                var rightHandDocked = lineStripTarget.GetDockedPointWS(rightHand.position, m_animator.rootRotation);

                m_fullBodyIK.leftHandTargetOffset = Vector3.up * (leftHandDocked.translation.y - tr.translation.y);
                m_fullBodyIK.rightHandTargetOffset = Vector3.up * (rightHandDocked.translation.y - tr.translation.y);
            }
        }

        // 动态调整动画播放时间速度扭曲
        private void DynamicAdjustPlaybackSpeed()
        {            
            if (null == m_dockingControlData) return;
            if (m_dockingControlData.m_dockingBlend == 1.0f) return;
            if (m_adjustPlayBackSpeed)
            {
                var dist1 = (m_worldFromLastTarget.translation - m_animator.transform.position).magnitude;
                var dist2 = (m_worldFromLastDesiredTarget.translation - m_animator.transform.position).magnitude;
                if (dist2 > dist1)
                {
                    m_desiredPlaybackSpeed = dist1 / dist2;
                    Debug.Log(m_desiredPlaybackSpeed);
                }
            }            
        }

        private void OnDrawGizmos()      
        {
            DockingGizmos.PushGizmosData();
            
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(m_worldFromLastDesiredTarget.translation, 0.1f);
            
            DockingGizmos.DrawCoordinateFrameWS(m_worldFromLastTarget);
            DockingGizmos.DrawCoordinateFrameWS(m_worldFromLastDesiredTarget);

            DockingGizmos.PopGizmosData();
        }
        private void OnGUI()
        {
            if(m_adjustPlayBackSpeed)
                GUI.Label(new Rect(50, 50, 100, 100), m_animator.speed.ToString());
        }
    }
}

