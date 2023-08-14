using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderInputReciever : InputReciever
{
    private Vector3 clickPos;

    /// <summary>
    /// Called once per frame.
    /// 
    /// Uses a RayCast to check the position a mouseclick action occurs at
    /// </summary>
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.allCameras[0].ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                clickPos = hit.point;
                OnInputReceived();
            }
        }
    }

    /// <summary>
    /// Handles click actions
    /// </summary>
    public override void OnInputReceived()
    {
        foreach (var handler in inputHandlers)
        {
            handler.ProcessInput(clickPos, null, null);
        }
    }



}
