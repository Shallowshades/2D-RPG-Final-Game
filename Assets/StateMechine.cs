using UnityEngine;

public class StateMechine
{
    public EntityState currentState {  get; private set; }
    
    public void Initialize(EntityState startState)
    {
        currentState = startState;
        startState.Enter();
    }

    public void ChangeState(EntityState newState)
    {
        currentState.Exit();
        currentState = newState;
        currentState.Enter();
    }

    public void UpdateActiveState()
    {
        currentState.Update();
    }
}
