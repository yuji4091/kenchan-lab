using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RfidKartTracker.Models;
using RfidKartTracker.Services;

namespace RfidKartTracker
{
    /// <summary>
    /// RFID 卡丁车跟踪系统主应用程序
    /// Main RFID Kart Tracking System Application
    /// </summary>
    public class KartTrackingApplication
    {
        private readonly RfidScannerService _scannerService;
        private readonly LapCounterService _lapCounterService;
        private readonly DebounceFilter _debounceFilter;

        public KartTrackingApplication()
        {
            _debounceFilter = new DebounceFilter(TimeSpan.FromSeconds(3));
            _lapCounterService = new LapCounterService(_debounceFilter);
            _scannerService = new RfidScannerService();

            SetupEventHandlers();
        }

        /// <summary>
        /// 设置事件处理器
        /// Setup event handlers
        /// </summary>
        private void SetupEventHandlers()
        {
            // RFID 扫描事件处理
            _scannerService.TagScanned += OnTagScanned;
            _scannerService.ScannerStatusChanged += OnScannerStatusChanged;

            // 圈数统计事件处理
            _lapCounterService.LapCounted += OnLapCounted;
            _lapCounterService.UnknownTagScanned += OnUnknownTagScanned;

            // 防重复过滤事件处理
            _debounceFilter.TagIgnored += OnTagIgnored;
        }

        /// <summary>
        /// 启动应用程序
        /// Start the application
        /// </summary>
        public async Task StartAsync()
        {
            Console.WriteLine("🏎️  RFID 卡丁车跟踪系统启动中...");
            Console.WriteLine("🏎️  RFID Kart Tracking System Starting...");
            Console.WriteLine("=====================================");
            
            DisplayCurrentMappings();
            Console.WriteLine();
            
            Console.WriteLine("📡 开始 RFID 扫描...");
            Console.WriteLine("📡 Starting RFID scanning...");
            
            await _scannerService.StartScanningAsync();
        }

        /// <summary>
        /// 停止应用程序
        /// Stop the application
        /// </summary>
        public void Stop()
        {
            Console.WriteLine("\n🛑 停止系统...");
            Console.WriteLine("🛑 Stopping system...");
            
            _scannerService.StopScanning();
            _scannerService.Dispose();
            
            DisplayFinalStats();
        }

        /// <summary>
        /// 显示当前映射配置
        /// Display current mapping configuration
        /// </summary>
        private void DisplayCurrentMappings()
        {
            Console.WriteLine("🔗 当前 EPC 到车号映射:");
            Console.WriteLine("🔗 Current EPC to Kart mappings:");
            
            var mappings = _lapCounterService.GetEpcMappings();
            foreach (var mapping in mappings)
            {
                Console.WriteLine($"   {mapping.Key} → 车号 {mapping.Value}");
            }
        }

        /// <summary>
        /// 显示最终统计
        /// Display final statistics
        /// </summary>
        private void DisplayFinalStats()
        {
            Console.WriteLine("\n📊 最终统计结果:");
            Console.WriteLine("📊 Final Statistics:");
            Console.WriteLine("==================");
            
            var allStats = _lapCounterService.GetAllKartStats().OrderBy(s => s.KartNumber);
            
            if (!allStats.Any())
            {
                Console.WriteLine("   没有检测到任何车辆活动");
                Console.WriteLine("   No kart activity detected");
                return;
            }

            foreach (var stats in allStats)
            {
                Console.WriteLine($"   车号 {stats.KartNumber}: {stats.LapCount} 圈 - 最后识别: {stats.LastRecognitionTime:HH:mm:ss}");
            }
        }

        /// <summary>
        /// 标签扫描事件处理
        /// Tag scanned event handler
        /// </summary>
        private void OnTagScanned(RfidScanEvent scanEvent)
        {
            Console.WriteLine($"📡 扫描: {scanEvent}");
            _lapCounterService.ProcessScan(scanEvent);
        }

        /// <summary>
        /// 扫描器状态变化事件处理
        /// Scanner status changed event handler
        /// </summary>
        private void OnScannerStatusChanged(string status)
        {
            Console.WriteLine($"📟 扫描器状态: {status}");
        }

        /// <summary>
        /// 圈数统计事件处理
        /// Lap counted event handler
        /// </summary>
        private void OnLapCounted(KartStats kartStats)
        {
            Console.WriteLine($"🏁 车号 {kartStats.KartNumber} 完成第 {kartStats.LapCount} 圈! 时间: {kartStats.LastRecognitionTime:HH:mm:ss}");
            Console.WriteLine($"🏁 Kart {kartStats.KartNumber} completed lap {kartStats.LapCount}! Time: {kartStats.LastRecognitionTime:HH:mm:ss}");
            
            // 显示当前排行榜
            DisplayLeaderboard();
        }

        /// <summary>
        /// 未知标签扫描事件处理
        /// Unknown tag scanned event handler
        /// </summary>
        private void OnUnknownTagScanned(string epcTag)
        {
            Console.WriteLine($"❓ 未知标签: {epcTag}");
            Console.WriteLine($"❓ Unknown tag: {epcTag}");
        }

        /// <summary>
        /// 标签被忽略事件处理
        /// Tag ignored event handler
        /// </summary>
        private void OnTagIgnored(string epcTag)
        {
            Console.WriteLine($"⏱️  标签 {epcTag} 被防重复过滤器忽略");
        }

        /// <summary>
        /// 显示排行榜
        /// Display leaderboard
        /// </summary>
        private void DisplayLeaderboard()
        {
            var allStats = _lapCounterService.GetAllKartStats()
                .Where(s => s.LapCount > 0)
                .OrderByDescending(s => s.LapCount)
                .ThenBy(s => s.LastRecognitionTime)
                .ToList();

            if (!allStats.Any())
                return;

            Console.WriteLine("\n🏆 当前排行榜:");
            Console.WriteLine("🏆 Current Leaderboard:");
            Console.WriteLine("=====================");
            
            for (int i = 0; i < allStats.Count; i++)
            {
                var stats = allStats[i];
                var position = i + 1;
                var medal = position switch
                {
                    1 => "🥇",
                    2 => "🥈", 
                    3 => "🥉",
                    _ => $"{position}."
                };
                
                Console.WriteLine($"   {medal} 车号 {stats.KartNumber}: {stats.LapCount} 圈");
            }
            Console.WriteLine();
        }

        /// <summary>
        /// 手动模拟扫描（用于测试）
        /// Manually simulate scan (for testing)
        /// </summary>
        public void SimulateScan(string epcTag)
        {
            _scannerService.SimulateScan(epcTag);
        }

        /// <summary>
        /// 重置所有统计
        /// Reset all statistics
        /// </summary>
        public void ResetStats()
        {
            _lapCounterService.ResetAllStats();
            Console.WriteLine("📊 所有统计已重置");
            Console.WriteLine("📊 All statistics reset");
        }
    }
}