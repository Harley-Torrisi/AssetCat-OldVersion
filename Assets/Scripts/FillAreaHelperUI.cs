using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FillAreaHelperUI : MonoBehaviour {

    public RectTransform parentTrans;
    public List<RectTransform> brotherTrans;
    private RectTransform myTrans;
    public float _Padding = 5;

    public enum FillDirections
    {
        Horizontal,
        Vertical,
        None
    }   public FillDirections _FillDirection = FillDirections.None;

    void Start ()
    {
        myTrans = GetComponent<RectTransform>();
	}

	void Update ()
    {
        float value = parentTrans.rect.width;
        switch (_FillDirection)
        {
            case FillDirections.Horizontal:
                foreach (RectTransform rect in brotherTrans)
                    value = value - rect.rect.width - _Padding;
                myTrans.sizeDelta = new Vector2(value, myTrans.sizeDelta.y);
                break;
            case FillDirections.Vertical:
                foreach (RectTransform rect in brotherTrans)
                    value = value - rect.rect.height - _Padding;
                myTrans.sizeDelta = new Vector2(myTrans.sizeDelta.x, value);
                break;
        }
    }
}
