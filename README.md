# mitoSoft.Workflows
A .net graph library to build statemachines and run them autonomously.
Statemachines are build by states and transitions. Every transition has a
condition and switch over form its predeccesor to its successor.
By this concept it is possible to build simple straight forward statemachines as well
as complex ones like:
- time state machines
- nested state machines
- looped state machines
- ... 

## Dependencies

This library based in the [mitoSoft.Graphs](https://github.com/michaelroth1/mitoSoft.Graphs) library (Version 1.2.0 or higher).

## Example simple state machine

```c#

  var stateMachine = new StateMachine()
    .AddNode(new State("Start", () => Debug.WriteLine("Start")))
    .AddNode(new State("State1", () => Debug.WriteLine("State1")))
    .AddNode(new State("State2", () => Debug.WriteLine("State2")))
    .AddNode(new State("End", () => Debug.WriteLine("End")))
    .AddEdge("Start", "State1", () => { return true; })
    .AddEdge("State1", "State2", () => { return true; })
    .AddEdge("State2", "End", () => { return true; });
  
  stateMachine.Invoke();
  ...  
  
```


## Example for a nested statemachine

```c#

  new StateMachine()
    .AddNode(new State("Start"))
    .AddNode(new State("State1",
    () =>
    {      
      new StateMachine()
        .AddNode(new State("InnerStart"))
        .AddNode(new State("Inner1"))
        .AddNode(new State("InnerEnd"))
        .AddEdge("InnerStart", "Inner1", () => { return true; })
        .AddEdge("Inner1", "InnerEnd", () => { return true; })
        .Invoke();
    }))
    .AddNode(new State("End"))
    .AddEdge("Start", "State1", () => { return true; })
    .AddEdge("State1", "End", () => { return true; })
  .Invoke();
  
  ...  
  
```

For more examples see the testclasses in [testproject](mitoSoft.Workflows.Tests.FullFramework).
