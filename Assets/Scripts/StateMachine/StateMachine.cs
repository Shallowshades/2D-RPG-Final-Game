using TMPro;
using UnityEngine;

public class StateMachine
{
    public EntityState currentState {  get; private set; }
    public bool canChangeState;
    
    public void Initialize(EntityState startState)
    {
        currentState = startState;
        canChangeState = true;
        startState.Enter();
    }

    public void ChangeState(EntityState newState)
    {
        if (!canChangeState)
        {
            return;
        }

        currentState.Exit();
        currentState = newState;
        currentState.Enter();
    }

    public void UpdateActiveState()
    {
        currentState.Update();
    }

    public void SwitchOffStateMachine() => canChangeState = false;
}
