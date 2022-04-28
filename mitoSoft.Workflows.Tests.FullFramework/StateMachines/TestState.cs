using System.Collections.Generic;

namespace mitoSoft.Workflows.Tests
{
    internal class TestState : State
    {
        private readonly List<string> _output;

        public TestState(string name, List<string> output) : base(name)
        {
            this._output = output;
        }

        public override void StateFunction()
        {
            this._output.Add(this.Name);
        }
    }
}