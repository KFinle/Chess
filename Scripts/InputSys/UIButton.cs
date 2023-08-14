using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIInputReciever))]
public class UIButton : Button
{
    private InputReciever reciever;

    /// <summary>
    /// Called when activated.
    /// 
    /// Adds click handling to button
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        reciever = GetComponent<UIInputReciever>();
        onClick.AddListener(() => reciever.OnInputReceived());
    }
}
