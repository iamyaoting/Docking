using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Docking
{
    public class DockingTarget_Test : MonoBehaviour
    {
        public DockingTarget target;

        private DockingTransform referenceFromModel = new DockingTransform();
        private DockingTransform referenceFromDesiredTarget = null;
        private DockingTransform worldFromReference = null;

        // Start is called before the first frame update
        void Start()
        {
            if(target == null)
            {
                Debug.LogError("Target is null!!");
            }    
        }

        // Update is called once per frame
        void Update()
        {
            worldFromReference = new DockingTransform(target.transform);
            var worldFromModel = new DockingTransform(transform);
            referenceFromModel = DockingTransform.Multiply(DockingTransform.Inverse(worldFromReference), worldFromModel);
            DockedVertexStatus status;
            target.GetDcokedTransfrom(referenceFromModel, out referenceFromDesiredTarget, out status);
        }

        private void OnDrawGizmos()
        {
            if (null == referenceFromDesiredTarget || null == worldFromReference) return;

            DockingGizmos.PushGizmosData();

            var worldFromModel = DockingTransform.Multiply(worldFromReference, referenceFromModel);
            var worldFormDesiredTarget = DockingTransform.Multiply(worldFromReference, referenceFromDesiredTarget);

            DockingGizmos.DrawCoordinateFrameWS(worldFromModel);
            DockingGizmos.DrawCoordinateFrameWS(worldFormDesiredTarget);

            DockingGizmos.PopGizmosData();
        }
    }
}

