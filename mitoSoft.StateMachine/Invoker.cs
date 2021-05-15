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
        public Invoker(StateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        private CancellationTokenSource _tokenSource;
        private StateMachine _stateMachine;

        public event EventHandler<StateMachineStartedAsyncronousEventArgs> Started;

        public event EventHandler<StateMachineFaultedEventArgs> Faulted;

        public event EventHandler<StateMachineCompletedEventArgs> Completed;

        public void Invoke()
        {
            this.Invoke(new CancellationTokenSource(), TimeSpan.Zero);
        }

        internal void Invoke(CancellationTokenSource tokenSource, TimeSpan timeOut)
        {
            try
            {
                var timeOutTime = timeOut.GetTime();

                _stateMachine.Start.Execute(tokenSource.Token, timeOutTime);

                Completed?.Invoke(this, new StateMachineCompletedEventArgs(_stateMachine));
            }
            catch (TimeoutException ex) when (ex.Message == "Timeout")
            {
                Faulted?.Invoke(this, new StateMachineFaultedEventArgs(_stateMachine, FaultType.ByTimeout, ex));
            }
            catch (OperationCanceledException ex)
            {
                Faulted?.Invoke(this, new StateMachineFaultedEventArgs(_stateMachine, FaultType.ByToken, ex));
            }
            catch (System.Exception ex)
            {
                Faulted?.Invoke(this, new StateMachineFaultedEventArgs(_stateMachine, FaultType.ByException, ex));
            }
            finally
            {
                tokenSource.Dispose();
            }
        }

        public Task InvokeAsync()
        {
            return this.InvokeAsync(TimeSpan.FromDays(5000.0));
        }

        public Task InvokeAsync(TimeSpan timeOut)
        {
            _tokenSource = new CancellationTokenSource();

            var task = Task.Run(() =>
            {
                this.Invoke(_tokenSource, timeOut);
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