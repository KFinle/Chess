using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InputReciever : MonoBehaviour
{
    protected IInputHandler[] inputHandlers;

    /// <summary>
    /// Overridden by inheriting classes
    /// </summary>
    public abstract void OnInputReceived();

    /// <summary>
    /// Called on activation
    /// </summary>
    private void Awake() 
    {
        inputHandlers = GetComponents<IInputHandler>();
    }
}
