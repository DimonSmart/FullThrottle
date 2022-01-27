namespace FullThrottle
{
    public class RunnerBuilder<T>
    {
        private readonly IEnumerable<T> _source;
        private readonly Action<T, int> _action;
        private Action<T, int>? _onSuccess = null;
        private Action<T, Exception, int>? _onError = null;
        private Action<IEnumerable<TaskInfo>>? _onNext = null;
        private int _maximumRunningTasksLimit = 10;
        private int _maximumTasksPerSecondLimit = 5;
        
        public static RunnerBuilder<T> Create(IEnumerable<T> source, Action<T, int> action)
        {
            return new RunnerBuilder<T>(source, action);
        }

        private RunnerBuilder(IEnumerable<T> source, Action<T, int> action)
        {
            _source = source;
            _action = action;
        }

        public RunnerBuilder<T> OnError(Action<T, Exception, int> onError)
        {
            _onError = onError;
            return this;
        }

        public RunnerBuilder<T> OnSuccess(Action<T, int> onSuccess)
        {
            _onSuccess = onSuccess;
            return this;
        }

        public RunnerBuilder<T> WithDiagnosticInfo(Action<IEnumerable<TaskInfo>> onNext)
        {
            _onNext = onNext;
            return this;
        }

        public RunnerBuilder<T> WithMaximumRunningTasksLimit(int maximumRunningTasksLimit)
        {
            if (maximumRunningTasksLimit <= 0) { throw new ArgumentOutOfRangeException(nameof(maximumRunningTasksLimit)); }
            _maximumRunningTasksLimit = maximumRunningTasksLimit;
            return this;
        }

        public RunnerBuilder<T> WithMaximumTasksPerSecondLimit(int maximumTasksPerSecondLimit)
        {
            if (maximumTasksPerSecondLimit <= 0) { throw new ArgumentOutOfRangeException(nameof(maximumTasksPerSecondLimit)); }
            _maximumTasksPerSecondLimit = maximumTasksPerSecondLimit;
            return this;
        }

        public Runner<T> Build()
        {
            var runner = new Runner<T>(_source, _action, _onSuccess, _onError, _maximumRunningTasksLimit, _maximumTasksPerSecondLimit);
            runner.OnNextForDebugOnly = _onNext;
            return runner;
        }
    }
}