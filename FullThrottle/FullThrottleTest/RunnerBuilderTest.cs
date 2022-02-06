using FullThrottle;
using System.Collections.Generic;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace FullThrottleTest
{
    public class RunnerBuilderTest
    {
        private readonly ITestOutputHelper _output;

        public RunnerBuilderTest(ITestOutputHelper output)
        {
            _output = output;
        }


        [Fact]
        public void SimpleRunnerBuilder()
        {
            var runner = RunnerBuilder<string>
                .Create((line, lineNumber) => { return; })
                .Build();

            runner.Run(Source, CancellationToken.None);
        }

        [Fact]
        public void SimpleRunnerBuildAndRunTwice()
        {
            var runner = RunnerBuilder<string>
                .Create((line, lineNumber) => { return; })
                .Build();

            runner.Run(Source, CancellationToken.None);
            runner.Run(Source, CancellationToken.None);
        }


        private static readonly IEnumerable<string> Source = new[] { "A", "B", "C" };
    }
}