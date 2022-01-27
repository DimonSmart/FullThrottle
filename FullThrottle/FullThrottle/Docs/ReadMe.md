#Full trottle

This library uses throttling to overcome the problem of excessively lengthy queue processing. Use case: You have a 100000000 line text file to parse, and you must submit a http post request to an external api server for each line. The server owner requests that you not overwhelm the server by sending more than 10 requests per second.

Many publications describe how to submit requests as quickly as possible, however this library adds the term allowed. As a result, the goal of this library is to deliver requests at the fastest possible pace in order to avoid DDOSing the target site.

# Usage example

```csharp
 var lines = new List<string> { "A", "B", "C" /* very long enumerations */ };

 var runner = RunnerBuilder<string>
     .Create(lines, (line, lineNumber) => Work(line, lineNumber))
     .OnError((line, e, lineNumber) => { Console.WriteLine($"Error. Line:{line} Number:{lineNumber} Error:{e.Message}"); })
     .OnSuccess((line, lineNumber) => { Console.WriteLine($"Finish. Line:{line} Number:{lineNumber}"); })
     .WithMaximumRunningTasksLimit(20)
     .WithMaximumTasksPerSecondLimit(5)
     .Build();
 runner.Run(CancellationToken.None);
```


