using System;
using System.Collections.Generic;
using RfidKartTracker.Models;

namespace RfidKartTracker.Services
{
    /// <summary>
    /// 防重复读取逻辑模块 - Anti-duplicate reading logic module (DebounceFilter)
    /// </summary>
    public class DebounceFilter
    {
        private readonly Dictionary<string, DateTime> _lastScanTimes = new();
        private readonly TimeSpan _minInterval;

        public event Action<string>? TagIgnored;

        public DebounceFilter(TimeSpan minInterval)
        {
            _minInterval = minInterval;
        }

        public DebounceFilter() : this(TimeSpan.FromSeconds(3))
        {
        }

        /// <summary>
        /// 检查标签是否应该被处理（不是重复扫描）
        /// Check if tag should be processed (not a duplicate scan)
        /// </summary>
        public bool ShouldProcess(string epcTag)
        {
            var now = DateTime.Now;
            
            if (_lastScanTimes.TryGetValue(epcTag, out var lastScanTime))
            {
                var timeSinceLastScan = now - lastScanTime;
                if (timeSinceLastScan < _minInterval)
                {
                    TagIgnored?.Invoke(epcTag);
                    return false;
                }
            }

            _lastScanTimes[epcTag] = now;
            return true;
        }

        /// <summary>
        /// 获取标签最后扫描时间
        /// Get last scan time for a tag
        /// </summary>
        public DateTime? GetLastScanTime(string epcTag)
        {
            return _lastScanTimes.TryGetValue(epcTag, out var time) ? time : null;
        }

        /// <summary>
        /// 清除所有记录
        /// Clear all records
        /// </summary>
        public void Clear()
        {
            _lastScanTimes.Clear();
        }

        /// <summary>
        /// 移除旧记录（超过指定时间的）
        /// Remove old records (older than specified time)
        /// </summary>
        public void CleanupOldRecords(TimeSpan maxAge)
        {
            var cutoffTime = DateTime.Now - maxAge;
            var keysToRemove = new List<string>();

            foreach (var kvp in _lastScanTimes)
            {
                if (kvp.Value < cutoffTime)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                _lastScanTimes.Remove(key);
            }
        }
    }
}