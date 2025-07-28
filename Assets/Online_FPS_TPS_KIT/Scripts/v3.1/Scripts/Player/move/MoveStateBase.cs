using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MoveStateBase
{
    protected CharacterMove characterMove;
    protected CharacterController characterController;

    public MoveStateBase(CharacterMove characterMove)
    {
        this.characterMove = characterMove;
        this.characterController = characterMove.characterController;
    }
    
    // Start is called before the first frame update
    public abstract void Tick();
    
    public virtual void OnStateEnter() { }
    public virtual void OnStateExit() { }
}
