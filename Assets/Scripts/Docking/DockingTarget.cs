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

    public enum DockingTargetType
    {
        TAKE_COVER,
        VAULT,
        CLIMB
    }

    //[RequireComponent(typeof(LineRenderer))]
    public abstract class DockingTarget : MonoBehaviour
    {
        public bool m_active = true;
        public DockingTargetType m_type = DockingTargetType.TAKE_COVER;
        public bool m_isSelected; // 是否被选中

        #region gizmos
        private Dictionary<DockingTargetType, Color> m_gizmosColorDict =
            new Dictionary<DockingTargetType, Color>{
                {DockingTargetType.TAKE_COVER,  Color.green },
                {DockingTargetType.VAULT,       Color.yellow },
                {DockingTargetType.CLIMB,       Color.blue }
            };

        protected const float m_vertexCubeSize = 0.05f;
        protected const float m_coordinateFrameAsixLength = 0.6f;
        protected const float m_lineWidth = 0.03f;
        public Color GetGizmosColor()
        {
            if (m_isSelected)
                return Color.red;
            
            var color = m_gizmosColorDict[m_type];
            return color;
        }

        protected abstract void DrawGizmos();
        protected void OnDrawGizmos()
        {          
            DrawGizmos();            
        }

        //获得WS下的TR点
        protected TR GetTRInWS(TR vertex)
        {
            TR tr = new TR();            
            tr.translation = transform.TransformPoint(vertex.translation);            
            tr.rotation = transform.rotation * vertex.rotation;            
            return tr;
        }

        #endregion
        public abstract TR GetDcokedVertex(Transform unDockedTrans);        

        private void Start()
        {
            m_isSelected = false;
        }        
    }

}

