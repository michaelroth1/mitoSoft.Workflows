using mitoSoft.Graphs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace mitoSoft.Workflows
{
    /// <summary>
    /// A state of a statemachine
    /// </summary>
    [DebuggerDisplay(nameof(State) + " ({ToString()})")]
    public class State : Node
    {
        private readonly Action _stateFunction;
        private readonly Action _stateExitFunction;

        /// <summary>
        /// A state of a statemachine
        /// </summary>
        public State(string name) : base(name) { }

        /// <summary>
        /// A state of a statemachine
        /// </summary>
        /// <param name="stateFunction">Entering delegate of the state</param>
        public State(string name, Action stateFunction) : base(name)
        {
            _stateFunction = stateFunction;
        }

        /// <summary>
        /// A state of a statemachine
        /// </summary>
        /// <param name="stateFunction">Entering delegate of the state</param>
        /// <param name="stateExitFunction">Exiting delegate of the state</param>
        public State(string name, Action stateFunction, Action stateExitFunction) : this(name, stateFunction)
        {
            _stateExitFunction = stateExitFunction;
        }

        /// <summary>
        /// All incoming and outgoing transitions
        /// </summary>
        public new IEnumerable<Transition> Edges => base.Edges.Cast<Transition>();

        /// <summary>
        /// All predecessor states
        /// </summary>
        public new IEnumerable<State> Predecessors => base.Predecessors.Cast<State>();

        /// <summary>
        /// All successor states
        /// </summary>
        public new IEnumerable<State> Successors => base.Successors.Cast<State>();

        /// <summary>
        /// Calls the delegate for entering the state
        /// </summary>
        public virtual void StateFunction()
        {
            _stateFunction?.Invoke();
        }

        /// <summary>
        /// Calls the delegate for exting the state
        /// </summary>
        public virtual void StateExit()
        {
            _stateExitFunction?.Invoke();
        }

        internal void Execute()
        {
            this.Execute(new CancellationToken());
        }

        /// <summary>
        /// Executes the StateMachine outgoing form this state
        /// </summary>
        /// <remarks>
        /// In case of more than one activated transitions -> first come first serve.
        /// For this reason it is possible to run states in parallel.
        /// </remarks>
        internal void Execute(CancellationToken cancellationToken)
        {
            var switchover = false;
            State successor = null;
            var isFinal = this.Edges.All(t => t.Target == this);//es handel sich um einen Final-State

            cancellationToken.ThrowIfCancellationRequested();

            this.StateFunction();

            cancellationToken.ThrowIfCancellationRequested();

            foreach (var transition in this.Edges.Where(t => t.Source == this))
            {
                if (transition.Check())
                {
                    successor = (State)transition.Target;
                    switchover = true;
                    break;
                }
            }

            cancellationToken.ThrowIfCancellationRequested();

            this.StateExit();

            if (switchover)
            {
                successor.Execute(cancellationToken);
            }
            else if (!isFinal)
            {
                this.Execute(cancellationToken);
            }
        }

        public override string ToString() => $"{this.Name} (Transitions: {this._edges.Count})";
    }
}