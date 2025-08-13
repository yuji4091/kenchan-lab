using System;
using System.Threading;
using Xunit;
using RfidKartTracker.Services;

namespace RfidKartTracker.Tests.Services
{
    public class DebounceFilterTests
    {
        [Fact]
        public void ShouldProcess_FirstTime_ReturnsTrue()
        {
            // Arrange
            var filter = new DebounceFilter(TimeSpan.FromSeconds(1));
            var epcTag = "E20000166021011740209049";

            // Act
            var result = filter.ShouldProcess(epcTag);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ShouldProcess_WithinInterval_ReturnsFalse()
        {
            // Arrange
            var filter = new DebounceFilter(TimeSpan.FromSeconds(1));
            var epcTag = "E20000166021011740209049";

            // Act
            filter.ShouldProcess(epcTag); // First call
            var result = filter.ShouldProcess(epcTag); // Second call within interval

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ShouldProcess_AfterInterval_ReturnsTrue()
        {
            // Arrange
            var filter = new DebounceFilter(TimeSpan.FromMilliseconds(100));
            var epcTag = "E20000166021011740209049";

            // Act
            filter.ShouldProcess(epcTag); // First call
            Thread.Sleep(150); // Wait longer than interval
            var result = filter.ShouldProcess(epcTag); // Second call after interval

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ShouldProcess_DifferentTags_ReturnsTrue()
        {
            // Arrange
            var filter = new DebounceFilter(TimeSpan.FromSeconds(1));
            var epcTag1 = "E20000166021011740209049";
            var epcTag2 = "E20000166021011740209050";

            // Act
            var result1 = filter.ShouldProcess(epcTag1);
            var result2 = filter.ShouldProcess(epcTag2);

            // Assert
            Assert.True(result1);
            Assert.True(result2);
        }

        [Fact]
        public void GetLastScanTime_ExistingTag_ReturnsTime()
        {
            // Arrange
            var filter = new DebounceFilter();
            var epcTag = "E20000166021011740209049";
            
            // Act
            filter.ShouldProcess(epcTag);
            var lastScanTime = filter.GetLastScanTime(epcTag);

            // Assert
            Assert.NotNull(lastScanTime);
            Assert.True(lastScanTime.Value > DateTime.MinValue);
        }

        [Fact]
        public void GetLastScanTime_NonExistingTag_ReturnsNull()
        {
            // Arrange
            var filter = new DebounceFilter();
            var epcTag = "NonExistingTag";

            // Act
            var lastScanTime = filter.GetLastScanTime(epcTag);

            // Assert
            Assert.Null(lastScanTime);
        }

        [Fact]
        public void Clear_RemovesAllRecords()
        {
            // Arrange
            var filter = new DebounceFilter();
            var epcTag = "E20000166021011740209049";
            
            filter.ShouldProcess(epcTag);
            Assert.NotNull(filter.GetLastScanTime(epcTag));

            // Act
            filter.Clear();

            // Assert
            Assert.Null(filter.GetLastScanTime(epcTag));
        }

        [Fact]
        public void TagIgnored_EventTriggered_WhenWithinInterval()
        {
            // Arrange
            var filter = new DebounceFilter(TimeSpan.FromSeconds(1));
            var epcTag = "E20000166021011740209049";
            var eventTriggered = false;
            string? ignoredTag = null;

            filter.TagIgnored += (tag) => 
            {
                eventTriggered = true;
                ignoredTag = tag;
            };

            // Act
            filter.ShouldProcess(epcTag); // First call
            filter.ShouldProcess(epcTag); // Second call within interval

            // Assert
            Assert.True(eventTriggered);
            Assert.Equal(epcTag, ignoredTag);
        }
    }
}