using FullThrottle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace FullThrottleTest
{
    public class RunnerBuilderTest
    {
        private readonly ITestOutputHelper output;

        public RunnerBuilderTest(ITestOutputHelper output)
        {
            this.output = output;
        }


        [Fact]
        public void SimpleRunnerBuilder()
        {
            var runner = RunnerBuilder<string>
                .Create(Source, (line, lineNumber) => { return; })
                .Build();
            runner.Run(CancellationToken.None);
        }

        private static readonly IEnumerable<string> Source = new[] { "A", "B", "C" };
    }
}