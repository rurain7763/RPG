using System;
using System.Collections.Generic;

public abstract class State
{
    public StateMachine stateMachine { get; set; }

    public virtual void Enter() { }
    public virtual void Execute() { }
    public virtual void Exit() { }
}

public class StateTransition
{
    public State FromState;
    public State ToState;
    public uint Priority;

    public ICondition Condition;

    public StateTransition(State from, State to, ICondition condition)
    {
        FromState = from;
        ToState = to;
        Condition = condition;
        Priority = 0;
    }
}

public class StateMachine
{
    public State CurrentState { get; private set; }

    Dictionary<Type, State> states = new();
    Dictionary<Type, List<StateTransition>> transitionsDic = new();
    List<StateTransition> globalTransitionList = new();

    public event Action<State, State> OnStateChanged;

    public void Begin()
    {
        globalTransitionList.Sort((a, b) => b.Priority.CompareTo(a.Priority));

        foreach (var kvp in transitionsDic)
        {
            kvp.Value.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        }

        CurrentState?.Enter();
    }

    public void Tick()
    {
        if (CurrentState == null)
        {
            return;
        }

        if (CheckGlobalTransitions())
        {
            return;
        }

        if (CheckTransitions())
        {
            return;
        }

        CurrentState.Execute();
    }

    bool CheckGlobalTransitions()
    {
        foreach (var transition in globalTransitionList)
        {
            if (!transition.Condition.Evaluate())
            {
                continue;
            }

            if (CurrentState == transition.ToState)
            {
                return false;
            }

            State prevState = CurrentState;
            // Logger.Info($"Global transition from {prevState.GetType().Name} to {transition.ToState.GetType().Name} triggered.");
            
            prevState.Exit();
            CurrentState = transition.ToState;
            CurrentState.Enter();

            OnStateChanged?.Invoke(prevState, transition.ToState);

            return true;
        }
        return false;
    }

    bool CheckTransitions()
    {
        if (transitionsDic.TryGetValue(CurrentState.GetType(), out var transitionList))
        {
            foreach (var transition in transitionList)
            {
                if (!transition.Condition.Evaluate())
                {
                    continue;
                }

                if (CurrentState == transition.ToState)
                {
                    return false;
                }

                // Logger.Info($"Transition from {CurrentState.GetType().Name} to {transition.ToState.GetType().Name} triggered.");
                CurrentState.Exit();
                CurrentState = transition.ToState;
                CurrentState.Enter();

                OnStateChanged?.Invoke(transition.FromState, transition.ToState);

                return true;
            }
        }

        return false;
    }

    public T AddState<T>(params object[] args) where T : State
    {
        var type = typeof(T);
        if (states.TryGetValue(type, out State exist))
        {
            return exist as T;
        }

        var state = Activator.CreateInstance(type, args) as T;
        state.stateMachine = this;

        states[type] = state;

        return state;
    }

    public void RemoveState<T>() where T : State
    {
        var type = typeof(T);
        if (states.ContainsKey(type))
        {
            states.Remove(type);
        }
    }

    public void AddTransition<TFrom, TTo>(Func<bool> condition, uint priority = 0)
        where TFrom : State
        where TTo : State
    {
        AddTransition<TFrom, TTo>(new FuncCondition(condition), priority);
    }

    public void AddTransition<TFrom, TTo>(ICondition condition, uint priority = 0)
        where TFrom : State
        where TTo : State
    {
        var fromType = typeof(TFrom);
        var toType = typeof(TTo);

        if (!states.TryGetValue(fromType, out State from) || !states.TryGetValue(toType, out State to))
        {
            throw new InvalidOperationException("Both states must be added to the state machine before adding a transition.");
        }

        if (!transitionsDic.TryGetValue(fromType, out var transitionsList))
        {
            transitionsList = new List<StateTransition>();
            transitionsDic[fromType] = transitionsList;
        }

        var transition = new StateTransition(from, to, condition);
        transition.Priority = priority;

        transitionsList.Add(transition);
    }

    public void AddGlobalTransition<TTo>(Func<bool> condition, uint priority = 0)
        where TTo : State
    {
        AddGlobalTransition<TTo>(new FuncCondition(condition), priority);
    }

    public void AddGlobalTransition<TTo>(ICondition condition, uint priority = 0)
        where TTo : State
    {
        var toType = typeof(TTo);
        if (!states.TryGetValue(toType, out State to))
        {
            throw new InvalidOperationException("State must be added to the state machine before adding a global transition.");
        }

        var transition = new StateTransition(null, to, condition);
        transition.Priority = priority;

        globalTransitionList.Add(transition);
    }

    public void SetAsEntryState<T>() where T : State
    {
        if (!states.TryGetValue(typeof(T), out State state))
        {
            throw new InvalidOperationException("State must be added to the state machine before setting it as the entry state.");
        }

        CurrentState = state;
    }

    public T GetState<T>() where T : State
    {
        var type = typeof(T);
        if (!states.TryGetValue(type, out State state))
        {
            throw new InvalidOperationException("State must be added to the state machine before getting it.");
        }
        return state as T;
    }

    public void ChangeState<T>() where T : State
    {
        ChangeState(typeof(T));
    }

    public void ChangeState(Type type)
    {
        if (!states.TryGetValue(type, out State newState))
        {
            throw new InvalidOperationException("State must be added to the state machine before changing to it.");
        }

        State prevState = CurrentState;
        prevState?.Exit();
        CurrentState = newState;
        CurrentState.Enter();
        OnStateChanged?.Invoke(prevState, newState);
    }

    public bool TryChangeState<T>() where T : State
    {
        var type = typeof(T);
        if (!states.TryGetValue(type, out State newState))
        {
            return false;
        }

        State prevState = CurrentState;
        prevState?.Exit();
        CurrentState = newState;
        CurrentState.Enter();
        OnStateChanged?.Invoke(prevState, newState);

        return true;
    }

    public void ChangeState(State newState)
    {
        if (newState == null)
        {
            throw new ArgumentNullException(nameof(newState), "New state cannot be null.");
        }

        State prevState = CurrentState;
        prevState?.Exit();
        CurrentState = newState;
        CurrentState.Enter();
        OnStateChanged?.Invoke(prevState, newState);
    }

    public bool IsInState<T>() where T : State
    {
        return IsInState(typeof(T));
    }

    public bool IsInState(Type type)
    {
        return CurrentState.GetType() == type;
    }
}