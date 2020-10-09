using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Docking
{
    [System.Serializable]
    public class DockingVertex
    {
        public Vector3 position = Vector3.zero;
        public Quaternion rotation = Quaternion.identity;
    }

    public enum DockingTargetType
    {
        TAKE_COVER,
        VAULT
    }    

    public abstract class DockingTarget : MonoBehaviour
    {
        public bool m_active = true;

        public DockingTargetType m_type = DockingTargetType.TAKE_COVER;

        #region gizmos
        public Dictionary<DockingTargetType, Color> colors =
            new Dictionary<DockingTargetType, Color>{
                {DockingTargetType.TAKE_COVER,  Color.green },
                {DockingTargetType.VAULT,       Color.red }
            };
        public float sphereSize = 0.05f;
        #endregion
        public abstract DockingVertex GetDcokedVertex(Transform unDockedTrans);
    }

}

