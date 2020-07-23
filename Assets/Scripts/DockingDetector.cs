using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DockingDetector : MonoBehaviour
{
    [Header("Init information")]    
    public Transform hostPlayer;
    public GameObject coneMesh;
    public Vector3 bias = new Vector3(0, 1.0f, 0);

    [Range(30, 80)]
    public float fov = 40;
    public float maxDist = 4.0f;
    public float minDist = 0.3f;
    

    [Header("Runtime or Debug")]    
    public float direction = 20;
    public bool showMesh = false;
    public float interval = 0.3f;        
    
    // private
    private Transform coneTrans;

    // Start is called before the first frame update
    void Start()
    {
        coneTrans = new GameObject("HostPlayer Docking Detector").transform;
        coneTrans.parent = hostPlayer;
        Instantiate(coneMesh).transform.parent = coneTrans;

        // calculate cone's transform
        coneTrans.localPosition = bias;       
        coneTrans.localScale = new Vector3(
            Mathf.Tan(Mathf.Deg2Rad * fov) / Mathf.Tan(Mathf.Deg2Rad * 45),
            maxDist / 1,
            Mathf.Tan(Mathf.Deg2Rad * fov) / Mathf.Tan(Mathf.Deg2Rad * 45)
            );
        coneTrans.localRotation = Quaternion.AngleAxis(90 - direction, Vector3.right);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void UpdateConeMeshTransformMs()
    {

    }
}
