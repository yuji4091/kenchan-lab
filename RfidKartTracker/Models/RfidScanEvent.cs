using System;

namespace RfidKartTracker.Models
{
    /// <summary>
    /// RFID 扫描事件数据 - RFID scan event data
    /// </summary>
    public class RfidScanEvent
    {
        public string EpcTag { get; set; } = string.Empty;
        public DateTime ScanTime { get; set; }
        public string? KartNumber { get; set; }
        public int SignalStrength { get; set; }

        public RfidScanEvent(string epcTag, int signalStrength = 0)
        {
            EpcTag = epcTag;
            ScanTime = DateTime.Now;
            SignalStrength = signalStrength;
        }

        public override string ToString()
        {
            return $"EPC: {EpcTag}, Kart: {KartNumber ?? "Unknown"}, Time: {ScanTime:HH:mm:ss.fff}, Signal: {SignalStrength}";
        }
    }
}