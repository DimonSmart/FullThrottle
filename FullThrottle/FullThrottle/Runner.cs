using static FullThrottle.RunningTasksTimeUtils;

namespace FullThrottle
{
    public class Runner<T>
    {
        private readonly RunnerParameters<T> _parameters;

        internal Runner(RunnerParameters<T> parameters)
        {
            _parameters = parameters;
        }

        public void Run(IEnumerable<T> source, CancellationToken cancellationToken)
        {
            List<TaskInfo> runningTasks = new();
            using (var enumerator = source.GetEnumerator())
            {
                var lineNumber = 0;
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var now = DateTime.Now;

                    if (CanRun(runningTasks))
                    {
                        if (!enumerator.MoveNext())
                        {
                            break;
                        }
                        lineNumber++;
                        runningTasks.Add(RunTask(enumerator.Current, lineNumber, cancellationToken));
                        Debug(runningTasks, "Task run");
                    }
                    else
                    {
                        runningTasks = WaitNextIteration(runningTasks, now, cancellationToken);
                    }
                }
            }
            Task.WaitAll(runningTasks.Select(i => i.Task).ToArray(), cancellationToken);
            Debug(runningTasks, "Tail finish wait");
        }

        private List<TaskInfo> WaitNextIteration(List<TaskInfo> runningTasks, DateTime now, CancellationToken cancellationToken)
        {
            var yangestTaskRunningTime = runningTasks.Select(i => now - i.StartTime).Where(i => i < _parameters.Interval).OrderByDescending(i => i).FirstOrDefault();
            var allTasks = runningTasks.Select(i => i.Task).ToArray();
            var timeToNextInterval = (int)(_parameters.Interval - yangestTaskRunningTime).TotalMilliseconds;
            Task.WaitAny(allTasks, timeToNextInterval, cancellationToken);
            runningTasks = runningTasks
                .Where(i => (now - i.StartTime) < _parameters.Interval / _parameters.MaximumTasksPerIntervalLimit || !i.Task.IsCompleted)
                .ToList();
            Debug(runningTasks, "Wait for run");
            return runningTasks;
        }

        private void Debug(List<TaskInfo> runningTasks, string s)
        {
            _parameters.OnNextForDebugOnly?.Invoke(s, runningTasks);
        }

        TaskInfo RunTask(T line, int lineNumber, CancellationToken cancellationToken)
        {
            var taskCreationOption = _parameters.UseThreadPool ? TaskCreationOptions.None : TaskCreationOptions.LongRunning;
            var t = new Task(() =>
            {
                try
                {
                    _parameters.Action(line, lineNumber);
                    _parameters.OnSuccess?.Invoke(line, lineNumber);
                }
                catch (Exception exception)
                {
                    _parameters.OnError?.Invoke(line, exception, lineNumber);
                    throw;
                }

            }, cancellationToken, taskCreationOption);
            t.Start(TaskScheduler.Default);
            return new TaskInfo(DateTime.Now, t);
        }

        bool CanRun(List<TaskInfo> runningTasks)
        {
            var now = DateTime.Now;

            var minimumTaskRunIntervalCondition = GetYoungestTaskRunningTimeOrMaximum(runningTasks, now) >= _parameters.MinimumTaskRunInterval;
            var maximumRunningTasksCondition = GetNotFinishedTasksCount(runningTasks) < _parameters.MaximumRunningTasksLimit;
            var taskStartedInTheIntervalCondition = GetLastIntervalStartedTasksCount(runningTasks, now, _parameters.Interval) < _parameters.MaximumTasksPerIntervalLimit;

            return minimumTaskRunIntervalCondition && maximumRunningTasksCondition && taskStartedInTheIntervalCondition;
        }

        private static int GetNotFinishedTasksCount(List<TaskInfo> runningTasks)
        {
            return runningTasks.Count(i => !i.Task.IsCompleted);
        }
    }

    public record TaskInfoBase(DateTime StartTime);

    public record TaskInfo(DateTime StartTime, Task Task) : TaskInfoBase(StartTime);
}