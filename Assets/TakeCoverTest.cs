using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TakeCoverTest : MonoBehaviour
{
    public Docking.DockingDetector detector;
    private Animator animator;
    private Docking.DockingDriver driver;

    // Start is called before the first frame update
    void Start()
    {
        animator = detector.hostPlayer.GetComponent<Animator>();
        driver = animator.GetComponent<Docking.DockingDriver>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            var target = detector.DetectNearestTarget();
            animator.SetTrigger("Commit");
            animator.SetFloat("Height", 1.0f);
            driver.SetDockingTarget(target);

            var closedTargetWS = target.GetClosedPointWS(animator.transform.position);
            var angle = Docking.Utils.GetYawAngle(animator.transform, closedTargetWS);
            
            if (angle > 0)
            {
                Debug.Log(angle);
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
