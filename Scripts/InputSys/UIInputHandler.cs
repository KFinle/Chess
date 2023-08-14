using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIInputHandler : MonoBehaviour, IInputHandler
{
    /// <summary>
    /// If click occurs, invoke click action
    /// </summary>
    /// <param name="inputPosition"></param>
    /// <param name="selectedObject"></param>
    /// <param name="onClick"></param>
    public void ProcessInput(Vector3 inputPosition, GameObject selectedObject, Action onClick)
    {
        onClick?.Invoke();
    }

}
