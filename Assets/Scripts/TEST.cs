using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TEST : MonoBehaviour
{
    public Animator animator;

    private void Update()
    {
        if(Input.GetKey(KeyCode.E))
        {
            animator.SetTrigger("Commit");
        }
    }
}
