using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Docking
{
    // 位姿，位置和朝向
    [System.Serializable]
    public class TR
    {
        public Vector3 translation = Vector3.zero;
        public Quaternion rotation = Quaternion.identity;
        public static TR Lerp(TR start, TR end, float k)
        {
            TR tr = new TR();
            tr.translation = Vector3.Lerp(start.translation, end.translation, k);
            tr.rotation = Quaternion.Slerp(start.rotation, end.rotation, k);
            return tr;
        }
    }

    [System.Serializable]
    public class DockingVertex
    {
        public DockingVertex() { tr = new TR(); }
        public DockingVertex(Vector3 pos, Quaternion rot, float resverdFloat = 0)
        {
            tr = new TR();
            tr.translation = pos;
            tr.rotation = rot;
            reserveFloatParam = resverdFloat;
        }

        public DockingVertex(DockingVertex other)
        {
            tr = new TR();
            tr.translation = other.tr.translation;
            tr.rotation = other.tr.rotation;
            reserveFloatParam = other.reserveFloatParam;
        }

        public TR       tr;

        // 对于take over代表墙高度，0：矮，1：高
        public float    reserveFloatParam = 0;      
    }

    public enum DockingTargetType
    {
        TAKE_COVER,
        VAULT,
        BRACED_HANG,
        HANGING,
        BEAM
    }

    // 角色Dock的点的信息
    public class DockedVertexStatus
    {
        public float                alpha;              // 当前停靠点的alpha, 0代表left，1代表right，用于表示是否停靠再边界
        public float                reserveFloatParam;  // 当前点的额外信息
        public DOCKED_POINT_MOVE_LIMIT  limit;              // 是否到target边界了
    }

    public abstract class DockingTarget : MonoBehaviour
    {
        public bool m_active = true;
        public DockingTargetType m_type = DockingTargetType.TAKE_COVER;
        public bool selected { get; set; } // 是否被选中

        public bool temporary { get; set; } // 是否是临时的, 程序生成的target，而非静态的

        protected float marginDist { 
            get
            {
                switch(m_type)
                {
                    case DockingTargetType.TAKE_COVER:
                        return .7f;
                    case DockingTargetType.HANGING:
                        return .4f;
                }
                return .0f;
            }
        }

        public abstract DockedVertexStatus GetDockedLS(TR undockedTRLS, out DockingVertex dockedVertexLS);

        public void Awake()
        {
            temporary = false;
        }

        // referenceFromTarget 其不考虑scale，不考虑非等比缩放，默认始终为1
        public void GetDcokedTransfrom(DockingTransform referenceFromUndocedTarget, 
            out DockingTransform referenceFromDesiredTarget, out DockedVertexStatus status)
        {
            SafeCheck(referenceFromUndocedTarget);
            var unDockedLS = GetLocalTRFromReferenceSpace(referenceFromUndocedTarget);
            
            DockingVertex dockedLS;
            status = GetDockedLS(unDockedLS, out dockedLS);

//             // 如果为翻墙，则禁止输入
//             if (m_type == DockingTargetType.VAULT) status.limit = DOCKED_POINT_MOVE_LIMIT.ALL;

            referenceFromDesiredTarget = GetReferenceSpaceFromLocalSpace(dockedLS.tr.translation, dockedLS.tr.rotation);
                       
            return;
        }
        
        // 安全性检查，确保reference From Target 的Scale 为 等比缩放
        protected void SafeCheck(DockingTransform referenceFromUndocedTarget)
        {
            if(!DockingTransform.IdentityScale(referenceFromUndocedTarget))
            {
                Debug.LogError("referenceFromTarget Transform's scale must be identity!");
            }
        }

        // 考虑到dockingTarget的scale可能不是1
        // referenceFromTarget中reference空间中scale为1
        // 将referenceFromTarget转换为dockingTarget内的局部TR
        protected TR GetLocalTRFromReferenceSpace(DockingTransform referenceFromUndocedTarget)
        {
            var posWS = transform.position + transform.rotation * referenceFromUndocedTarget.translation;
            TR tr = new TR();
            tr.translation = transform.InverseTransformPoint(posWS);
            tr.rotation = referenceFromUndocedTarget.rotation;
            return tr;
        }

        // 考虑到dockingTarget的scale可能不是1
        // referenceFromTarget中reference空间中scale为1
        // 将dockingTarget内的局部TR转换为referenceFromTarget
        protected DockingTransform GetReferenceSpaceFromLocalSpace(Vector3 posLS, Quaternion rotLS)
        {
            DockingTransform referenceFromTarget = new DockingTransform();
            referenceFromTarget.rotation = rotLS;

            var posWS = transform.TransformPoint(posLS);
            referenceFromTarget.translation =
                Quaternion.Inverse(transform.rotation) * (posWS - transform.position);            
            return referenceFromTarget;
        }

        public DockingTransform GetWorldFromReference()
        {
            DockingTransform trans = new DockingTransform(transform);
            return trans;
        }

        public TR GetDockedPointWS(Vector3 pointWS, Quaternion quat)
        {
            DockedVertexStatus status = null;
            TR dockedTRWS = null;
            GetDockedPointWS(pointWS, quat, out dockedTRWS, out status);
            return dockedTRWS;           
        }

        public void GetDockedPointWS(Vector3 pointWS, Quaternion quat, out TR dockedTRWS, out DockedVertexStatus status)
        {
            DockingTransform worldFromUndockedPoint = new DockingTransform();
            worldFromUndockedPoint.translation = pointWS;
            DockingTransform worldFromReference = GetWorldFromReference();
            DockingTransform referenceFromUndockedPoint = DockingTransform.Multiply(
                DockingTransform.Inverse(worldFromReference), worldFromUndockedPoint);
            DockingTransform referenceFromDockedVertex = null;            
            GetDcokedTransfrom(referenceFromUndockedPoint, out referenceFromDockedVertex, out status);
            
            DockingTransform worldFromDockedPoint = DockingTransform.Multiply(worldFromReference, referenceFromDockedVertex);
            
            dockedTRWS = new TR();
            dockedTRWS.translation = worldFromDockedPoint.translation;
            dockedTRWS.rotation = worldFromDockedPoint.rotation;            
        }


        private void Update()
        {
            selected = false;
        }


        #region gizmos
        private Dictionary<DockingTargetType, Color> m_gizmosColorDict =
            new Dictionary<DockingTargetType, Color>{
                {DockingTargetType.TAKE_COVER,  Color.green },
                {DockingTargetType.VAULT,       Color.yellow },
                {DockingTargetType.BRACED_HANG, Color.blue },
                {DockingTargetType.HANGING,     Color.cyan},
                {DockingTargetType.BEAM,        Color.white}
            };

        protected const float m_lineWidth = 0.1f;
        public Color GetGizmosColor()
        {
            if (selected)
                return Color.red;

            var color = m_gizmosColorDict[m_type];
            return color;
        }

        protected abstract void DrawGizmos();
        protected void OnDrawGizmos()
        {
            if (m_active && enabled)
            {
                DrawGizmos();
            }
        }

        //LS-->WS,获得WS下的TR点
        protected TR GetTRInWS(TR vertex)
        {
            TR tr = new TR();
            tr.translation = transform.TransformPoint(vertex.translation);
            tr.rotation = transform.rotation * vertex.rotation;
            return tr;
        }

        #endregion

    }

}

