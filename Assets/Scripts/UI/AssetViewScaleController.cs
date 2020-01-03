using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AssetViewScaleController : MonoBehaviour {

    GridLayoutGroup myGrid;
    Slider zoomSlider;
    private Vector2 baseGridScale;

	// Use this for initialization
	void Start () {
        myGrid = GetComponent<GridLayoutGroup>();
        baseGridScale = myGrid.cellSize;
        float startScale = PlayerPrefs.GetFloat("AssetViewScale");
        zoomSlider = GameObject.Find("AssetViewScale").GetComponent<Slider>();
        if (startScale != 0 && startScale != 1)
            zoomSlider.value = startScale;    
	}
	
	public void SetZoomScale(float multiplier)
    {
        myGrid.cellSize = baseGridScale * multiplier;
        zoomSlider.value = multiplier;
        PlayerPrefs.SetFloat("AssetViewScale", multiplier);
        PlayerPrefs.Save();
    }

    public void SetZoomScale(Slider multiplier)
    {
        myGrid.cellSize = baseGridScale * multiplier.value;
        PlayerPrefs.SetFloat("AssetViewScale", multiplier.value);
        PlayerPrefs.Save();
    }
}
