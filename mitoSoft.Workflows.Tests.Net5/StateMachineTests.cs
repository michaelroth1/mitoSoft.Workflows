using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace mitoSoft.Workflows.Tests.Net5
{
    [TestClass]
    public class StateMachineTests
    {
        [TestMethod]
        public void SelfReference()
        {
            var log = new List<string>();

            var stateMachine = new StateMachine();
            stateMachine.AddNode(new TestState("Start", log));
            stateMachine.AddNode(new TestState("State1", log));
            stateMachine.AddNode(new TestState("State2", log));
            stateMachine.AddNode(new TestState("End", log));
            stateMachine.AddEdge("Start", "State1", () => { return true; });
            stateMachine.AddEdge("State1", "State1", () => { return false; });
            stateMachine.AddEdge("State1", "State2", () => { return true; });
            stateMachine.AddEdge("State2", "End", () => { return true; });

            stateMachine.Invoke();

            Assert.AreEqual("Start->State1->State2->End", string.Join("->", log));
        }

        [TestMethod]
        public void SelfReferenceLoop1()
        {
            var log = new List<string>();
            bool loop = false;

            var stateMachine = new StateMachine();
            stateMachine.AddNode(new TestState("Start", log));
            stateMachine.AddNode(new TestState("State1", log));
            stateMachine.AddNode(new TestState("State2", log));
            stateMachine.AddNode(new TestState("End", log));
            stateMachine.AddEdge("Start", "State1", () => { return true; });
            stateMachine.AddEdge("State1", "State1", () =>
            {
                loop = !loop;
                return loop;
            });
            stateMachine.AddEdge("State1", "State2", () => { return true; });
            stateMachine.AddEdge("State2", "End", () => { return true; });

            stateMachine.Invoke();

            Assert.AreEqual("Start->State1->State1->State2->End", string.Join("->", log));
        }

        [TestMethod]
        public void SelfReferenceLoop2()
        {
            var log = new List<string>();
            bool loop = false;

            var stateMachine = new StateMachine();
            stateMachine.AddNode(new TestState("Start", log));
            stateMachine.AddNode(new TestState("State1", log));
            stateMachine.AddNode(new TestState("State2", log));
            stateMachine.AddNode(new TestState("End", log));
            stateMachine.AddEdge("Start", "State1", () => { return true; });
            stateMachine.AddEdge("State1", "State2", () => { return loop; });
            stateMachine.AddEdge("State1", "State1", () =>
            {
                loop = !loop;
                return loop;
            });
            stateMachine.AddEdge("State2", "End", () => { return true; });

            stateMachine.Invoke();

            Assert.AreEqual("Start->State1->State1->State2->End", string.Join("->", log));
        }

        [TestMethod]
        public void Standard1()
        {
            var log = new List<string>();

            var stateMachine = new StateMachine()
                .AddNode(new State("Start", () => Debug.WriteLine("Start")))
                .AddNode(new State("State1", () => Debug.WriteLine("State1")))
                .AddNode(new State("State2", () => Debug.WriteLine("State2")))
                .AddNode(new State("End", () => Debug.WriteLine("End")))
                .AddEdge("Start", "State1", () => { return true; })
                .AddEdge("State1", "State2", () => { return true; })
                .AddEdge("State2", "End", () => { return true; });

            stateMachine.Invoke();

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Standard2()
        {
            var log = new List<string>();

            var stateMachine = new StateMachine()
                .AddNode(new TestState("Start", log))
                .AddNode(new TestState("State1", log))
                .AddNode(new TestState("State2", log))
                .AddNode(new TestState("End", log))
                .AddEdge("Start", "State1", () => { return true; })
                .AddEdge("State1", "State2", () => { return true; })
                .AddEdge("State2", "End", () => { return true; });

            stateMachine.Invoke();

            Assert.AreEqual("Start->State1->State2->End", string.Join("->", log));
        }

        [TestMethod]
        public void Standard3()
        {
            var log = new List<string>();

            var stateMachine = new StateMachine()
                .AddNode(new TestState("Start", log))
                .AddNode(new TestState("State1", log))
                .AddNode(new TestState("State2", log))
                .AddNode(new TestState("End", log))
                .AddEdge("Start", "State1", () => { return false; })
                .AddEdge("Start", "State2", () => { return true; })
                .AddEdge("State2", "End", () => { return true; });

            stateMachine.Invoke();

            Assert.AreEqual("Start->State2->End", string.Join("->", log));
        }

        [TestMethod]
        public void ForeignStart()
        {
            var log = new List<string>();

            var stateMachine = new StateMachine()
                .AddNode(new TestState("Start", log))
                .AddNode(new TestState("State1", log))
                .AddNode(new TestState("State2", log))
                .AddNode(new TestState("End", log))
                .AddEdge("Start", "State1", () => { return true; })
                .AddEdge("State1", "State2", () => { return true; })
                .AddEdge("State2", "End", () => { return true; });

            stateMachine.Start = stateMachine.GetNode("State1");
            stateMachine.Invoke();

            Assert.AreEqual("State1->State2->End", string.Join("->", log));
        }

        [TestMethod]
        public void TransitionWait1()
        {
            //Variables
            var log = new List<string>();
            var i = 0;

            //States 
            var start = new TestState("Start", log);
            var state1 = new TestState("State1", log);
            var state2 = new TestState("State2", log);
            var end = new TestState("End", log);

            var stateMachine = new StateMachine()
                .AddNode(start)
                .AddNode(state1)
                .AddNode(state2)
                .AddNode(end);

            var t = new Transition(start, state2, () =>
            {
                log.Add("Edge1");
                i++;
                return i >= 3;
            });

            stateMachine.AddEdge("Start", "State1", () => { return false; })
                        .AddEdge(t)
                        .AddEdge("State2", "End", () => { return true; });

            stateMachine.Invoke();

            Assert.AreEqual("Start->Edge1->Edge1->Edge1->State2->End", string.Join("->", log));
        }

        [TestMethod]
        public void TransitionWait2()
        {
            //Variables
            var log = new List<string>();
            var i = 0;

            new StateMachine()
                .AddNode(new TestState("Start", log))
                .AddNode(new TestState("State1", log))
                .AddNode(new TestState("State2", log))
                .AddNode(new TestState("End", log))
                .AddEdge("Start", "State1", () => { return false; })
                .AddEdge("Start", "State2", () =>
                {
                    log.Add("Edge1");
                    i++;
                    return i >= 3;
                })
                .AddEdge("State2", "End", () => { return true; })
                .Invoke();

            Assert.AreEqual("Start->Edge1->Edge1->Edge1->State2->End", string.Join("->", log));
        }

        [TestMethod]
        public void Loop1()
        {
            //Variables
            var log = new List<string>();
            var i = 0;
            var state2Reached = false;

            new StateMachine()
                .AddNode(new TestState("Start", log))
                .AddNode(new TestState("State1", log))
                .AddNode(new State("State2", () =>
                {
                    log.Add("State2");
                    state2Reached = true;
                }))
                .AddNode(new TestState("End", log))
                .AddEdge("Start", "End", () =>
                {
                    return state2Reached;
                })
                .AddEdge("Start", "State1", () =>
                {
                    log.Add("Edge1");
                    i++;
                    return i >= 3;
                })
                .AddEdge("State1", "State2", () => { return true; })
                .AddEdge("State2", "Start", () => { return true; })
                .Invoke();

            Assert.AreEqual("Start->Edge1->Edge1->Edge1->State1->State2->Start->End", string.Join("->", log));
        }

        [TestMethod]
        public void Loop2()
        {
            //Variables
            var log = new List<string>();
            var i = 0;
            var state2Reached = false;

            var stateMachine = new StateMachine()
                .AddNode(new TestState("Start", log))
                .AddNode(new TestState("State1", log))
                .AddNode(new State("State2", () =>
                {
                    log.Add("State2");
                    state2Reached = true;
                }))
                .AddNode(new TestState("End", log))
                .AddEdge("Start", "End", () =>
                {
                    return state2Reached && i >= 3;
                })
                .AddEdge("Start", "State1", () =>
                {
                    log.Add("Edge1");
                    i++;
                    return i >= 3;
                })
                .AddEdge("State1", "State2", () => { return true; })
                .AddEdge("State2", "Start", () => { return true; });

            stateMachine.Start = stateMachine.GetNode("State1");
            stateMachine.Invoke();

            Assert.AreEqual("State1->State2->Start->Edge1->Edge1->Edge1->State1->State2->Start->End", string.Join("->", log));
        }

        [TestMethod]
        public void TimedTransition()
        {
            //Variables
            var log = new List<string>();
            var now = DateTime.Now;

            new StateMachine()
                .AddNode(new TestState("Start", log))
                .AddNode(new State("State1",
                () =>
                {
                    log.Add("State1");
                },
                () =>
                {
                    System.Threading.Thread.Sleep(1000);
                }))
                .AddNode(new TestState("State2", log))
                .AddNode(new TestState("End", log))
                .AddEdge("Start", "State1", () => { return true; })
                .AddEdge("State1", "State2", () =>
                {
                    log.Add("Edge1");
                    return DateTime.Now >= now.AddSeconds(4); //results in 5 cycles of State1 due to the fact the execution takes more than zero time
                })
                .AddEdge("State2", "End", () => { return true; })
                .Invoke();

            Assert.AreEqual("Start->State1->Edge1->Edge1->Edge1->Edge1->Edge1->State2->End", string.Join("->", log));
        }

        [TestMethod]
        public void NestedTest()
        {
            //Variables
            var log = new List<string>();
            var now = DateTime.Now;

            new StateMachine()
                .AddNode(new TestState("Start", log))
                .AddNode(new State("State1",
                () =>
                {
                    log.Add("State1");
                    new StateMachine()
                        .AddNode(new TestState("InnerStart", log))
                        .AddNode(new TestState("Inner1", log))
                        .AddNode(new TestState("InnerEnd", log))
                        .AddEdge("InnerStart", "Inner1", () => { return true; })
                        .AddEdge("Inner1", "InnerEnd", () => { return true; })
                        .Invoke();
                }))
                .AddNode(new TestState("End", log))
                .AddEdge("Start", "State1", () => { return true; })
                .AddEdge("State1", "End", () => { return true; })
                .Invoke();

            Assert.AreEqual("Start->State1->InnerStart->Inner1->InnerEnd->End", string.Join("->", log));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Faulted()
        {
            //Variables
            var log = new List<string>();
            var now = DateTime.Now;

            new StateMachine()
                .AddNode(new TestState("Start", log))
                .AddNode(new State("State1",
                () =>
                {
                    log.Add("State1");
                    throw new InvalidOperationException();
                }))
                .AddNode(new TestState("End", log))
                .AddEdge("Start", "State1", () => { return true; })
                .AddEdge("State1", "End", () => { return true; })
                .Invoke();
        }
    }
}