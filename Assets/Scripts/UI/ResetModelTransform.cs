using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
[RequireComponent(typeof(EventTrigger))]

public class ResetModelTransform : MonoBehaviour {

    public GameObject parentObject;
    private EventTrigger myEventTrigger;

    private Vector3 startPos = new Vector3(0, -30, 100);

    void Start ()
    {
        myEventTrigger = GetComponent<EventTrigger>();

        EventTrigger.Entry mouseClick = new EventTrigger.Entry();
        mouseClick.eventID = EventTriggerType.PointerUp;
        mouseClick.callback.AddListener((data) => { Clicked(); });

        myEventTrigger.triggers.Add(mouseClick);
    }

    public void Clicked()
    {
        if (Input.GetMouseButtonUp(1))
        {
            parentObject.transform.localPosition = startPos;
            parentObject.transform.localScale = Vector3.one;
            parentObject.transform.eulerAngles = Vector3.zero;
        }
    }
}
