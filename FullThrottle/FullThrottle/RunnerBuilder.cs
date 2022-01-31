namespace FullThrottle
{
    public class RunnerBuilder<T>
    {
        private RunnerParameters<T> _parameters;

        public static RunnerBuilder<T> Create(Action<T, int> action)
        {
            return new RunnerBuilder<T>(action);
        }

        /// <summary>
        /// Create a RunnerBuilder with action as a mandatory argument 
        /// </summary>
        /// <param name="action">A source element handler</param>
        private RunnerBuilder(Action<T, int> action)
        {
            _parameters = new RunnerParameters<T>(action);
        }

        /// <summary>
        /// In case of action produce an error we could handle this error here
        /// For examle we could store error messages somewhere
        /// Note. This method should be threadsafe
        /// </summary>
        /// <param name="onError">T - handledo object, Exception, int - line number from source</param>
        /// <returns></returns>
        public RunnerBuilder<T> OnError(Action<T, Exception, int> onError)
        {
            _parameters.OnError = onError;
            return this;
        }

        /// <summary>
        /// After successful action execution this method called
        /// </summary>
        /// <param name="onSuccess"></param>
        /// <returns></returns>
        public RunnerBuilder<T> OnSuccess(Action<T, int> onSuccess)
        {
            _parameters.OnSuccess = onSuccess;
            return this;
        }

        /// <summary>
        /// Internal method for Runner debugging purpose
        /// Should not be used on production
        /// </summary>
        /// <param name="onNext"></param>
        /// <returns></returns>
        public RunnerBuilder<T> WithDiagnosticInfo(Action<string, IEnumerable<TaskInfo>> onNext)
        {
            _parameters.OnNextForDebugOnly = onNext;
            return this;
        }

        /// <summary>
        /// Limit simultaneously started tasks
        /// Note! As task ruuner use a ThreadPool this limit could limit queued tasks
        /// If you want to have a real task use ForceNewThreadCreation
        /// </summary>
        /// <param name="maximumRunningTasksLimit"></param>
        /// <returns></returns>
        /// <see cref="ForceNewThreadCreation"/>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public RunnerBuilder<T> WithMaximumRunningTasksLimit(int maximumRunningTasksLimit)
        {
            if (maximumRunningTasksLimit <= 0) { throw new ArgumentOutOfRangeException(nameof(maximumRunningTasksLimit)); }
            _parameters.MaximumRunningTasksLimit = maximumRunningTasksLimit;
            return this;
        }

        /// <summary>
        /// Limit maximum tasks count started in one second
        /// </summary>
        /// <param name="maximumTasksPerSecondLimit"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public RunnerBuilder<T> WithMaximumTasksPerSecondLimit(int maximumTasksPerSecondLimit)
        {
            if (maximumTasksPerSecondLimit <= 0) { throw new ArgumentOutOfRangeException(nameof(maximumTasksPerSecondLimit)); }
            _parameters.MaximumTasksPerIntervalLimit = maximumTasksPerSecondLimit;
            _parameters.Interval = TimeSpan.FromSeconds(1);
            return this;
        }

        /// <summary>
        /// Limit the maximum tasks count start in the specified interval
        /// Usefull for long and slow operations.
        /// This method allows us to run 6 tasks per minute
        /// </summary>
        /// <param name="maximumTasksPerIntervalLimit"></param>
        /// <param name="interval"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public RunnerBuilder<T> WithMaximumTasksPerIntervalLimit(int maximumTasksPerIntervalLimit, TimeSpan interval)
        {
            if (maximumTasksPerIntervalLimit <= 0) { throw new ArgumentOutOfRangeException(nameof(maximumTasksPerIntervalLimit)); }
            if (interval <= TimeSpan.Zero) { throw new ArgumentOutOfRangeException(nameof(interval)); }
            _parameters.MaximumTasksPerIntervalLimit = maximumTasksPerIntervalLimit;
            _parameters.Interval = interval;
            return this;
        }

        public RunnerBuilder<T> WithMinimumTaskRunInterval(TimeSpan minimumTaskRunInterval)
        {
            _parameters.MinimumTaskRunInterval = minimumTaskRunInterval;
            return this;
        }

        /// <summary>
        /// Set the minimum interval between running tasks to 1 second / maximumTasksPerSecondLimit
        /// It could be useful to not start too much tasks simultaniously.
        /// Example: if you run no more then 4 tasks per second there are two options:
        /// 1. Start all 4 tasks at the beginning of current second
        /// 2. Spread 4 tasks by second with 1/4 second interval
        /// </summary>
        /// <see cref="WithMinimumTaskRunInterval"/>
        /// <returns>RunnerBuilder</returns>
        public RunnerBuilder<T> RespectMinimumTaskRunInterval()
        {
            _parameters.MinimumTaskRunInterval = TimeSpan.FromMilliseconds(1000.0 / _parameters.MaximumTasksPerIntervalLimit);
            return this;
        }

        /// <summary>
        /// By default Starts task method, scheduling it for execution to the current TaskScheduler
        /// In case you want to force creating new thread you could use this method.
        /// Underhood it is equivalent LongRunningTask options
        /// </summary>
        public RunnerBuilder<T> ForceNewThreadCreation()
        {
            _parameters.UseThreadPool = false;
            return this;
        }

        public Runner<T> Build()
        {
            return new Runner<T>(_parameters);
        }
    }
}