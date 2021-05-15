using mitoSoft.Workflows.Enum;
using mitoSoft.Workflows.EventArgs;
using mitoSoft.Workflows.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace mitoSoft.Workflows
{
    public class Invoker
    {
        private CancellationTokenSource _tokenSource;

        public event EventHandler<StateMachineStartedAsyncronousEventArgs> Started;

        public event EventHandler<StateMachineFaultedEventArgs> Faulted;

        public event EventHandler<StateMachineCompletedEventArgs> Completed;

        public void Invoke(StateMachine stateMachine)
        {
            this.Invoke(stateMachine, new CancellationTokenSource(), TimeSpan.Zero);
        }

        internal void Invoke(StateMachine stateMachine, CancellationTokenSource tokenSource, TimeSpan timeOut)
        {
            try
            {
                var timeOutTime = timeOut.GetTime();

                stateMachine.Start.Execute(tokenSource.Token, timeOutTime);

                Completed?.Invoke(this, new StateMachineCompletedEventArgs(stateMachine));
            }
            catch (TimeoutException ex) when (ex.Message == "Timeout")
            {
                Faulted?.Invoke(this, new StateMachineFaultedEventArgs(stateMachine, FaultType.ByTimeout, ex));
            }
            catch (OperationCanceledException ex)
            {
                Faulted?.Invoke(this, new StateMachineFaultedEventArgs(stateMachine, FaultType.ByToken, ex));
            }
            catch (System.Exception ex)
            {
                Faulted?.Invoke(this, new StateMachineFaultedEventArgs(stateMachine, FaultType.ByException, ex));
            }
            finally
            {
                tokenSource.Dispose();
            }
        }

        public Task InvokeAsync(StateMachine stateMachine)
        {
            return this.InvokeAsync(stateMachine, TimeSpan.FromDays(5000.0));
        }

        public Task InvokeAsync(StateMachine stateMachine, TimeSpan timeOut)
        {
            _tokenSource = new CancellationTokenSource();

            var task = Task.Run(() =>
            {
                this.Invoke(stateMachine, _tokenSource, timeOut);
            });

            Started?.Invoke(this, new StateMachineStartedAsyncronousEventArgs(task, _tokenSource));

            return task;
        }

        public bool CancelAsync()
        {
            _tokenSource.Cancel();
            return _tokenSource.IsCancellationRequested;
        }
    }
}