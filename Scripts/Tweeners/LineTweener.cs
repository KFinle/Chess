using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class LineTweener : MonoBehaviour, IObjectTweener
{
    [SerializeField] private float speed;

    /// <summary>
    /// Move object to new position without lifting
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="targetPos"></param>
    public void MoveTo(Transform transform, Vector3 targetPos)
    {
        float distance = Vector3.Distance(targetPos, transform.position);
        transform.DOMove(targetPos, distance / speed);
    }
}
