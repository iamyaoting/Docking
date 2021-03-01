using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VSyncSet60 : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        Application.targetFrameRate = 60;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
