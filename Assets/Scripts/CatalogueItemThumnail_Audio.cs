using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CatalogueItemThumnail_Audio : MonoBehaviour {

    private CatalogueItem_Audio tile = new CatalogueItem_Audio();

    public int assetID;
    public Text lable;
    public Image thumbnail;
    public bool favourite;
    
    public int GetSubCategory
    {
        get { return tile.itemTypeCategory; }
    }

    public string[] GetTags
    {
        get { return tile.tags; }
    }

    public void ObjectParse(object parse)
    {
        CatalogueItem_Audio item = parse as CatalogueItem_Audio;
        this.tile = item;
        lable.text = item.friendlyName;
        assetID = item.itemID;
        favourite = item.favourite;
        if (item.thumnailData != null)
        {
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(item.thumnailData);
            Sprite mySprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
            thumbnail.sprite = mySprite;
        }
    }

    public void ShowAsset(GameObject assetView)
    {
        GameObject go = Instantiate(assetView);
        go.GetComponent<CatalogueItemManager_Audio>().ObjectParse(tile, this);
    }
}
