namespace FullThrottle
{
    internal static class RunningTasksTimeUtils
    {
        internal static TimeSpan GetYoungestTaskRunningTimeOrMaximum(IReadOnlyCollection<TaskInfoBase> runningTasks, DateTime now)
        {
            if (runningTasks.Count == 0)
            {
                return TimeSpan.MaxValue;
            }
            return runningTasks.OrderByDescending(i => i.StartTime).Select(i => now - i.StartTime).First();
        }

        internal static int GetLastIntervalStartedTasksCount(IReadOnlyCollection<TaskInfoBase> runningTasks, DateTime now, TimeSpan interval)
        {
            return runningTasks.Count(i => (now - i.StartTime) < interval);
        }
    }
}