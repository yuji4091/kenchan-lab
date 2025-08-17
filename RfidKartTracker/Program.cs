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

        /// <summary>
        /// 主程序入口点 - Main执行流程的起始点
        /// Main program entry point - Starting point of Main execution flow
        /// </summary>
        static async Task Main(string[] args)
        {
            Console.WriteLine("🏎️  RFID 卡丁车跟踪系统");
            Console.WriteLine("🏎️  RFID Kart Tracking System");
            Console.WriteLine("==============================");
            Console.WriteLine();

            // 设置控制台取消处理 - Main线程的信号处理机制
            // Setup console cancellation handling - Signal handling mechanism for Main thread
            Console.CancelKeyPress += OnCancelKeyPress;

            try
            {
                // Main: 初始化核心应用程序实例
                // Main: Initialize core application instance
                _application = new KartTrackingApplication();
                
                // Main: 显示操作说明
                // Main: Display operation instructions
                DisplayInstructions();

                // Branch 1: 启动RFID扫描分支（后台持续扫描任务）
                // Branch 1: Start RFID scanning branch (background continuous scanning task)
                var scanningTask = _application.StartAsync();

                // Branch 2: 启动用户输入处理分支（交互命令处理任务）
                // Branch 2: Start user input processing branch (interactive command handling task)
                var inputTask = ProcessUserInputAsync(_cancellationTokenSource.Token);

                // Main: 并发协调 - 等待任一分支完成即继续执行
                // Main: Concurrency coordination - wait for any branch to complete
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
        /// Branch 2: 用户输入处理分支 - 处理交互命令的并发分支
        /// Branch 2: User input processing branch - Concurrent branch for handling interactive commands
        /// </summary>
        private static async Task ProcessUserInputAsync(CancellationToken cancellationToken)
        {
            // Branch 2: EPC标签到命令的映射配置
            // Branch 2: EPC tag to command mapping configuration
            var epcMappings = new Dictionary<string, string>
            {
                ["s1"] = "E20000166021011740209049", // Kart 001
                ["s2"] = "E20000166021011740209050", // Kart 002
                ["s3"] = "E20000166021011740209051", // Kart 003
                ["s4"] = "E20000166021011740209052", // Kart 004
                ["s5"] = "E20000166021011740209053"  // Kart 005
            };

            // Branch 2: 持续监听用户输入的主循环
            // Branch 2: Main loop for continuously listening to user input
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    Console.Write("💬 输入命令: ");
                    var input = await ReadLineAsync(cancellationToken);
                    
                    if (input == null)
                        continue;
                        
                    input = input.Trim().ToLower();

                    // Branch 2: 命令分支决策树 - 根据用户输入选择执行路径
                    // Branch 2: Command branch decision tree - Choose execution path based on user input
                    switch (input)
                    {
                        case "quit":
                        case "q":
                        case "exit":
                            // Branch 2 → Main: 触发主程序退出
                            // Branch 2 → Main: Trigger main program exit
                            _cancellationTokenSource.Cancel();
                            return;

                        case "reset":
                            // Branch 2 → Application: 重置统计分支
                            // Branch 2 → Application: Reset statistics branch
                            _application?.ResetStats();
                            break;

                        case var cmd when epcMappings.ContainsKey(cmd):
                            // Branch 2 → Scanning: 模拟扫描分支
                            // Branch 2 → Scanning: Simulate scanning branch
                            var epcTag = epcMappings[cmd];
                            Console.WriteLine($"🎯 模拟扫描标签: {epcTag}");
                            _application?.SimulateScan(epcTag);
                            break;

                        case "":
                            // Branch 2: 空输入处理分支
                            // Branch 2: Empty input handling branch
                            break;

                        default:
                            // Branch 2: 未知命令处理分支
                            // Branch 2: Unknown command handling branch
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
