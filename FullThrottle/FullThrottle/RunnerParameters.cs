namespace FullThrottle
{
    public struct RunnerParameters<T>
    {
        public Action<string, IEnumerable<TaskInfo>>? OnNextForDebugOnly;
        public int MaximumRunningTasksLimit;
        public int MaximumTasksPerIntervalLimit;
        public TimeSpan Interval;
        public TimeSpan MinimumTaskRunInterval;

        public Action<T, int> Action;
        public Action<T, int>? OnSuccess;
        public Action<T, Exception, int>? OnError;

        public bool UseThreadPool;

        public RunnerParameters(Action<T, int> action)
        {
            Interval = TimeSpan.FromSeconds(1);
            Action = action;
            MaximumRunningTasksLimit = 10;
            MaximumTasksPerIntervalLimit = 4;
            OnNextForDebugOnly = null;
            OnSuccess = null;
            OnError = null;
            UseThreadPool = true;
            MinimumTaskRunInterval = TimeSpan.Zero;
        }
    }
}