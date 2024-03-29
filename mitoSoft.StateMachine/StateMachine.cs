﻿using mitoSoft.Graphs;
using mitoSoft.Graphs.Exceptions;
using mitoSoft.Workflows.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace mitoSoft.Workflows
{
    [Serializable]
    [DebuggerDisplay(nameof(StateMachine) + " ({ToString()})")]
    public class StateMachine : Graph<State, Transition>
    {
        //
        private List<State> _callStack = new List<State>();

        /// <summary>
        /// Start state of the state machine
        /// </summary>
        public State Start { get; set; }

        /// <summary>
        /// Actually activated state of the state machine
        /// </summary>
        public State Activated { get; set; }

        /// <summary>
        /// CancellationToken of the state machine
        /// </summary>
        public CancellationToken CancellationToken { get; private set; }

        /// <summary>
        /// Timeout of the state machine
        /// </summary>
        public DateTime Timeout { get; private set; }

        /// <summary>
        /// Name of the state machine
        /// </summary>
        public string Name { get; set; }

        public StateMachine() : this("StateMachine") { }

        public StateMachine(string name)
        {
            this.Name = name;
        }

        /// <summary>
        /// Invokes the state machine
        /// </summary>
        public virtual void Invoke()
        {
            this.Invoke(new CancellationToken(), new DateTime());
        }

        /// <summary>
        /// Invokes the state machine
        /// </summary>
        public virtual void Invoke(CancellationToken cancellationToken, DateTime timeout)
        {
            this._callStack = new List<State>();

            if (this.Activated == null)
            {
                this.StateExecute(this.Start, cancellationToken, timeout);
            }
            else
            {
                this.StateExecute(this.Activated, cancellationToken, timeout);
            }
        }

        internal void StateExecute(State state, CancellationToken cancellationToken, DateTime timeout)
        {
            while (state != null)
            {
                this._callStack.Add(state);

                this.Activated = state;

                this.CancellationToken = cancellationToken;
                this.Timeout = timeout;

                state.Execute(cancellationToken, timeout);

                state = GetSuccessor(state, cancellationToken, timeout);
            }
        }

        private static State GetSuccessor(State state, CancellationToken cancellationToken, DateTime timeout)
        {
            bool hasSuccessor = false;
            State follow = null;
            while (!hasSuccessor)
            {
                hasSuccessor = state.GetSuccessor(out follow);

                cancellationToken.ThrowIfCanceled();
                timeout.ThrowIfTimeExceeded();

                state.StateExit();
            }

            return follow;
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
            var source = this.GetNode(sourceName);
            var target = this.GetNode(targetName);

            return this.AddEdge(source, target, condition);
        }

        /// <summary>
        /// Add an edge to the state machine that connects the 'source' node and the 'target' node.
        /// </summary>
        public StateMachine AddEdge(State source, State target, Condition condition)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (target == null)
            {
                throw new ArgumentNullException("target");
            }

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