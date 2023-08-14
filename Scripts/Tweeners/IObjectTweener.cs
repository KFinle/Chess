using UnityEngine;

internal interface IObjectTweener
{
    /// <summary>
    /// Move object to target position
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="targetPosition"></param>
    void MoveTo(Transform transform, Vector3 targetPosition);
}
