using System;
using System.Diagnostics;

namespace mitoSoft.Workflows.Tests.FullFramework.StateMachines
{
    [Serializable]
    internal class PersistableStateMachine : StateMachine
    {
        public PersistableStateMachine()
        {
            this
            .AddNode(new State("Start", () => Debug.WriteLine("Start")))
            .AddNode(new State("State1", () =>
            {
                Debug.WriteLine("State1");

                this.InvokingCountOfState1++;
            }))
            .AddNode(new State("State2", () =>
            {
                Debug.WriteLine("State2");

                if (!this.Reboot)
                {
                    this.Reboot = true;

                    throw new InvalidOperationException();
                }
            }))
            .AddNode(new State("End", () => Debug.WriteLine("End")))
            .AddEdge("Start", "State1", () => { return true; })
            .AddEdge("State1", "State2", () => { return true; })
            .AddEdge("State2", "End", () => { return true; });
        }

        public bool Reboot { get; set; } = false;

        public int InvokingCountOfState1 { get; set; } = 0;
    }
}