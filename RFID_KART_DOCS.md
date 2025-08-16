# 🏎️ RFID Kart Tracking System
# 🏎️ RFID 卡丁车跟踪系统

A comprehensive real-time RFID tracking system for go-kart racing, designed to automatically count laps and maintain live leaderboards.

## 📋 Features / 功能特性

### ✅ Module A: Real-time RFID Automatic Scanning
- **CF815 Reader Integration**: Simulates UHFReader288.dll functionality
- **EPC to Kart Mapping**: Automatic mapping of RFID tags to kart numbers
- **Real-time Scanning**: Continuous RFID tag detection with signal strength monitoring
- **Event-driven Architecture**: Asynchronous tag scanning with event notifications

### ✅ Module B: Lap Counting Service (LapCounterService)
- **KartStats Data Structure**: Tracks kart number, lap count, and last recognition time
- **Multi-kart Support**: Independent lap counting for multiple karts simultaneously
- **Live Leaderboard**: Real-time ranking based on lap count and timing
- **Event Notifications**: Lap completion events with detailed statistics

### ✅ Module C: Anti-duplicate Reading Logic (DebounceFilter)
- **Debounce Mechanism**: 3-second default interval to prevent duplicate readings
- **Per-tag Tracking**: Individual last scan time tracking for each RFID tag
- **Configurable Intervals**: Adjustable minimum recognition intervals
- **Cleanup Functionality**: Automatic removal of old records

## 🏗️ Architecture / 系统架构

```
┌─────────────────────────────────────────────────────────────────┐
│                    KartTrackingApplication                      │
│                     (Main Application)                          │
└─────────────────────┬───────────────────────────────────────────┘
                      │
                      ▼
┌─────────────────────────────────────────────────────────────────┐
│                  Event Coordination                             │
│  • TagScanned → ProcessScan → LapCounted → DisplayLeaderboard  │
└─────────────────────┬───────────────────────────────────────────┘
                      │
           ┌──────────┼──────────┐
           ▼          ▼          ▼
┌──────────────┐ ┌──────────────┐ ┌──────────────┐
│RfidScannerSvc│ │LapCounterSvc │ │DebounceFilter│
│              │ │              │ │              │
│• Continuous  │ │• Kart Mapping│ │• Anti-dup    │
│  Scanning    │ │• Lap Counting│ │  Logic       │
│• Tag Events  │ │• Statistics  │ │• Time Track  │
└──────────────┘ └──────────────┘ └──────────────┘
       │                 │               │
       ▼                 ▼               ▼
┌──────────────┐ ┌──────────────┐ ┌──────────────┐
│ RfidScanEvent│ │  KartStats   │ │LastScanTimes │
│• EPC Tag     │ │• Kart Number │ │• Tag->Time   │
│• Scan Time   │ │• Lap Count   │ │  Mapping     │
│• Signal      │ │• Last Time   │ │              │
└──────────────┘ └──────────────┘ └──────────────┘
```

## 🚀 Quick Start / 快速开始

### Prerequisites / 前置要求
- .NET 8.0 or later
- Windows, Linux, or macOS

### Installation / 安装

1. Clone the repository / 克隆仓库:
```bash
git clone https://github.com/yuji4091/yuji4091.git
cd yuji4091/RfidKartTracker
```

2. Build the project / 构建项目:
```bash
dotnet build
```

3. Run the application / 运行应用:
```bash
dotnet run
```

### Usage / 使用方法

The application starts with automatic RFID scanning simulation. Use the following commands:

应用程序启动后会自动开始 RFID 扫描模拟。使用以下命令：

- `s1` - Simulate scan for Kart 001 / 模拟扫描车号 001
- `s2` - Simulate scan for Kart 002 / 模拟扫描车号 002  
- `s3` - Simulate scan for Kart 003 / 模拟扫描车号 003
- `s4` - Simulate scan for Kart 004 / 模拟扫描车号 004
- `s5` - Simulate scan for Kart 005 / 模拟扫描车号 005
- `reset` - Reset all statistics / 重置所有统计
- `quit` - Exit application / 退出应用

## 📊 Sample Output / 示例输出

```
🏎️  RFID 卡丁车跟踪系统
🏎️  RFID Kart Tracking System
==============================

📋 可用命令:
📋 Available commands:
   's1' - 模拟扫描车号001标签
   's2' - 模拟扫描车号002标签
   ...

🔗 当前 EPC 到车号映射:
🔗 Current EPC to Kart mappings:
   E20000166021011740209049 → 车号 001
   E20000166021011740209050 → 车号 002
   ...

📡 扫描: EPC: E20000166021011740209049, Kart: 001, Time: 14:30:15.123, Signal: 85
🏁 车号 001 完成第 1 圈! 时间: 14:30:15
🏁 Kart 001 completed lap 1! Time: 14:30:15

🏆 当前排行榜:
🏆 Current Leaderboard:
=====================
   🥇 车号 001: 1 圈
```

## 🔧 Configuration / 配置

### Default EPC Mappings / 默认 EPC 映射
```csharp
E20000166021011740209049 → Kart 001
E20000166021011740209050 → Kart 002
E20000166021011740209051 → Kart 003
E20000166021011740209052 → Kart 004
E20000166021011740209053 → Kart 005
```

### Debounce Settings / 防抖设置
- Default interval: 3 seconds / 默认间隔：3秒
- Configurable per instance / 每个实例可配置
- Prevents duplicate lap counting / 防止重复计圈

## 🧪 Testing / 测试

Run the test suite / 运行测试套件:
```bash
cd RfidKartTracker.Tests
dotnet test
```

Test coverage includes:
- KartStats model validation
- DebounceFilter logic testing
- LapCounterService functionality
- Event handling verification

## 📁 Project Structure / 项目结构

```
RfidKartTracker/
├── Models/
│   ├── KartStats.cs           # Kart statistics data model
│   └── RfidScanEvent.cs       # RFID scan event data
├── Services/
│   ├── RfidScannerService.cs  # RFID scanning simulation
│   ├── LapCounterService.cs   # Lap counting logic
│   └── DebounceFilter.cs      # Anti-duplicate filtering
├── KartTrackingApplication.cs # Main application coordinator
├── Program.cs                 # Application entry point
└── RfidKartTracker.csproj     # Project file

RfidKartTracker.Tests/
├── Models/
│   └── KartStatsTests.cs      # Model unit tests
├── Services/
│   ├── DebounceFilterTests.cs # Debounce logic tests
│   └── LapCounterServiceTests.cs # Lap counter tests
└── RfidKartTracker.Tests.csproj # Test project file
```

## 🔮 Future Enhancements / 未来扩展

### ⏳ Phase D: UI Enhancement Module (KartDashboard)
- Multi-kart grouped display (shift/color grouping)
- Support for kart number, lap count, status icons
- Dynamic layout adaptation for different screen sizes

### ⏳ Phase E: LED Display Control Module (LedDisplayService)
- Display content format design (kart number + lap count)
- Serial port or GPIO control protocol encapsulation
- Support for blinking animations and shift change notifications

## 🤝 Contributing / 贡献

1. Fork the repository / 克隆仓库
2. Create a feature branch / 创建功能分支
3. Make your changes / 进行修改
4. Add tests for new functionality / 为新功能添加测试
5. Submit a pull request / 提交拉取请求

## 📄 License / 许可证

This project is open source and available under the MIT License.

## 📞 Support / 支持

For questions or support, please open an issue in the GitHub repository.

如有问题或需要支持，请在 GitHub 仓库中开启 issue。