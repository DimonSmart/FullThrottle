namespace FullThrottle
{
    public class Runner<T>
    {
        public Action<IEnumerable<TaskInfo>>? OnNextForDebugOnly;

        private IList<TaskInfo> runnningTasks = new List<TaskInfo>();
        private readonly int _maximumRunningTasksLimit = 10;
        private readonly int _maximumTasksPerSecondLimit = 4;
        private readonly IEnumerable<T> _source;
        private readonly Action<T, int> _action;
        private readonly Action<T, int>? _onSuccess;
        private readonly Action<T, Exception, int>? _onError;


        public Runner(IEnumerable<T> source, Action<T, int> action, Action<T, int>? onSuccess,
            Action<T, Exception, int>? onError, int maximumRunningTasksLimit, int maximumTasksPerSecondLimit)
        {
            _source = source;
            _action = action;
            _onSuccess = onSuccess;
            _onError = onError;
            _maximumRunningTasksLimit = maximumRunningTasksLimit;
            _maximumTasksPerSecondLimit = maximumTasksPerSecondLimit;
        }

        public void Run(CancellationToken cancellationToken)
        {
            var enumerator = _source.GetEnumerator();
            var lineNumber = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var now = DateTime.Now;

                OnNextForDebugOnly?.Invoke(runnningTasks);

                if (CanRun())
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    lineNumber++;
                    RunTask(enumerator.Current, lineNumber, cancellationToken);
                }
                else
                {
                    var oldestTaskRunningTime = runnningTasks.Select(i => (int)(now - i.StartTime).TotalMilliseconds).Where(i => i < 1000).OrderByDescending(i => i).FirstOrDefault();
                    var allTasks = runnningTasks.Select(i => i.Task).ToArray();
                    var idx = Task.WaitAny(allTasks, 1000 - oldestTaskRunningTime, cancellationToken);
                    runnningTasks = runnningTasks.Where(i => !i.Task.IsCompleted).ToList();
                }
            }
            Task.WaitAll(runnningTasks.Select(i => i.Task).ToArray(), cancellationToken);
        }

        void RunTask(T line, int lineNumber, CancellationToken cancellationToken)
        {
            var t = new Task(() =>
            {
                try
                {
                    _action(line, lineNumber);
                    _onSuccess?.Invoke(line, lineNumber);
                }
                catch (Exception exception)
                {
                    _onError?.Invoke(line, exception, lineNumber);
                    throw;
                }

            }, cancellationToken); 
            var taskInfo = new TaskInfo(t);
            runnningTasks.Add(taskInfo);
            t.Start(TaskScheduler.Default);
        }

        bool CanRun()
        {
            var now = DateTime.Now;
            if (runnningTasks.Count(i => !i.Task.IsCompleted) < _maximumRunningTasksLimit &&
                runnningTasks.Count(i => (now - i.StartTime).TotalMilliseconds < 1000) < _maximumTasksPerSecondLimit)
            {
                return true;
            }
            return false;
        }
    }

    public class TaskInfo
    {
        public TaskInfo(Task task)
        {
            Task = task;
            StartTime = DateTime.Now;
        }
        public DateTime StartTime { get; set; }
        public Task Task { get; set; }
    }
}