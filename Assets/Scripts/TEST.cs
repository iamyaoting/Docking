using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TEST : MonoBehaviour
{
    public GameObject go;

    private void Update()
    {
        if(go.GetComponent<Docking.DockingTarget>())
        {
            Debug.Log("YEs");
        }
        else
        {
            Debug.Log("NO");
        }
    }
}
