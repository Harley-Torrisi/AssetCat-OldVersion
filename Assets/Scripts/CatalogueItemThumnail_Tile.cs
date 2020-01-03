using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CatalogueItemThumnail_Tile : MonoBehaviour {

    private CatalogueItem_Tile tile = new CatalogueItem_Tile();

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
        CatalogueItem_Tile item = parse as CatalogueItem_Tile;
        this.tile = item;
        lable.text = item._FriendlyName;
        assetID = item._ItemID;
        favourite = item.favourite;
        if (!string.IsNullOrEmpty(Application.persistentDataPath + item._TextureColourPath))
        {
            WWW www = new WWW("file:///" + Application.persistentDataPath + item._TextureColourPath);
            thumbnail.texture = www.texture;
        }
    }
    public void ShowAsset(GameObject assetView)
    {
        GameObject go = Instantiate(assetView);
        go.GetComponent<CatalogueItemManager_Tile>().ObjectParse(tile, this);
    }
}
