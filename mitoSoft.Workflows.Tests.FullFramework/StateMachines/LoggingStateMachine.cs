using System;
using System.Collections.Generic;
using System.Threading;

namespace mitoSoft.Workflows.Tests.FullFramework
{
    [Serializable]
    internal class LoggingStateMachine : StateMachine
    {
        public LoggingStateMachine(string name, int sleep, List<string> logger)
        {
            this.AddNode(new State("Start", () =>
            {
                Thread.Sleep(sleep);
                logger.Add($"{name}.Start");
            }))
            .AddNode(new State("State1", () => logger.Add($"{name}.State1")))
            .AddNode(new State("State2", () =>
            {
                logger.Add($"{name}.State2");
            }))
            .AddNode(new State($"End", () => logger.Add($"{name}.End")))
            .AddEdge("Start", "State1", () => { return true; })
            .AddEdge("State1", "State2", () => { return true; })
            .AddEdge("State2", "End", () => { return true; });
        }
    }
}