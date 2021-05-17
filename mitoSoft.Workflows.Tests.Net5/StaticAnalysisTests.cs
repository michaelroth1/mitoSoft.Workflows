using Microsoft.VisualStudio.TestTools.UnitTesting;
using mitoSoft.Graphs.Analysis;
using mitoSoft.StateMachine.Extensions;
using System.Diagnostics;
using System.Linq;

namespace mitoSoft.Workflows.Tests.FullFramework
{
    [TestClass]
    public class StaticAnalysisTests
    {
        [TestMethod]
        public void Standard1()
        {
            var stateMachine = new StateMachine()
               .AddNode(new State("Start", () => Debug.WriteLine("Start")))
               .AddNode(new State("State1", () => Debug.WriteLine("State1")))
               .AddNode(new State("State2", () => Debug.WriteLine("State2")))
               .AddNode(new State("End", () => Debug.WriteLine("End")))
               .AddEdge("Start", "State1", () => { return true; })
               .AddEdge("Start", "State2", () => { return true; })
               .AddEdge("State2", "End", () => { return true; });

            var directedGraph = stateMachine.ToDirectedGraph();
            var shortestGraph = directedGraph.ToShortestGraph("Start", "End");

            Assert.AreEqual(3, shortestGraph.Nodes.Count());
        }
    }
}
