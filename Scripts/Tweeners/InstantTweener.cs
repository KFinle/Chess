using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantTweener : MonoBehaviour, IObjectTweener
{
    /// <summary>
    /// Immediately move piece to new location
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="targetPosition"></param>
    public void MoveTo(Transform transform, Vector3 targetPosition)
    {
        transform.position = targetPosition;
    }
}
