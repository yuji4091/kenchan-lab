using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RfidKartTracker.Models;

namespace RfidKartTracker.Services
{
    /// <summary>
    /// 实时 RFID 自动扫描模块 - Real-time RFID automatic scanning module
    /// 模拟 CF815 读取器功能 - Simulates CF815 reader functionality
    /// </summary>
    public class RfidScannerService
    {
        private readonly Random _random = new();
        private readonly List<string> _availableTags = new();
        private CancellationTokenSource? _scanningCancellation;
        private bool _isScanning = false;

        public event Action<RfidScanEvent>? TagScanned;
        public event Action<string>? ScannerStatusChanged;

        public bool IsScanning => _isScanning;

        public RfidScannerService()
        {
            InitializeAvailableTags();
        }

        /// <summary>
        /// 初始化可用的标签列表（模拟环境）
        /// Initialize available tags list (simulation environment)
        /// </summary>
        private void InitializeAvailableTags()
        {
            _availableTags.AddRange(new[]
            {
                "E20000166021011740209049", // Kart 001
                "E20000166021011740209050", // Kart 002
                "E20000166021011740209051", // Kart 003
                "E20000166021011740209052", // Kart 004
                "E20000166021011740209053", // Kart 005
            });
        }

        /// <summary>
        /// 开始扫描
        /// Start scanning
        /// </summary>
        public async Task StartScanningAsync()
        {
            if (_isScanning)
            {
                return;
            }

            _isScanning = true;
            _scanningCancellation = new CancellationTokenSource();
            ScannerStatusChanged?.Invoke("Scanning started");

            try
            {
                await ScanContinuouslyAsync(_scanningCancellation.Token);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
            }
            finally
            {
                _isScanning = false;
                ScannerStatusChanged?.Invoke("Scanning stopped");
            }
        }

        /// <summary>
        /// 停止扫描
        /// Stop scanning
        /// </summary>
        public void StopScanning()
        {
            _scanningCancellation?.Cancel();
        }

        /// <summary>
        /// 连续扫描逻辑
        /// Continuous scanning logic
        /// </summary>
        private async Task ScanContinuouslyAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // 模拟随机扫描间隔（1-5秒）
                var scanInterval = _random.Next(1000, 5000);
                await Task.Delay(scanInterval, cancellationToken);

                // 随机选择一个标签进行扫描
                if (_availableTags.Count > 0)
                {
                    var tagIndex = _random.Next(_availableTags.Count);
                    var selectedTag = _availableTags[tagIndex];
                    var signalStrength = _random.Next(30, 100); // 模拟信号强度

                    var scanEvent = new RfidScanEvent(selectedTag, signalStrength);
                    TagScanned?.Invoke(scanEvent);
                }
            }
        }

        /// <summary>
        /// 手动扫描指定标签（用于测试）
        /// Manually scan specific tag (for testing)
        /// </summary>
        public void SimulateScan(string epcTag, int signalStrength = 50)
        {
            var scanEvent = new RfidScanEvent(epcTag, signalStrength);
            TagScanned?.Invoke(scanEvent);
        }

        /// <summary>
        /// 添加新的可扫描标签
        /// Add new scannable tag
        /// </summary>
        public void AddAvailableTag(string epcTag)
        {
            if (!_availableTags.Contains(epcTag))
            {
                _availableTags.Add(epcTag);
            }
        }

        /// <summary>
        /// 移除可扫描标签
        /// Remove scannable tag
        /// </summary>
        public void RemoveAvailableTag(string epcTag)
        {
            _availableTags.Remove(epcTag);
        }

        /// <summary>
        /// 获取所有可扫描标签
        /// Get all scannable tags
        /// </summary>
        public IReadOnlyList<string> GetAvailableTags()
        {
            return _availableTags.AsReadOnly();
        }

        /// <summary>
        /// 释放资源
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            StopScanning();
            _scanningCancellation?.Dispose();
        }
    }
}