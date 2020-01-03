using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SkyboxCameraRotator : MonoBehaviour {

    public float rotateScensitivity = 0.1f;
    public float fovSensitivity = 5;
    public Transform camTrans;

    private EventTrigger myEventTrigger;
    private Camera thisCam;

    private Vector2Int minMaxFOV = new Vector2Int(40, 160);


    // Use this for initialization
    void Start () {
        thisCam = camTrans.GetComponent<Camera>();
        myEventTrigger = GetComponent<EventTrigger>();

        EventTrigger.Entry drag = new EventTrigger.Entry();
        drag.eventID = EventTriggerType.Drag;
        drag.callback.AddListener((data) => { Dragging(); });

        EventTrigger.Entry scroll = new EventTrigger.Entry();
        scroll.eventID = EventTriggerType.Scroll;
        scroll.callback.AddListener((data) => { Scrolling(); });

        myEventTrigger.triggers.Add(drag);
        myEventTrigger.triggers.Add(scroll);
    }

    private void OnDestroy()
    {
        myEventTrigger.triggers.Clear();
    }


    public void Dragging()
    {
        Vector3 rot = new Vector3(camTrans.eulerAngles.x + -Input.GetAxis("Mouse Y") * rotateScensitivity, camTrans.eulerAngles.y + Input.GetAxis("Mouse X") * rotateScensitivity, 0);
        if (rot != Vector3.zero)
            camTrans.eulerAngles = rot;
    }

    public void Scrolling()
    {
        float speed = Input.GetAxis("Mouse ScrollWheel") * fovSensitivity * Time.deltaTime;
        thisCam.fieldOfView += speed ;
        thisCam.fieldOfView = Mathf.Clamp(camTrans.GetComponent<Camera>().fieldOfView, minMaxFOV.x, minMaxFOV.y);
    }
}
