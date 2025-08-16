using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using RfidKartTracker.Models;

namespace RfidKartTracker.Services
{
    /// <summary>
    /// 圈数统计模块 - Lap counting module (LapCounterService)
    /// </summary>
    public class LapCounterService
    {
        private readonly ConcurrentDictionary<string, KartStats> _kartStats = new();
        private readonly Dictionary<string, string> _epcToKartMapping = new();
        private readonly DebounceFilter _debounceFilter;

        public event Action<KartStats>? LapCounted;
        public event Action<string>? UnknownTagScanned;

        public LapCounterService(DebounceFilter debounceFilter)
        {
            _debounceFilter = debounceFilter;
            InitializeDefaultMappings();
        }

        public LapCounterService() : this(new DebounceFilter())
        {
        }

        /// <summary>
        /// 初始化默认的 EPC 到车号映射
        /// Initialize default EPC to kart number mappings
        /// </summary>
        private void InitializeDefaultMappings()
        {
            // 示例映射 - Example mappings
            _epcToKartMapping["E20000166021011740209049"] = "001";
            _epcToKartMapping["E20000166021011740209050"] = "002";
            _epcToKartMapping["E20000166021011740209051"] = "003";
            _epcToKartMapping["E20000166021011740209052"] = "004";
            _epcToKartMapping["E20000166021011740209053"] = "005";
        }

        /// <summary>
        /// 添加或更新 EPC 到车号的映射
        /// Add or update EPC to kart number mapping
        /// </summary>
        public void AddKartMapping(string epcTag, string kartNumber)
        {
            _epcToKartMapping[epcTag] = kartNumber;
        }

        /// <summary>
        /// 处理 RFID 扫描事件
        /// Process RFID scan event
        /// </summary>
        public void ProcessScan(RfidScanEvent scanEvent)
        {
            // 防重复检查
            if (!_debounceFilter.ShouldProcess(scanEvent.EpcTag))
            {
                return;
            }

            // 查找对应的车号
            if (!_epcToKartMapping.TryGetValue(scanEvent.EpcTag, out var kartNumber))
            {
                UnknownTagScanned?.Invoke(scanEvent.EpcTag);
                return;
            }

            scanEvent.KartNumber = kartNumber;

            // 获取或创建车辆统计
            var kartStats = _kartStats.GetOrAdd(kartNumber, _ => new KartStats(kartNumber, scanEvent.EpcTag));

            // 增加圈数
            kartStats.IncrementLap();

            // 触发事件
            LapCounted?.Invoke(kartStats);
        }

        /// <summary>
        /// 获取所有车辆统计
        /// Get all kart statistics
        /// </summary>
        public IEnumerable<KartStats> GetAllKartStats()
        {
            return _kartStats.Values;
        }

        /// <summary>
        /// 获取指定车号的统计
        /// Get statistics for specific kart number
        /// </summary>
        public KartStats? GetKartStats(string kartNumber)
        {
            return _kartStats.TryGetValue(kartNumber, out var stats) ? stats : null;
        }

        /// <summary>
        /// 重置所有统计
        /// Reset all statistics
        /// </summary>
        public void ResetAllStats()
        {
            _kartStats.Clear();
            _debounceFilter.Clear();
        }

        /// <summary>
        /// 重置指定车号的统计
        /// Reset statistics for specific kart number
        /// </summary>
        public void ResetKartStats(string kartNumber)
        {
            if (_kartStats.TryGetValue(kartNumber, out var stats))
            {
                stats.LapCount = 0;
                stats.LastRecognitionTime = DateTime.Now;
            }
        }

        /// <summary>
        /// 获取当前映射的所有车号
        /// Get all currently mapped kart numbers
        /// </summary>
        public IEnumerable<string> GetMappedKartNumbers()
        {
            return _epcToKartMapping.Values;
        }

        /// <summary>
        /// 获取 EPC 到车号的映射
        /// Get EPC to kart number mappings
        /// </summary>
        public IReadOnlyDictionary<string, string> GetEpcMappings()
        {
            return _epcToKartMapping;
        }
    }
}