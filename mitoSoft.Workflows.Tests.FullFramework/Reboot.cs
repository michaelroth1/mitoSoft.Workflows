using Microsoft.VisualStudio.TestTools.UnitTesting;
using mitoSoft.Workflows.Tests.FullFramework.StateMachines;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace mitoSoft.Workflows.Tests.FullFramework
{
    [TestClass]
    public class RebootingTests
    {
        [TestMethod]
        public void Standard()
        {
            var reboot = false;
            var count = 0;

            var stateMachine = new StateMachine()
              .AddNode(new State("Start", () =>
              {
                  count++;
                  Debug.WriteLine("Start");
              }))
              .AddNode(new State("State1", () => Debug.WriteLine("State1")))
              .AddNode(new State("State2", () =>
              {
                  if (!reboot)
                  {
                      reboot = true;
                      throw new InvalidOperationException();
                  }
              }))
              .AddNode(new State("End", () => Debug.WriteLine("End")))
              .AddEdge("Start", "State1", () => { return true; })
              .AddEdge("State1", "State2", () => { return true; })
              .AddEdge("State2", "End", () => { return true; });

            var invoker = new Invoker(stateMachine);
            invoker.Faulted += (sender, args) =>
            {
                invoker.Invoke().Wait();
            };
            invoker.Completed += (sender, args) =>
            {
                Assert.AreEqual(1, count); //start state should only runned once
            };
            invoker.Invoke().Wait();
        }

        [TestMethod]
        public void PersistInFile()
        {
            var stateMachine = new PersistableStateMachine();

            var invoker = new Invoker(stateMachine);
            invoker.Faulted += (sender, args) =>
            {
                this.SerializeToFile(stateMachine);
            };
            invoker.Completed += (sender, args) =>
            {
                Assert.Fail("1st run should result in an exception.");
            };
            invoker.Invoke().Wait();

            Thread.Sleep(1000);

            //2nd Run
            var sm = this.DeserializeFromFile();

            invoker = new Invoker(sm);
            invoker.Faulted += (sender, args) =>
            {
                Assert.Fail("2nd run should not result in an exception.");
            };
            invoker.Completed += (sender, args) =>
            {
                //start state should only runned once
                Assert.AreEqual(1, ((PersistableStateMachine)args.StateMachine).InvokingCountOfState1);
            };
            invoker.Invoke().Wait();
        }

        private void SerializeToFile(PersistableStateMachine stateMachine)
        {
            var formatter = new BinaryFormatter();
            var stream = new FileStream("temp.bin", FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, stateMachine);
            stream.Close();
        }

        private PersistableStateMachine DeserializeFromFile()
        {
            var formatter = new BinaryFormatter();
            var stream = new FileStream("temp.bin", FileMode.Open, FileAccess.Read, FileShare.Read);
            var sm = (PersistableStateMachine)formatter.Deserialize(stream);
            stream.Close();
            return sm;
        }
    }
}