#Full trottle

This library uses throttling to overcome the problem of excessively lengthy queue processing. Use case: You have a 100000000 line text file to parse, and you must submit a http post request to an external api server for each line. The server owner requests that you not overwhelm the server by sending more than 10 requests per second.

Many publications describe how to submit requests as quickly as possible, however this library adds the term allowed. As a result, the goal of this library is to deliver requests at the fastest possible pace in order to avoid DDOSing the target site.

# Usage example
## Minimum example
```csharp
 var lines = new List<string> { "A", "B", "C" /* very long enumerations */ };

 var runner = RunnerBuilder<string>
     .Create((line, lineNumber) => Work(line, lineNumber))
     .Build();

 runner.Run(lines, CancellationToken.None);
```

## Maximum example
```csharp
 var lines = new List<string> { "A", "B", "C" /* very long enumerations */ };

 var runner = RunnerBuilder<string>
     .Create((line, lineNumber) => Work(line, lineNumber))
     .OnError((line, e, lineNumber) => { Console.WriteLine($"Error. Line:{line} Number:{lineNumber} Error:{e.Message}"); })
     .OnSuccess((line, e, lineNumber) => { Console.WriteLine($"Success. Line:{line} Number:{lineNumber} Error:{e.Message}"); })
     .WithMaximumRunningTasksLimit(20)
     .WithMaximumTasksPerIntervalLimit(10, TimeSpan.FromSeconds(5))
     .WithMinimumTaskRunInterval(TimeSpan.FromMilliseconds(250))
     .WithDiagnosticInfo(LogIt)
     .ForceNewThreadCreation()
     .Build();

 runner.Run(lines, CancellationToken.None);

// .WithDiagnosticInfo
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
```
