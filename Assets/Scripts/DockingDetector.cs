﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Rendering;


public class DockingDetector : MonoBehaviour
{
    [Header("Init information")]    
    public Transform hostPlayer;
    public GameObject coneMesh;
    public Vector3 biasMS = new Vector3(0.2f, 1.0f, 0);

    [Range(30, 80)]
    public float fov = 40;
    [Range(3, 6)]
    public float maxDist = 5.0f;
    [Range(0.2f, 2)]
    public float minDist = 1.0f;    

    [Header("Runtime or Debug")]
    [Range(-40, 40)]
    public float elevationAngleMS = 20;
    public bool showMesh = false;
    public bool showGizmos = true;
    public float interval = 0.3f;

        
    private Transform coneTrans;
    private Vector3 directionMS;

    // gui debug related points in WS
    private Vector3[] gizmosPoints = new Vector3[5];
    private int gizmosAngle = 0;
    private LineRenderer gizmosDetector;
    private float gizmosTwistSpeed = 300;
    private DetectContactPoints detector;

    // draw contacts points
    private CommandBuffer commandBuffer = null;
    private Material contactPointMat = null;
    private Mesh contactPointMesh = null;

    // Start is called before the first frame update
    void Start()
    {
        coneTrans = new GameObject("HostPlayer Docking Detector").transform;
        coneTrans.parent = hostPlayer;
        coneMesh = Instantiate(coneMesh);
        coneMesh.transform.parent = coneTrans;
        
        gizmosDetector = gameObject.AddComponent<LineRenderer>();
        gizmosDetector.widthMultiplier = 0.05f;

        gizmosDetector.material = Resources.Load<Material>("Materials/DetectorGizmosMat");

        //add mesh cllider
        var meshCollider = coneMesh.AddComponent<MeshCollider>();
        detector = coneMesh.AddComponent<DetectContactPoints>();
        meshCollider.convex = true;

        var rigidBody = coneMesh.AddComponent<Rigidbody>();
        rigidBody.constraints = RigidbodyConstraints.FreezeAll;

        // contact points
        contactPointMat = Resources.Load<Material>("Materials/ContatctPointMat");
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        contactPointMesh = sphere.GetComponent<MeshFilter>().sharedMesh;
        GameObject.Destroy(sphere);

        commandBuffer = new CommandBuffer();

        Camera.main.AddCommandBuffer(CameraEvent.AfterSkybox, commandBuffer);
#if UNITY_EDITOR
        foreach (UnityEditor.SceneView scenView in UnityEditor.SceneView.sceneViews)
        {
            scenView.camera.AddCommandBuffer(CameraEvent.AfterSkybox, commandBuffer);
        }
#endif
    }

    // Update is called once per frame
    void Update()
    {
        gizmosAngle = (gizmosAngle + (int)(Time.deltaTime * gizmosTwistSpeed)) % 360;
        coneMesh.GetComponent<MeshRenderer>().enabled = showMesh;

        UpdateConeMeshTransformMS();
        UpdateGizmosPoints();

        var ps = detector.GetPoints();
        DrawContactPoints(ps);
    }

    private void OnDisable()
    {
        if (Camera.main)
        {
            Camera.main.RemoveCommandBuffer(CameraEvent.AfterSkybox, commandBuffer);
        }

#if UNITY_EDITOR
        foreach (UnityEditor.SceneView scenView in UnityEditor.SceneView.sceneViews)
        {
            scenView.camera.RemoveCommandBuffer(CameraEvent.AfterSkybox, commandBuffer);
        }
#endif
    }

    private void DrawContactPoints(List<Vector3> points)
    {
        commandBuffer.Clear();

        List<Matrix4x4> matrixs = new List<Matrix4x4>();
        foreach(var p in points)
        {
            Matrix4x4 mat4 = new Matrix4x4();
            mat4.SetTRS(p, Quaternion.identity, Vector3.one * 0.1f);
            matrixs.Add(mat4);
        }

        commandBuffer.DrawMeshInstanced(contactPointMesh, 0, contactPointMat, 0, matrixs.ToArray());

        //Debug.Log(points.Count);
    }

    private void UpdateConeMeshTransformMS()
    {
        // calculate cone's transform
        coneTrans.localPosition = biasMS;
        coneTrans.localScale = new Vector3(
            maxDist * Mathf.Tan(Mathf.Deg2Rad * fov) / Mathf.Tan(Mathf.Deg2Rad * 45),
            maxDist,
            maxDist * Mathf.Tan(Mathf.Deg2Rad * fov) / Mathf.Tan(Mathf.Deg2Rad * 45)
            );

        directionMS = Quaternion.AngleAxis(90 - elevationAngleMS, Vector3.right) * Vector3.up;
        coneTrans.localRotation = Quaternion.FromToRotation(Vector3.up, directionMS);
     }

    private void UpdateGizmosPoints()
    {
        if (!showGizmos) return;
        Vector3 orginP = coneTrans.position;
        Vector3 destP = coneTrans.TransformPoint(Vector3.up);
        Vector3 n = destP - orginP;        

        // 计算空间一个圆
        float r = maxDist * Mathf.Tan(fov * Mathf.Deg2Rad);
        Vector3 u = new Vector3(n.y, -n.x, 0);
        Vector3 v = new Vector3(n.x * n.z, n.y * n.z, -n.x * n.x - n.y * n.y);

        Func<float, Vector3> CalCircleTwoPoint = (float angle) =>
        {
            return destP + r * (u.normalized * Mathf.Cos(Mathf.Deg2Rad * angle)
            + v.normalized * Mathf.Sin(Mathf.Deg2Rad * angle));
        };

        gizmosPoints[0] = orginP;
        gizmosPoints[1] = CalCircleTwoPoint(gizmosAngle);
        gizmosPoints[2] = CalCircleTwoPoint(gizmosAngle + 180);
        gizmosPoints[3] = orginP;
        gizmosPoints[4] = destP;
        gizmosDetector.positionCount = 5;
        gizmosDetector.SetPositions(gizmosPoints);
    }
}
