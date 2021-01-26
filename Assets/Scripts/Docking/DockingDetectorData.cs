using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Docking
{
    [System.Flags]
    public enum DetectorType
    {
        None = 0x00,
        LowDetector = 0x1,          // ��ͨ�ͽǶ�������
        HighDetector = 0x2,         // ��ͨ�߽Ƕ�������

        HangDetector = 0x4,         // ��������������
        TopDetector = 0x8,          // ����������
        HangBackDetector = 0x16
    }

    [System.Serializable]
    public class DetectorData
    {
        public DetectorType type = DetectorType.None;

        public Vector3 m_biasMS;

        [Range(10, 80)]
        public float m_fov = 40;
        
        [Range(3, 9)]
        public float m_maxDist = 5.0f;
        
        [Range(0.2f, 2)]
        public float m_minDist = 1.0f;

        public float m_phi = 20;  // γ��
        public float m_lamda = 0; // ����
    }

    [CreateAssetMenu(fileName = "DockingDetectorData", menuName = "Docking/DetectorData", order = 2)]
    public class DockingDetectorData : ScriptableObject
    {
        public DetectorData[] dataArray;

        public Dictionary<DetectorType, DetectorData> dict { get; set; }

        private void OnEnable()
        {
            dict = new Dictionary<DetectorType, DetectorData>();
            
            foreach(var data in dataArray)
            {
                if(dict.ContainsKey(data.type))
                {
                    Debug.LogError("Detector type overlap!");
                }
                else
                {
                    dict.Add(data.type, data);
                }
            }
        }
    }
}