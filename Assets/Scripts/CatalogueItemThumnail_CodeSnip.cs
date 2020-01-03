using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CatalogueItemThumnail_CodeSnip : MonoBehaviour {

    private CatalogueItem_CodeSnip codeSnip = new CatalogueItem_CodeSnip();

    public int assetID;
    public Text lable;
    public bool favourite;

    public int GetSubCategory
    {
        get { return codeSnip.itemTypeCategory; }
    }

    public string[] GetTags
    {
        get { return codeSnip.tags; }
    }

    public void ObjectParse(object parse)
    {
        CatalogueItem_CodeSnip item = parse as CatalogueItem_CodeSnip;
        this.codeSnip = item;
        lable.text = item._FriendlyName;
        favourite = item.favourite;
        assetID = item._ItemID;
    }

    public void ShowAsset(GameObject assetView)
    {
        GameObject go = Instantiate(assetView);
        go.GetComponent<CatalogueItemManager_CodeSnip>().ObjectParse(codeSnip, this);
    }
}
