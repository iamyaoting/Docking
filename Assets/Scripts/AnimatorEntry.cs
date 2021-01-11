using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorEntry : MonoBehaviour
{
    public string stateEntry;

    [Range(0.2f, 1.0f)]
    public float timeScale = 1.0f;


    // Start is called before the first frame update
    void Start()
    {
        var animators = Object.FindObjectsOfType<Animator>();
        foreach(var animator in animators)
        {
            animator.CrossFade(stateEntry, 0);
        }
    }

    // Update is called once per frame
    void Update()
    {
        Time.timeScale = timeScale;
    }
}
