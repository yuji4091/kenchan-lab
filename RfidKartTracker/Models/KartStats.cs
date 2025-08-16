using System;

namespace RfidKartTracker.Models
{
    /// <summary>
    /// 卡丁车统计数据结构 - Kart statistics data structure
    /// </summary>
    public class KartStats
    {
        public string KartNumber { get; set; } = string.Empty;
        public int LapCount { get; set; } = 0;
        public DateTime LastRecognitionTime { get; set; } = DateTime.MinValue;
        public string EpcTag { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        public KartStats(string kartNumber, string epcTag)
        {
            KartNumber = kartNumber;
            EpcTag = epcTag;
            LastRecognitionTime = DateTime.Now;
        }

        public void IncrementLap()
        {
            LapCount++;
            LastRecognitionTime = DateTime.Now;
        }

        public TimeSpan TimeSinceLastScan => DateTime.Now - LastRecognitionTime;

        public override string ToString()
        {
            return $"Kart: {KartNumber}, Laps: {LapCount}, Last: {LastRecognitionTime:HH:mm:ss}";
        }
    }
}