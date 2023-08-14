using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInputHandler
{
    /// <summary>
    /// Handles input actions
    /// </summary>
    /// <param name="inputPosition">Where the input occurs</param>
    /// <param name="selectedObject">What object is selected</param>
    /// <param name="onClick">Action to occur onCLick</param>
    void ProcessInput(Vector3 inputPosition, GameObject selectedObject, Action onClick);
}
