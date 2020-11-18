using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TakeCoverTest : MonoBehaviour
{
    public Docking.DockingDetector detector;
    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = detector.hostPlayer.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            var target = detector.DetectNearestTarget();
            var vertex = target.GetDcokedVertex(animator.transform);
            animator.SetTrigger("Commit");
            animator.SetFloat("Height", vertex.reserveFloatParam);

            // left or right            
            var angle = Vector3.SignedAngle(animator.transform.InverseTransformVector(vertex.tr.rotation * Vector3.right), Vector3.left, Vector3.up);            
            angle = (angle + 360) % 180;
            if (angle < 90)
            {
                animator.SetFloat("LeftRightSelctor", 1);
            }
            else
            {
                animator.SetFloat("LeftRightSelctor", 0);
            }
        }

        var moveDir = Input.GetAxis("Horizontal");
        animator.SetFloat("MoveDirection", moveDir);
    }
}
