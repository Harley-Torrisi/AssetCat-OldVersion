using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeAndDestroyImage : MonoBehaviour {

    public float delay = 1;
    private Image image;
    private Color startColour;

	// Use this for initialization
	void Start () {
        image = GetComponent<Image>();
        Destroy(this.gameObject, delay);
        startColour = image.color;
        StartCoroutine(LerpFace());
	}
	
	// Update is called once per frame
	void Update ()
    {
        
	}

    private IEnumerator LerpFace()
    {
        float t = 0f;
        while (t < 1)
        {
            t += Time.deltaTime / delay;
            image.color = Vector4.Lerp(startColour, new Vector4(image.color.r, image.color.g, image.color.b, 0), t);
            yield return null;
        }
    }
}
