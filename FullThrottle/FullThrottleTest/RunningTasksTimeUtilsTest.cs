using FullThrottle;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;
using static FullThrottle.RunningTasksTimeUtils;

namespace FullThrottleTest
{
    public class RunningTasksTimeUtilsTest
    {
        private readonly ITestOutputHelper _output;
        DateTime _now = new DateTime(2000, 1, 1, 0, 0, 0, 0);

        public RunningTasksTimeUtilsTest(ITestOutputHelper output)
        {
            _output = output;
        }


        [Fact]
        public void GetYoungestTaskRunningTimeOrMaximum_ShouldReturnMaxTime_OnEmptyCollection()
        {
            var time = GetYoungestTaskRunningTimeOrMaximum(Array.Empty<TaskInfoBase>(), _now);
            Assert.Equal(TimeSpan.MaxValue, time);
        }

        [Fact]
        public void GetLastIntervalStartedTasksCount_ShouldReturnZero_OnEmptyCollection()
        {
            Assert.Equal(0, GetLastIntervalStartedTasksCount(Array.Empty<TaskInfoBase>(), _now, TimeSpan.Zero));
        }


        [Fact]
        public void GetLastIntervalStartedTasksCount_ShouldReturnZero_OnOnCollectionWithTasksOutOfInterval()
        {
            Assert.Equal(0,
                GetLastIntervalStartedTasksCount(
                    new List<TaskInfoBase>
                    {
                        new TaskInfoBase(_now),
                        new TaskInfoBase(_now.AddSeconds(-1)),
                    }, _now, TimeSpan.Zero));
        }

        [Fact]
        public void GetLastIntervalStartedTasksCount_ShouldReturnTwo_OnOnCollectionWithTwoTasksInInterval()
        {
            Assert.Equal(2,
                GetLastIntervalStartedTasksCount(
                    new List<TaskInfoBase>
                    {
                        new TaskInfoBase(_now),  // First
                        new TaskInfoBase(_now.AddSeconds(-1)), // Second
                        new TaskInfoBase(_now.AddSeconds(-2)), // Out of interval
                        new TaskInfoBase(_now.AddSeconds(-3))  // Out of interval
                    }, _now, TimeSpan.FromSeconds(1.5)));
        }
    }
}