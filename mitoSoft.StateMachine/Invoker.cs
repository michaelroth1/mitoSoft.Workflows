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
        private readonly StateMachine _stateMachine;

        public event EventHandler<StateMachineStartedAsyncronousEventArgs> Started;

        public event EventHandler<StateMachineFaultedEventArgs> Faulted;

        public event EventHandler<StateMachineCompletedEventArgs> Completed;

        /// <summary>
        /// Invokes a state machine asynchronously
        /// </summary>
        public Task Invoke()
        {
            return this.Invoke(TimeSpan.FromDays(5000.0));
        }

        /// <summary>
        /// Invokes a state machine asynchronously
        /// </summary>
        public Task Invoke(TimeSpan timeout)
        {
            _tokenSource = new CancellationTokenSource();

            var task = Task.Run(() =>
            {
                this.Invoke(_tokenSource, timeout);
            });

            Started?.Invoke(this, new StateMachineStartedAsyncronousEventArgs(task, _tokenSource));

            return task;
        }

        /// <summary>
        /// Cancels the run of a state machine
        /// </summary>
        /// <remarks>
        /// The cancelation is only possible in the idle state of a state machine.
        /// The idle state occurs before and after every statefunction execution.
        /// </remarks>
        public bool Cancel()
        {
            _tokenSource.Cancel();
            return _tokenSource.IsCancellationRequested;
        }

        internal void Invoke(CancellationTokenSource tokenSource, TimeSpan timeout)
        {
            try
            {
                var timeoutTime = timeout.GetTime();

                _stateMachine.Start.Execute(tokenSource.Token, timeoutTime);

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
    }
}