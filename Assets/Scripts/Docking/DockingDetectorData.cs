using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Docking
{
    [System.Flags]
    public enum DetectorType
    {
        None = 0x00,
        LowDetector = 0x1,          // ÆÕÍ¨µÍ½Ç¶ÈËÑË÷Æ÷
        HighDetector = 0x2,         // ÆÕÍ¨¸ß½Ç¶ÈËÑË÷Æ÷

        HangDetector = 0x4,         // ÅÊÅÀÁ¢ÃæËÑË÷Æ÷
        TopDetector = 0x8,          // ¶¥²¿ËÑË÷Æ÷
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

        public float m_phi = 20;  // Î³¶È
        public float m_lamda = 0; // ¾­¶È
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