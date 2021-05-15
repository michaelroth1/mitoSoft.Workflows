using mitoSoft.Graphs;
using mitoSoft.Graphs.Exceptions;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace mitoSoft.Workflows
{
    [DebuggerDisplay(nameof(StateMachine) + " ({ToString()})")]
    public class StateMachine : Graph<State, Transition>
    {
        /// <summary>
        /// Start state of the state machine
        /// </summary>
        public State Start { get; set; }

        /// <summary>
        /// Invokes the state machine
        /// </summary>
        public void Invoke()
        {
            this.Start.Execute();
        }

        public Task InvokeAsyn()
        {
            return Task.Run(() =>
            {
                this.Invoke();
            });
        }

        /// <summary>
        /// Add a node to the state machine.
        /// </summary>
        public new StateMachine AddNode(State node)
        {
            if (this.Start == null)
            {
                this.Start = node;
            }

            base.AddNode(node);

            return this;
        }

        /// <summary>
        /// Add an edge to the state machine that connects the source node, given by the 'sourceName',
        /// and the target node, given by the 'targetName'.
        /// </summary>
        public StateMachine AddEdge(string sourceName, string targetName, Condition condition)
        {
            if (!this.TryAddEdge(sourceName, targetName, condition, out _))
            {
                throw new EdgeAlreadyExistingException(sourceName, targetName);
            }

            return this;
        }

        /// <summary>
        /// Add an edge to the state machine that connects the 'source' node and the 'target' node.
        /// </summary>
        public StateMachine AddEdge(State source, State target, Condition condition)
        {
            if (!this.TryAddEdge(source, target, condition, out _))
            {
                throw new EdgeAlreadyExistingException(source.Name, target.Name);
            }

            return this;
        }

        public new StateMachine AddEdge(Transition edge) => (StateMachine)base.AddEdge(edge);

        /// <summary>
        /// Tries to add an edge to the state machine that connects the source node, given bv the 'sourceName',
        /// and the target node, given by the 'targetName'.
        /// </summary>
        /// <returns>True when the edge was actually added or false when an existing edge already exists.</returns>
        public bool TryAddEdge(string sourceName, string targetName, Condition condition, out Transition edge)
        {
            try
            {
                var source = this.GetNode(sourceName);
                var target = this.GetNode(targetName);

                return this.TryAddEdge(source, target, condition, out edge);
            }
            catch (NodeNotFoundException)
            {
                edge = null;
                return false;
            }
        }

        /// <summary>
        /// Tries to add an edge to the state machine that connects the 'sourceNode' and the 'targetNode'.
        /// </summary>
        /// <returns>True when the edge was actually added or false when an existing edge already exists.</returns>
        public bool TryAddEdge(State source, State target, Condition condition, out Transition edge)
        {
            try
            {
                edge = this.GetEdge(source, target);
                return false;
            }
            catch (EdgeNotFoundException)
            {
                edge = new Transition(source, target, condition);
                source.AddEdge(edge);
                return true;
            }
            catch (System.Exception)
            {
                edge = null;
                return false;
            }
        }
    }
}