using mitoSoft.Graphs;

namespace mitoSoft.StateMachine.Extensions
{
    public static class StateMachineExtensions
    {
        /// <summary>
        /// Converts a atate machine to a directed graph.
        /// This allows the usage of all analysis functions of 'mitoSoft.Graphs.Analysis'.
        /// </summary>
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
                var graphEdge = directedGraph.GetEdge(edge.Source.Name, edge.Target.Name);
                graphEdge.Description = edge.Description;
            }

            return directedGraph;
        }
    }
}