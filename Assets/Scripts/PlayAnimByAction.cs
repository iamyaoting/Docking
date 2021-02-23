using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAnimByAction : MonoBehaviour
{
    public string stateEntry;

    [Range(0.2f, 1.5f)]
    public float timeScale = 1.0f;

    public Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Time.timeScale = timeScale;
        if (Input.GetKeyDown(KeyCode.E))
        {
            animator.CrossFade(stateEntry, 0);
        }
    }
}
