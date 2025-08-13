using System.Linq;
using Xunit;
using RfidKartTracker.Models;
using RfidKartTracker.Services;

namespace RfidKartTracker.Tests.Services
{
    public class LapCounterServiceTests
    {
        [Fact]
        public void ProcessScan_ValidEpc_IncreasesLapCount()
        {
            // Arrange
            var lapCounter = new LapCounterService();
            var scanEvent = new RfidScanEvent("E20000166021011740209049"); // Kart 001

            // Act
            lapCounter.ProcessScan(scanEvent);
            var kartStats = lapCounter.GetKartStats("001");

            // Assert
            Assert.NotNull(kartStats);
            Assert.Equal(1, kartStats.LapCount);
            Assert.Equal("001", kartStats.KartNumber);
        }

        [Fact]
        public void ProcessScan_UnknownEpc_TriggersUnknownTagEvent()
        {
            // Arrange
            var lapCounter = new LapCounterService();
            var scanEvent = new RfidScanEvent("UnknownEPC");
            var eventTriggered = false;
            string? unknownTag = null;

            lapCounter.UnknownTagScanned += (tag) => 
            {
                eventTriggered = true;
                unknownTag = tag;
            };

            // Act
            lapCounter.ProcessScan(scanEvent);

            // Assert
            Assert.True(eventTriggered);
            Assert.Equal("UnknownEPC", unknownTag);
        }

        [Fact]
        public void ProcessScan_MultipleLaps_CountsCorrectly()
        {
            // Arrange
            var debounceFilter = new DebounceFilter(TimeSpan.FromMilliseconds(10));
            var lapCounter = new LapCounterService(debounceFilter);
            var scanEvent = new RfidScanEvent("E20000166021011740209049"); // Kart 001

            // Act
            lapCounter.ProcessScan(scanEvent);
            Thread.Sleep(20); // Wait for debounce
            lapCounter.ProcessScan(scanEvent);
            Thread.Sleep(20);
            lapCounter.ProcessScan(scanEvent);

            var kartStats = lapCounter.GetKartStats("001");

            // Assert
            Assert.NotNull(kartStats);
            Assert.Equal(3, kartStats.LapCount);
        }

        [Fact]
        public void ProcessScan_WithDebounce_IgnoresDuplicates()
        {
            // Arrange
            var debounceFilter = new DebounceFilter(TimeSpan.FromSeconds(1));
            var lapCounter = new LapCounterService(debounceFilter);
            var scanEvent = new RfidScanEvent("E20000166021011740209049"); // Kart 001

            // Act
            lapCounter.ProcessScan(scanEvent);
            lapCounter.ProcessScan(scanEvent); // Should be ignored
            lapCounter.ProcessScan(scanEvent); // Should be ignored

            var kartStats = lapCounter.GetKartStats("001");

            // Assert
            Assert.NotNull(kartStats);
            Assert.Equal(1, kartStats.LapCount); // Only first scan counted
        }

        [Fact]
        public void AddKartMapping_NewMapping_AddsCorrectly()
        {
            // Arrange
            var lapCounter = new LapCounterService();
            var epcTag = "NewEPC";
            var kartNumber = "999";

            // Act
            lapCounter.AddKartMapping(epcTag, kartNumber);
            var scanEvent = new RfidScanEvent(epcTag);
            lapCounter.ProcessScan(scanEvent);

            var kartStats = lapCounter.GetKartStats(kartNumber);

            // Assert
            Assert.NotNull(kartStats);
            Assert.Equal(kartNumber, kartStats.KartNumber);
            Assert.Equal(1, kartStats.LapCount);
        }

        [Fact]
        public void GetAllKartStats_ReturnsAllActiveKarts()
        {
            // Arrange
            var lapCounter = new LapCounterService();
            var scanEvent1 = new RfidScanEvent("E20000166021011740209049"); // Kart 001
            var scanEvent2 = new RfidScanEvent("E20000166021011740209050"); // Kart 002

            // Act
            lapCounter.ProcessScan(scanEvent1);
            lapCounter.ProcessScan(scanEvent2);
            var allStats = lapCounter.GetAllKartStats().ToList();

            // Assert
            Assert.Equal(2, allStats.Count);
            Assert.Contains(allStats, s => s.KartNumber == "001");
            Assert.Contains(allStats, s => s.KartNumber == "002");
        }

        [Fact]
        public void ResetAllStats_ClearsAllData()
        {
            // Arrange
            var lapCounter = new LapCounterService();
            var scanEvent = new RfidScanEvent("E20000166021011740209049"); // Kart 001
            
            lapCounter.ProcessScan(scanEvent);
            Assert.NotNull(lapCounter.GetKartStats("001"));

            // Act
            lapCounter.ResetAllStats();

            // Assert
            Assert.Null(lapCounter.GetKartStats("001"));
            Assert.Empty(lapCounter.GetAllKartStats());
        }

        [Fact]
        public void ResetKartStats_ResetsSpecificKart()
        {
            // Arrange
            var lapCounter = new LapCounterService();
            var scanEvent = new RfidScanEvent("E20000166021011740209049"); // Kart 001
            
            lapCounter.ProcessScan(scanEvent);
            lapCounter.ProcessScan(scanEvent);
            
            var kartStats = lapCounter.GetKartStats("001");
            Assert.NotNull(kartStats);
            var initialTime = kartStats.LastRecognitionTime;

            // Act
            Thread.Sleep(10); // Ensure time difference
            lapCounter.ResetKartStats("001");

            // Assert
            kartStats = lapCounter.GetKartStats("001");
            Assert.NotNull(kartStats);
            Assert.Equal(0, kartStats.LapCount);
            Assert.True(kartStats.LastRecognitionTime > initialTime);
        }

        [Fact]
        public void LapCounted_EventTriggered_WhenLapIncremented()
        {
            // Arrange
            var lapCounter = new LapCounterService();
            var scanEvent = new RfidScanEvent("E20000166021011740209049"); // Kart 001
            var eventTriggered = false;
            KartStats? eventKartStats = null;

            lapCounter.LapCounted += (stats) => 
            {
                eventTriggered = true;
                eventKartStats = stats;
            };

            // Act
            lapCounter.ProcessScan(scanEvent);

            // Assert
            Assert.True(eventTriggered);
            Assert.NotNull(eventKartStats);
            Assert.Equal("001", eventKartStats.KartNumber);
            Assert.Equal(1, eventKartStats.LapCount);
        }
    }
}