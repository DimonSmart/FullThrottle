using FullThrottle;
using System.Text;

namespace FullThrottleConsole
{
    public class Program
    {
        private static Random _random = new();

        public static void Main()
        {
            Console.WriteLine("Hello, World!");
            IEnumerable<string> lines = GetSource();

            var runner = RunnerBuilder<string>
                .Create((line, lineNumber) => Work(line, lineNumber))
                .OnError((line, e, lineNumber) => { Console.WriteLine($"Error. Line:{line} Number:{lineNumber} Error:{e.Message}"); })
                .WithMaximumRunningTasksLimit(2)
                .WithMaximumTasksPerIntervalLimit(2, TimeSpan.FromSeconds(5))
                .WithMinimumTaskRunInterval(TimeSpan.FromSeconds(2))
                .WithDiagnosticInfo(LogIt)
                .ForceNewThreadCreation()
                .Build();
            runner.Run(lines, CancellationToken.None);
        }

        private static IEnumerable<string> GetSource()
        {
            return Enumerable.Range('a', 'z' - 'a' + 1).Union(Enumerable.Range('A', 'Z' - 'A' + 1)).Select(i => ((char)i).ToString());
        }

        private static void Work(string line, int lineNumber)
        {
            Console.WriteLine($"Work start. Line:{line} Number:{lineNumber}");
            var delaySeconds = 5 + _random.Next(4);
            Thread.Sleep(TimeSpan.FromSeconds(delaySeconds));
            Console.WriteLine($"Work. Finish. Line:{line} Number:{lineNumber}");
        }

        private static void LogIt(string message, IEnumerable<TaskInfo> tasks)
        {
            var now = DateTime.Now;
            var sb = new StringBuilder();
            foreach (var element in Enum.GetNames(typeof(TaskStatus)))
            {
                TaskStatus s = (TaskStatus)Enum.Parse(typeof(TaskStatus), element);
                sb.Append($"{element}:  {tasks.Count(i => i.Task.Status == s)}, ");
            }

            if (tasks.TryGetNonEnumeratedCount(out int count))
            {
                Console.WriteLine($"{message} {count} {sb}");
            }
        }
    }
}