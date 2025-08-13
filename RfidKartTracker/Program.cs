using System;
using System.Threading;
using System.Threading.Tasks;
using RfidKartTracker;

namespace RfidKartTracker
{
    class Program
    {
        private static KartTrackingApplication? _application;
        private static readonly CancellationTokenSource _cancellationTokenSource = new();

        static async Task Main(string[] args)
        {
            Console.WriteLine("🏎️  RFID 卡丁车跟踪系统");
            Console.WriteLine("🏎️  RFID Kart Tracking System");
            Console.WriteLine("==============================");
            Console.WriteLine();

            // 设置控制台取消处理
            Console.CancelKeyPress += OnCancelKeyPress;

            try
            {
                _application = new KartTrackingApplication();
                
                // 显示操作说明
                DisplayInstructions();

                // 启动应用程序（这会开始自动扫描）
                var scanningTask = _application.StartAsync();

                // 等待用户输入命令
                var inputTask = ProcessUserInputAsync(_cancellationTokenSource.Token);

                // 等待任一任务完成
                await Task.WhenAny(scanningTask, inputTask);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("\n✅ 程序已正常退出");
                Console.WriteLine("✅ Program exited normally");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ 程序异常: {ex.Message}");
                Console.WriteLine($"❌ Program error: {ex.Message}");
            }
            finally
            {
                _application?.Stop();
                Console.WriteLine("\n👋 再见！");
                Console.WriteLine("👋 Goodbye!");
            }
        }

        /// <summary>
        /// 显示操作说明
        /// Display instructions
        /// </summary>
        private static void DisplayInstructions()
        {
            Console.WriteLine("📋 可用命令:");
            Console.WriteLine("📋 Available commands:");
            Console.WriteLine("   's1' - 模拟扫描车号001标签");
            Console.WriteLine("   's2' - 模拟扫描车号002标签");
            Console.WriteLine("   's3' - 模拟扫描车号003标签");
            Console.WriteLine("   's4' - 模拟扫描车号004标签");
            Console.WriteLine("   's5' - 模拟扫描车号005标签");
            Console.WriteLine("   'reset' - 重置所有统计");
            Console.WriteLine("   'quit' 或 Ctrl+C - 退出程序");
            Console.WriteLine();
        }

        /// <summary>
        /// 处理用户输入
        /// Process user input
        /// </summary>
        private static async Task ProcessUserInputAsync(CancellationToken cancellationToken)
        {
            var epcMappings = new Dictionary<string, string>
            {
                ["s1"] = "E20000166021011740209049", // Kart 001
                ["s2"] = "E20000166021011740209050", // Kart 002
                ["s3"] = "E20000166021011740209051", // Kart 003
                ["s4"] = "E20000166021011740209052", // Kart 004
                ["s5"] = "E20000166021011740209053"  // Kart 005
            };

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    Console.Write("💬 输入命令: ");
                    var input = await ReadLineAsync(cancellationToken);
                    
                    if (input == null)
                        continue;
                        
                    input = input.Trim().ToLower();

                    switch (input)
                    {
                        case "quit":
                        case "q":
                        case "exit":
                            _cancellationTokenSource.Cancel();
                            return;

                        case "reset":
                            _application?.ResetStats();
                            break;

                        case var cmd when epcMappings.ContainsKey(cmd):
                            var epcTag = epcMappings[cmd];
                            Console.WriteLine($"🎯 模拟扫描标签: {epcTag}");
                            _application?.SimulateScan(epcTag);
                            break;

                        case "":
                            // 空输入，忽略
                            break;

                        default:
                            Console.WriteLine("❓ 未知命令，请重试");
                            break;
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// 异步读取控制台输入
        /// Asynchronously read console input
        /// </summary>
        private static async Task<string?> ReadLineAsync(CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                try
                {
                    return Console.ReadLine();
                }
                catch
                {
                    return null;
                }
            }, cancellationToken);
        }

        /// <summary>
        /// 处理 Ctrl+C 信号
        /// Handle Ctrl+C signal
        /// </summary>
        private static void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true; // 阻止立即退出
            _cancellationTokenSource.Cancel();
        }
    }
}
