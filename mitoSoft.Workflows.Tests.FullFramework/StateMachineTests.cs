using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace mitoSoft.Workflows.Tests
{
    [TestClass]
    public class StateMachineTests
    {
        [TestMethod]
        public void Standard1()
        {
            var result = new List<string>();

            //States 
            var stateMachine = new StateMachine()
                .AddNode(new TestState("Start", result))
                .AddNode(new TestState("State1", result))
                .AddNode(new TestState("State2", result))
                .AddNode(new TestState("End", result))
                .AddEdge("Start", "State1", () => { return true; })
                .AddEdge("State1", "State2", () => { return true; })
                .AddEdge("State2", "End", () => { return true; });

            stateMachine.Invoke();

            Assert.AreEqual("Start->State1->State2->End", string.Join("->", result));
        }

        [TestMethod]
        public void ForeignStart()
        {
            var result = new List<string>();

            var stateMachine = new StateMachine()
                .AddNode(new TestState("Start", result))
                .AddNode(new TestState("State1", result))
                .AddNode(new TestState("State2", result))
                .AddNode(new TestState("End", result))
                .AddEdge("Start", "State1", () => { return true; })
                .AddEdge("State1", "State2", () => { return true; })
                .AddEdge("State2", "End", () => { return true; });

            stateMachine.Invoke("State1");

            Assert.AreEqual("State1->State2->End", string.Join("->", result));
        }

        [TestMethod]
        public void Standard2()
        {
            var result = new List<string>();

            //States 
            var stateMachine = new StateMachine()
                .AddNode(new TestState("Start", result))
                .AddNode(new TestState("State1", result))
                .AddNode(new TestState("State2", result))
                .AddNode(new TestState("End", result))
                .AddEdge("Start", "State1", () => { return false; })
                .AddEdge("Start", "State2", () => { return true; })
                .AddEdge("State2", "End", () => { return true; });

            stateMachine.Invoke();

            Assert.AreEqual("Start->State2->End", string.Join("->", result));
        }

        [TestMethod]
        public void TransitionWait1()
        {
            //Variables
            var result = new List<string>();
            var i = 0;

            //States 
            var start = new TestState("Start", result);
            var state1 = new TestState("State1", result);
            var state2 = new TestState("State2", result);
            var end = new TestState("End", result);

            var stateMachine = new StateMachine()
                .AddNode(start)
                .AddNode(state1)
                .AddNode(state2)
                .AddNode(end);

            var t = new Transition(start, state2, () =>
            {
                i++;

                return i >= 3;
            });

            stateMachine.AddEdge("Start", "State1", () => { return false; })
                        .AddEdge(t)
                        .AddEdge("State2", "End", () => { return true; });

            stateMachine.Invoke();

            Assert.AreEqual("Start->Start->Start->State2->End", string.Join("->", result));
        }

        [TestMethod]
        public void TransitionWait2()
        {
            //Variables
            var result = new List<string>();
            var i = 0;

            new StateMachine()
                .AddNode(new TestState("Start", result))
                .AddNode(new TestState("State1", result))
                .AddNode(new TestState("State2", result))
                .AddNode(new TestState("End", result))
                .AddEdge("Start", "State1", () => { return false; })
                .AddEdge("Start", "State2", () =>
                {
                    i++;

                    return i >= 3;
                })
                .AddEdge("State2", "End", () => { return true; })
                .Invoke();

            Assert.AreEqual("Start->Start->Start->State2->End", string.Join("->", result));
        }

        [TestMethod]
        public void Loop1()
        {
            //Variables
            var result = new List<string>();
            var i = 0;
            var state2Reached = false;

            new StateMachine()
                .AddNode(new TestState("Start", result))
                .AddNode(new TestState("State1", result))
                .AddNode(new State("State2", () =>
                {
                    result.Add("State2");
                    state2Reached = true;
                }))
                .AddNode(new TestState("End", result))
                .AddEdge("Start", "End", () =>
                {
                    return state2Reached;
                })
                .AddEdge("Start", "State1", () =>
                {
                    i++;

                    return i >= 3;
                })
                .AddEdge("State1", "State2", () => { return true; })
                .AddEdge("State2", "Start", () => { return true; })
                .Invoke("Start");

            Assert.AreEqual("Start->Start->Start->State1->State2->Start->End", string.Join("->", result));
        }

        [TestMethod]
        public void Loop2()
        {
            //Variables
            var result = new List<string>();
            var i = 0;
            var state2Reached = false;

            new StateMachine()
                .AddNode(new TestState("Start", result))
                .AddNode(new TestState("State1", result))
                .AddNode(new State("State2", () =>
                {
                    result.Add("State2");
                    state2Reached = true;
                }))
                .AddNode(new TestState("End", result))
                .AddEdge("Start", "End", () =>
                {
                    return state2Reached && i >= 3;
                })
                .AddEdge("Start", "State1", () =>
                {
                    i++;

                    return i >= 3;
                })
                .AddEdge("State1", "State2", () => { return true; })
                .AddEdge("State2", "Start", () => { return true; })
                .Invoke("State1");

            Assert.AreEqual("State1->State2->Start->Start->Start->State1->State2->Start->End", string.Join("->", result));
        }

        [TestMethod]
        public void TimedTransition()
        {
            //Variables
            var result = new List<string>();
            var now = DateTime.Now;

            new StateMachine()
                .AddNode(new TestState("Start", result))
                .AddNode(new State("State1",
                () =>
                {
                    result.Add("State1");
                },
                () =>
                {
                    System.Threading.Thread.Sleep(1000);
                }))
                .AddNode(new TestState("State2", result))
                .AddNode(new TestState("End", result))
                .AddEdge("Start", "State1", () => { return true; })
                .AddEdge("State1", "State2", () =>
                {
                    return DateTime.Now >= now.AddSeconds(4); //results in 5 cycles of State1 due to the fact the execution takes more than zero time
                })
                .AddEdge("State2", "End", () => { return true; })
                .Invoke();

            Assert.AreEqual("Start->State1->State1->State1->State1->State1->State2->End", string.Join("->", result));
        }

        [TestMethod]
        public void NestedTest()
        {
            //Variables
            var result = new List<string>();
            var now = DateTime.Now;

            new StateMachine()
                .AddNode(new TestState("Start", result))
                .AddNode(new State("State1",
                () =>
                {
                    result.Add("State1");
                    new StateMachine()
                        .AddNode(new TestState("InnerStart", result))
                        .AddNode(new TestState("Inner1", result))
                        .AddNode(new TestState("InnerEnd", result))
                        .AddEdge("InnerStart", "Inner1", () => { return true; })
                        .AddEdge("Inner1", "InnerEnd", () => { return true; })
                        .Invoke();
                }))
                .AddNode(new TestState("End", result))
                .AddEdge("Start", "State1", () => { return true; })
                .AddEdge("State1", "End", () => { return true; })
                .Invoke();

            Assert.AreEqual("Start->State1->InnerStart->Inner1->InnerEnd->End", string.Join("->", result));
        }
    }
}