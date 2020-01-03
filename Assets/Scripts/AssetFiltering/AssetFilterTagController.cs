using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class AssetFilterTagController : MonoBehaviour {

    public GameObject tageButtonTemplate;
    public Transform tagsObjectParent;
    private AssetFilterManager filterManager;
    private List<string> tags;

    private void Start()
    {
        filterManager = GameObject.FindGameObjectWithTag("ThumbnailGrid").GetComponent<AssetFilterManager>();
    }

    public void GetAndPopulateTags()
    {
        foreach (Transform trans in tagsObjectParent)
            Destroy(trans.gameObject);
        tags = filterManager.GetAllChildsTags();
        tags = tags.Distinct().ToList();
        foreach (string tag in tags)
        {
            if (!string.IsNullOrEmpty(tag))
            {
                GameObject go = Instantiate(tageButtonTemplate, tagsObjectParent);
                go.SetActive(true);
                go.GetComponentInChildren<Text>().text = "#" + tag;
            }
        }
    }

}
