using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class UIInputReciever : InputReciever
{
    [SerializeField] private UnityEvent clickEvent;

    /// <summary>
    /// On click, execute click method
    /// </summary>
    public override void OnInputReceived()
    {
        foreach (var handler in inputHandlers)
        {
            handler.ProcessInput(Input.mousePosition, gameObject, () => clickEvent.Invoke());
            
        }
    }
}
