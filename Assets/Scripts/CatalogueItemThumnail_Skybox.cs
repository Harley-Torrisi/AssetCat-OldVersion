using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CatalogueItemThumnail_Skybox : MonoBehaviour {

    private CatalogueItem_Skybox tile = new CatalogueItem_Skybox();

    public Text lable;
    public RawImage thumbnail;
    public int assetID;

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
        CatalogueItem_Skybox item = parse as CatalogueItem_Skybox;
        this.tile = item;
        lable.text = item.friendlyName;
        assetID = item.itemID;
        favourite = item.favourite;
        if (!string.IsNullOrEmpty(item.textureFrontPath))
        {
            WWW www = new WWW("file:///" + Application.persistentDataPath + item.textureFrontPath);
            thumbnail.texture = www.texture;
        }
    }
    public void ShowAsset(GameObject assetView)
    {
        GameObject go = Instantiate(assetView);
        go.GetComponent<CatalogueItemManager_Skybox>().ObjectParse(tile, this);
    }
}
