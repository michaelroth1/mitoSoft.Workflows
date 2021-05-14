using mitoSoft.Graphs;
using System.Diagnostics;

namespace mitoSoft.Workflows
{
    public delegate bool Condition();

    /// <summary>
    /// A transition of a statemachine
    /// </summary>
    [DebuggerDisplay(nameof(Transition) + " ({ToString()})")]
    public class Transition : Edge
    {
        private readonly Condition _condition;

        /// <summary>
        /// A transition of a statemachine
        /// </summary>
        public Transition(State sourceNode, State targetNode, Condition condition) : base(sourceNode, targetNode)
        {
            this._condition = condition;
        }

        /// <summary>
        /// The source state of the transition
        /// </summary>
        public new State Source => (State)base.Source;

        /// <summary>
        /// The target state of the transition
        /// </summary>
        public new State Target => (State)base.Target;

        /// <summary>
        /// Returns the result of the 'Condition' delegate
        /// </summary>
        public virtual bool ConditionStatement
        {
            get
            {
                var result = _condition?.Invoke() ?? false;
                return result;
            }
        }

        internal bool Check()
        {
            return this.ConditionStatement;
        }
    }
}