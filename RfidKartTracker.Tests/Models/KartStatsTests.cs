using System;
using Xunit;
using RfidKartTracker.Models;

namespace RfidKartTracker.Tests.Models
{
    public class KartStatsTests
    {
        [Fact]
        public void KartStats_Constructor_SetsPropertiesCorrectly()
        {
            // Arrange
            var kartNumber = "001";
            var epcTag = "E20000166021011740209049";

            // Act
            var kartStats = new KartStats(kartNumber, epcTag);

            // Assert
            Assert.Equal(kartNumber, kartStats.KartNumber);
            Assert.Equal(epcTag, kartStats.EpcTag);
            Assert.Equal(0, kartStats.LapCount);
            Assert.True(kartStats.IsActive);
            Assert.True(kartStats.LastRecognitionTime > DateTime.MinValue);
        }

        [Fact]
        public void IncrementLap_IncreasesLapCountAndUpdatesTime()
        {
            // Arrange
            var kartStats = new KartStats("001", "EPC001");
            var initialTime = kartStats.LastRecognitionTime;

            // Act
            kartStats.IncrementLap();

            // Assert
            Assert.Equal(1, kartStats.LapCount);
            Assert.True(kartStats.LastRecognitionTime > initialTime);
        }

        [Fact]
        public void IncrementLap_MultipleCalls_IncreasesCorrectly()
        {
            // Arrange
            var kartStats = new KartStats("001", "EPC001");

            // Act
            kartStats.IncrementLap();
            kartStats.IncrementLap();
            kartStats.IncrementLap();

            // Assert
            Assert.Equal(3, kartStats.LapCount);
        }

        [Fact]
        public void TimeSinceLastScan_ReturnsCorrectTimeSpan()
        {
            // Arrange
            var kartStats = new KartStats("001", "EPC001");
            
            // Act
            var timeSince = kartStats.TimeSinceLastScan;

            // Assert
            Assert.True(timeSince.TotalMilliseconds >= 0);
            Assert.True(timeSince.TotalMinutes < 1); // Should be very recent
        }

        [Fact]
        public void ToString_ReturnsFormattedString()
        {
            // Arrange
            var kartStats = new KartStats("001", "EPC001");
            kartStats.IncrementLap();

            // Act
            var result = kartStats.ToString();

            // Assert
            Assert.Contains("Kart: 001", result);
            Assert.Contains("Laps: 1", result);
            Assert.Contains("Last:", result);
        }
    }
}