using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixHangFeetPitch : MonoBehaviour
{
    public Animator animator;
    [Range(-50, 40)]
    public float leftFootAngle;
    [Range(-50, 40)]
    public float rightFootAngle;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LateUpdate()
    {
        var lFootTrans = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
        var rFootTrans = animator.GetBoneTransform(HumanBodyBones.RightFoot);
        lFootTrans.rotation *= Quaternion.AngleAxis(leftFootAngle, Vector3.right);
        rFootTrans.rotation *= Quaternion.AngleAxis(rightFootAngle, Vector3.right);
    }
}
