using mitoSoft.Workflows.Enum;

namespace mitoSoft.Workflows.EventArgs
{
    public class StateMachineFaultedEventArgs : System.EventArgs
    {
        public StateMachineFaultedEventArgs(StateMachine stateMachine, FaultType type, System.Exception exception)
        {
            this.StateMachine = stateMachine;
            this.Exception = exception;
            this.WorkflowCancelationType = type;
        }

        public StateMachine StateMachine { get; }

        public System.Exception Exception { get; }

        public FaultType WorkflowCancelationType { get; }
    }
}