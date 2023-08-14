using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ArcTweener : MonoBehaviour, IObjectTweener
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private float height;

    /// <summary>
    /// Move object in an arc to the target location
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="targetPos"></param>
    public void MoveTo(Transform transform, Vector3 targetPos)
    {
        float distance = Vector3.Distance(targetPos, transform.position);
        transform.DOJump(targetPos, height, 1, distance / moveSpeed);
    }
}
