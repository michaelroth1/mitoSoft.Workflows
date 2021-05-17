using mitoSoft.Graphs;

namespace mitoSoft.StateMachine.Extensions
{
    public static class StateMachineExtensions
    {
        public static DirectedGraph ToDirectedGraph(this Workflows.StateMachine stateMachine)
        {
            var directedGraph = new DirectedGraph();
            foreach (var state in stateMachine.Nodes)
            {
                directedGraph.AddNode(state.Name);
            }

            foreach (var edge in stateMachine.Edges)
            {
                directedGraph.AddEdge(edge.Source.Name, edge.Target.Name, 1, false);
            }

            return directedGraph;
        }
    }
}