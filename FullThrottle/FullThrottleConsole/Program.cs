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

            var lines = new List<string> { "A", "B", "C", "D", "E", "F", "G", "H", "I", "A", "B", "C", "D", "E", "F", "G", "H", "I", "A", "B", "C", "D", "E", "F", "G", "H", "I", "A", "B", "C", "D", "E", "F", "G", "H", "I", "A", "B", "C", "D", "E", "F", "G", "H", "I", "A", "B", "C", "D", "E", "F", "G", "H", "I", "A", "B", "C", "D", "E", "F", "G", "H", "I", "A", "B", "C", "D", "E", "F", "G", "H", "I", "A", "B", "C", "D", "E", "F", "G", "H", "I", "A", "B", "C", "D", "E", "F", "G", "H", "I", "A", "B", "C", "D", "E", "F", "G", "H", "I", "A", "B", "C", "D", "E", "F", "G", "H", "I" };

            var runner = RunnerBuilder<string>
                .Create(lines, (line, lineNumber) => Work(line, lineNumber))
                .OnError((line, e, lineNumber) => { Console.WriteLine($"Error. Line:{line} Number:{lineNumber} Error:{e.Message}"); })
                .OnSuccess((line, lineNumber) => { Console.WriteLine($"Finish. Line:{line} Number:{lineNumber}"); })
                .WithMaximumRunningTasksLimit(20)
                .WithMaximumTasksPerSecondLimit(5)
                .Build();
            runner.Run(CancellationToken.None);
        }
        static void Work(string line, int lineNumber)
        {
            Console.WriteLine($"Work start. Line:{line} Number:{lineNumber}");
            var delaySeconds = 5 + _random.Next(5);
            Thread.Sleep(TimeSpan.FromSeconds(delaySeconds));
            Console.WriteLine($"Work. Finish. Line:{line} Number:{lineNumber}");
        }

        private static void LogIt(IEnumerable<TaskInfo> tasks)
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
                Console.WriteLine($"{count} {sb}");
            }
        }
    }
}