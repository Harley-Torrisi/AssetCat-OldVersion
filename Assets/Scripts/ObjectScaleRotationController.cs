using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
[RequireComponent(typeof(EventTrigger))]

public class ObjectScaleRotationController : MonoBehaviour {

    public bool singleRotate = true;

    EventTrigger myEventTrigger;

    public float rotateScensitivity = 0.1f;
    public float scaleSensitivity = 5;
    public float moveSensitivity = 0.02f;

    public List<Transform> rotatableObjects = new List<Transform>();

    private Vector3 _mouseReference;
    private Vector3 _mouseOffset;
    private Vector3 _rotation = Vector3.zero;
    
    void Start()
    {
        myEventTrigger = GetComponent<EventTrigger>();

        EventTrigger.Entry beginDrag = new EventTrigger.Entry();
        beginDrag.eventID = EventTriggerType.BeginDrag;
        beginDrag.callback.AddListener((data) => { BeginDrag(); });

        EventTrigger.Entry drag = new EventTrigger.Entry();
        drag.eventID = EventTriggerType.Drag;
        drag.callback.AddListener((data) => { Dragging(); });

        EventTrigger.Entry scroll = new EventTrigger.Entry();
        scroll.eventID = EventTriggerType.Scroll;
        scroll.callback.AddListener((data) => { Scrolling(); });

        myEventTrigger.triggers.Add(beginDrag);
        myEventTrigger.triggers.Add(drag);
        myEventTrigger.triggers.Add(scroll);
    }

    private void OnDestroy()
    {
        myEventTrigger.triggers.Clear();
    }


    public void BeginDrag()
    {
        _mouseReference = Input.mousePosition;
    }

    public void Dragging()
    {
        _mouseOffset = (Input.mousePosition - _mouseReference);

        if (Input.GetMouseButton(2))
        {
            _rotation.x = _mouseOffset.x * moveSensitivity;
            _rotation.y = _mouseOffset.y * moveSensitivity;
            if (singleRotate)
            {
                foreach (Transform trans in rotatableObjects)
                    if (trans.GetComponent<Renderer>().enabled)
                        trans.Translate(_rotation, Space.World);
            }
            else
            {
                foreach (Transform trans in rotatableObjects)
                    trans.Translate(_rotation, Space.World);
            }
            
        } else
        {
            _rotation.y = -_mouseOffset.x * rotateScensitivity;
            _rotation.x = _mouseOffset.y * rotateScensitivity;
            if (singleRotate)
            {
                foreach (Transform trans in rotatableObjects)
                    if (trans.GetComponent<Renderer>().enabled)
                        trans.Rotate(_rotation, Space.World);
            }
            else
            {
                foreach (Transform trans in rotatableObjects)
                    trans.Rotate(_rotation, Space.World);
            }
        }

        
        
        _mouseReference = Input.mousePosition;
    }

    public void Scrolling()
    {
        float scaleSpeed = Input.GetAxis("Mouse ScrollWheel") * scaleSensitivity;
        if (singleRotate)
        {
            foreach (Transform trans in rotatableObjects)
                if (trans.GetComponent<Renderer>().enabled)
                    trans.localScale = new Vector3(
                        trans.localScale.x - scaleSpeed,
                        trans.localScale.y - scaleSpeed,
                        trans.localScale.z - scaleSpeed);
        }
        else
        {
            foreach (Transform trans in rotatableObjects)
                trans.localScale = new Vector3(
                    trans.localScale.x - scaleSpeed,
                    trans.localScale.y - scaleSpeed,
                    trans.localScale.z - scaleSpeed);
        }
        
    }

}
