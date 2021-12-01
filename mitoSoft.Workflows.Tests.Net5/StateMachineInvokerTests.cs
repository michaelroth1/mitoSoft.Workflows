using Microsoft.VisualStudio.TestTools.UnitTesting;
using mitoSoft.Workflows.Enum;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace mitoSoft.Workflows.Tests.Net5
{
    [TestClass]
    public class StateMachineInvokerTests
    {
        [TestMethod]
        public void Standard()
        {
            var log = new List<string>();

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

            var invoker = new Invoker(stateMachine);
            invoker.Completed += (sender, args) =>
            {
                log.Add("Ended");
            };

            var t = invoker.Invoke();

            log.Add("Started");

            t.Wait();

            Assert.AreEqual("Started->Ended", string.Join("->", log));
        }

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

            var invoker = new Invoker(stateMachine);
            invoker.Completed += (sender, args) =>
            {
                completed = true;
            };
            invoker.Invoke().Wait();

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

            var invoker = new Invoker(stateMachine);
            invoker.Faulted += (sender, args) =>
            {
                aborted = true;
            };
            invoker.Completed += (sender, args) =>
            {
                completed = true;
            };
            invoker.Invoke().Wait();

            Assert.AreEqual(false, completed);
            Assert.AreEqual(true, aborted);
        }

        [TestMethod]
        public void WithoutCancelation()
        {
            var log = new List<string>();
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

            var invoker = new Invoker(stateMachine);
            invoker.Started += (sender, args) =>
            {
                token = args.CancellationTokenSource;
            };
            invoker.Completed += (sender, args) =>
            {
                log.Add("Ended");
            };
            invoker.Faulted += (sender, args) =>
            {
                log.Add("Canceled");
            };

            var t = invoker.Invoke();

            Assert.AreEqual(false, token.IsCancellationRequested);

            log.Add("Started");

            Thread.Sleep(100);

            t.Wait();

            Assert.AreEqual("Started->Ended", string.Join("->", log));
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

            var invoker = new Invoker(stateMachine);
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

            var t = invoker.Invoke();

            Assert.AreEqual(false, token.IsCancellationRequested);

            states.Add("Started");

            Thread.Sleep(100);

            states.Add("BeforeCancelRequested");
            token.Cancel();
            states.Add("AfterCancelRequested");

            t.Wait();

            Assert.AreEqual("Started->BeforeCancelRequested->AfterCancelRequested->Canceled", string.Join("->", states));
        }

        [TestMethod]
        public void ManualCancelationByInvoker()
        {
            var log = new List<string>();

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

            var invoker = new Invoker(stateMachine);
            invoker.Completed += (sender, args) =>
            {
                log.Add("Ended");
            };
            invoker.Faulted += (sender, args) =>
            {
                log.Add("Canceled");
            };

            var t = invoker.Invoke();

            log.Add("Started");

            Thread.Sleep(100);

            log.Add("BeforeCancelRequested");
            invoker.Cancel();
            log.Add("AfterCancelRequested");

            t.Wait();

            Assert.AreEqual("Started->BeforeCancelRequested->AfterCancelRequested->Canceled", string.Join("->", log));
        }

        [TestMethod]
        public void TwoParallelStatemachines()
        {
            var log = new List<string>();

            var inner1 = new TestStateMachine("inner1", 1000, log);
            var inner2 = new TestStateMachine("inner2", 100, log);

            var outer = new StateMachine()
                .AddNode(new State("Start", () => log.Add("outer.Start")))
                .AddNode(new State("State1", () =>
                {
                    log.Add("outer.State1");
                    var t1 = (new Invoker(inner1)).Invoke();
                    var t2 = (new Invoker(inner2)).Invoke();
                    var tasks = new List<Task>() { t1, t2 };
                    Task.WaitAll(tasks.ToArray());
                }))
                .AddNode(new State("End", () => log.Add("outer.End")))
                .AddEdge("Start", "State1", () => { return true; })
                .AddEdge("State1", "End", () => { return true; });

            var invoker = new Invoker(outer);
            invoker.Completed += (sender, args) =>
            {
                log.Add("outer.Ended");
            };
            invoker.Faulted += (sender, args) =>
            {
                log.Add("outer.Canceled");
            };

            var t = invoker.Invoke();

            t.Wait();

            Assert.AreEqual("outer.Start->outer.State1->" +
                            "inner2.Start->inner2.State1->inner2.State2->inner2.End->" +
                            "inner1.Start->inner1.State1->inner1.State2->inner1.End->" +
                            "outer.End->outer.Ended", string.Join("->", log));
        }

        [TestMethod]
        public void Timeout()
        {
            var log = new List<string>();
            FaultType faultType = FaultType.None;

            var inner1 = new TestStateMachine("inner1", 1000, log);
            var inner2 = new TestStateMachine("inner2", 100, log);

            var stateMachine = new StateMachine()
                .AddNode(new State("Start", () =>
                {
                    log.Add("Start");
                    Thread.Sleep(1100);
                }))
                .AddNode(new State("State1", () =>
                {
                    log.Add("State1");
                    Thread.Sleep(1100);
                }))
                .AddNode(new State("End", () => log.Add("End")))
                .AddEdge("Start", "State1", () => { return true; })
                .AddEdge("State1", "End", () => { return true; });

            var invoker = new Invoker(stateMachine);
            invoker.Completed += (sender, args) =>
            {
                log.Add("Ended");
            };
            invoker.Faulted += (sender, args) =>
            {
                faultType = args.FaultType;
                log.Add("Canceled");
            };

            var t = invoker.Invoke(TimeSpan.FromSeconds(1));

            t.Wait();

            Assert.AreEqual("Start->Canceled", string.Join("->", log));
            Assert.AreEqual(FaultType.ByTimeout, faultType);
        }

        [TestMethod]
        public void EndlessLoop() //runs in an endless loop and ccheck if no Stackoverflow exception is throw due to a full StackTrace
        {
            //Variables
            var log = new List<string>();

            var stateMachine = new StateMachine()
                .AddNode(new TestState("a", null))
                .AddNode(new TestState("b", null))
                .AddEdge("a", "b", () => { return true; })
                .AddEdge("b", "a", () => { return true; });

            var invoker = new Invoker(stateMachine);
            invoker.Completed += (sender, args) =>
            {
                log.Add("Ended");
            };
            invoker.Faulted += (sender, args) =>
            {
                log.Add($"Faulted:{args.FaultType}");
            };

            var t = invoker.Invoke(TimeSpan.FromSeconds(5));

            t.Wait();

            Assert.AreEqual("Faulted:ByTimeout", string.Join("->", log));
        }

        [TestMethod]
        public void CheckCallStackSize()
        {
            //Variables
            var log = new List<string>();
            var now = DateTime.Now;

            var stateMachine = new StateMachine()
                .AddNode(new TestState("a", null))
                .AddNode(new TestState("b", null))
                .AddEdge("a", "b", () => { return true; })
                .AddEdge("b", "a", () =>
                {
                    if (DateTime.Now > now.AddSeconds(9))
                    {
                        throw new InvalidOperationException("CheckCallStackText");
                    }
                    else
                    {
                        return true;
                    }
                });

            try
            {
                stateMachine.Invoke();
            }
            catch (Exception ex)
            {
                var callStack = ex.StackTrace; 
                var callStackLines = callStack.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();
                Assert.IsTrue(callStackLines.Count() < 20);
            }  
        }

        /// <summary>
        /// From here
        /// https://docs.microsoft.com/de-de/dotnet/framework/windows-workflow-foundation/using-workflowinvoker-and-workflowapplication
        /// </summary>
        [TestMethod]
        public void MicroSoftWfAppComparison()
        {
            var log = new List<string>();

            AutoResetEvent syncEvent = new(false);

            var wf = new StateMachine()
                .AddNode(new State("State1", () =>
                {
                    log.Add("Hello World");
                }));

            // Create the WorkflowApplication using the desired
            // workflow definition.
            var wfApp = new Invoker(wf);

            // Handle the desired lifecycle events.
            wfApp.Completed += (sender, args) =>
            {
                log.Add("Completed");
                syncEvent.Set();
            };

            // Start the workflow.
            wfApp.Invoke();

            // Wait for Completed to arrive and signal that
            // the workflow is complete.
            syncEvent.WaitOne();

            Assert.AreEqual("Hello World;Completed", string.Join(";", log));
        }
    }
}