﻿using System;
using UnityEngine;
using UnityEngine.UI;

public class MessageBoxDialog : MonoBehaviour
{

    private Animator myAnim;
    public Text titleText, messageText;
    public event Action OnDone;

    void Start()
    {
        this.gameObject.SetActive(true);
        myAnim = GetComponent<Animator>();
        myAnim.SetTrigger("Show");
    }

    public void HideDialog(float HideDelay = 0)
    {
        Invoke("Hide", HideDelay);
    }

    private void Hide()
    {
        myAnim.SetTrigger("Hide");
    }

    private void Destroy()
    {
        OnDone();
        OnDone = null;
        Destroy(this.gameObject);
    }
}