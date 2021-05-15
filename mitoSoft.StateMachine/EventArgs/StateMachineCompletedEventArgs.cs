namespace mitoSoft.Workflows.EventArgs
{
    public class StateMachineCompletedEventArgs : System.EventArgs
    {
        public StateMachineCompletedEventArgs(StateMachine stateMachine)
        {
            this.StateMachine = stateMachine;
        }

        public StateMachine StateMachine { get; }
    }
}