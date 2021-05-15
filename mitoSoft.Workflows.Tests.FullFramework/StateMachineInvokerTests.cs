using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace mitoSoft.Workflows.Tests.FullFramework
{
    [TestClass]
    public class StateMachineInvokerTests
    {
        [TestMethod]
        public void Completed()
        {
            var completed = false;

            var stateMachine = new StateMachine()
              .AddNode(new State("Start", () => Debug.WriteLine("Start")))
              .AddNode(new State("State1", () => Debug.WriteLine("State1")))
              .AddNode(new State("State2", () => Debug.WriteLine("State2")))
              .AddNode(new State("End", () => Debug.WriteLine("End")))
              .AddEdge("Start", "State1", () => { return true; })
              .AddEdge("State1", "State2", () => { return true; })
              .AddEdge("State2", "End", () => { return true; });

            var invoker = new Invoker();
            invoker.Completed += (sender, args) =>
            {
                completed = true;
            };
            invoker.Invoke(stateMachine);

            Assert.AreEqual(true, completed);
        }

        [TestMethod]
        public void Faulted()
        {
            var completed = false;
            var aborted = false;

            var stateMachine = new StateMachine()
              .AddNode(new State("Start", () => Debug.WriteLine("Start")))
              .AddNode(new State("State1", () => Debug.WriteLine("State1")))
              .AddNode(new State("State2", () =>
              {
                  throw new InvalidOperationException();
              }))
              .AddNode(new State("End", () => Debug.WriteLine("End")))
              .AddEdge("Start", "State1", () => { return true; })
              .AddEdge("State1", "State2", () => { return true; })
              .AddEdge("State2", "End", () => { return true; });

            var invoker = new Invoker();
            invoker.Faulted += (sender, args) =>
            {
                aborted = true;
            };
            invoker.Completed += (sender, args) =>
            {
                completed = true;
            };
            invoker.Invoke(stateMachine);

            Assert.AreEqual(false, completed);
            Assert.AreEqual(true, aborted);
        }

        [TestMethod]
        public void AsynchronRun()
        {
            var states = new List<string>();

            var stateMachine = new StateMachine()
              .AddNode(new State("Start", () => Debug.WriteLine("Start")))
              .AddNode(new State("State1", () => Debug.WriteLine("State1")))
              .AddNode(new State("State2", () =>
              {
                  Thread.Sleep(1000);
                  Debug.WriteLine("State2");
              }))
              .AddNode(new State("End", () => Debug.WriteLine("End")))
              .AddEdge("Start", "State1", () => { return true; })
              .AddEdge("State1", "State2", () => { return true; })
              .AddEdge("State2", "End", () => { return true; });

            var invoker = new Invoker();
            invoker.Completed += (sender, args) =>
            {
                states.Add("Ended");
            };

            var t = invoker.InvokeAsync(stateMachine);

            states.Add("Started");

            t.Wait();

            Assert.AreEqual("Started;Ended", string.Join(";", states));
        }

        [TestMethod]
        public void WithoutCancelation()
        {
            var states = new List<string>();
            CancellationTokenSource token = null;

            var stateMachine = new StateMachine()
              .AddNode(new State("Start", () => Debug.WriteLine("Start")))
              .AddNode(new State("State1", () => Debug.WriteLine("State1")))
              .AddNode(new State("State2", () =>
              {
                  Thread.Sleep(1000);
                  Debug.WriteLine("State2");
              }))
              .AddNode(new State("End", () => Debug.WriteLine("End")))
              .AddEdge("Start", "State1", () => { return true; })
              .AddEdge("State1", "State2", () => { return true; })
              .AddEdge("State2", "End", () => { return true; });

            var invoker = new Invoker();
            invoker.Started += (sender, args) =>
            {
                token = args.CancellationTokenSource;
            };
            invoker.Completed += (sender, args) =>
            {
                states.Add("Ended");
            };
            invoker.Faulted += (sender, args) =>
            {
                states.Add("Canceled");
            };

            var t = invoker.InvokeAsync(stateMachine);

            Assert.AreEqual(false, token.IsCancellationRequested);

            states.Add("Started");

            Thread.Sleep(100);

            t.Wait();

            Assert.AreEqual("Started;Ended", string.Join(";", states));
        }

        [TestMethod]
        public void ManualCancelationByToken()
        {
            var states = new List<string>();
            CancellationTokenSource token = null;

            var stateMachine = new StateMachine()
              .AddNode(new State("Start", () => Debug.WriteLine("Start")))
              .AddNode(new State("State1", () => Debug.WriteLine("State1")))
              .AddNode(new State("State2", () =>
              {
                  Thread.Sleep(1000);
                  Debug.WriteLine("State2");
              }))
              .AddNode(new State("End", () => Debug.WriteLine("End")))
              .AddEdge("Start", "State1", () => { return true; })
              .AddEdge("State1", "State2", () => { return true; })
              .AddEdge("State2", "End", () => { return true; });

            var invoker = new Invoker();
            invoker.Started += (sender, args) =>
            {
                token = args.CancellationTokenSource;
            };
            invoker.Completed += (sender, args) =>
            {
                states.Add("Ended");
            };
            invoker.Faulted += (sender, args) =>
            {
                states.Add("Canceled");
            };

            var t = invoker.InvokeAsync(stateMachine);

            Assert.AreEqual(false, token.IsCancellationRequested);

            states.Add("Started");

            Thread.Sleep(100);

            states.Add("BeforeCancelRequested");
            token.Cancel();
            states.Add("AfterCancelRequested");

            t.Wait();

            Assert.AreEqual("Started;BeforeCancelRequested;AfterCancelRequested;Canceled", string.Join(";", states));
        }

        [TestMethod]
        public void ManualCancelationByInvoker()
        {
            var states = new List<string>();

            var stateMachine = new StateMachine()
              .AddNode(new State("Start", () => Debug.WriteLine("Start")))
              .AddNode(new State("State1", () => Debug.WriteLine("State1")))
              .AddNode(new State("State2", () =>
              {
                  Thread.Sleep(1000);
                  Debug.WriteLine("State2");
              }))
              .AddNode(new State("End", () => Debug.WriteLine("End")))
              .AddEdge("Start", "State1", () => { return true; })
              .AddEdge("State1", "State2", () => { return true; })
              .AddEdge("State2", "End", () => { return true; });

            var invoker = new Invoker();
            invoker.Completed += (sender, args) =>
            {
                states.Add("Ended");
            };
            invoker.Faulted += (sender, args) =>
            {
                states.Add("Canceled");
            };

            var t = invoker.InvokeAsync(stateMachine);

            states.Add("Started");

            Thread.Sleep(100);

            states.Add("BeforeCancelRequested");
            invoker.CancelAsync();
            states.Add("AfterCancelRequested");

            t.Wait();

            Assert.AreEqual("Started;BeforeCancelRequested;AfterCancelRequested;Canceled", string.Join(";", states));
        }
    }
}