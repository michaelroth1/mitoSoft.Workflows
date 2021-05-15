using System.Threading;
using System.Threading.Tasks;

namespace mitoSoft.Workflows.EventArgs
{
    public class StateMachineStartedAsyncronousEventArgs : System.EventArgs
    {
        public StateMachineStartedAsyncronousEventArgs(Task task, CancellationTokenSource cancellationTokenSource)
        {
            this.Task = task;
            this.CancellationTokenSource = cancellationTokenSource;
        }

        public Task Task { get; }

        public CancellationTokenSource CancellationTokenSource { get; }
    }
}