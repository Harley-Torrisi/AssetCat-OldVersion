using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CatalogueItemThumnail_Graphic : MonoBehaviour {

    private CatalogueItem_Graphic tile = new CatalogueItem_Graphic();

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
        CatalogueItem_Graphic item = parse as CatalogueItem_Graphic;
        this.tile = item;
        lable.text = item.friendlyName;
        assetID = item.itemID;
        favourite = item.favourite;
        if (!string.IsNullOrEmpty(item.graphicPath))
        {
            StartCoroutine(SetGraphicTexture(Application.persistentDataPath + item.graphicPath, thumbnail));
        }
    }

    private IEnumerator SetGraphicTexture(string path, RawImage imageTex)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture("file:///" + path))
        {
            yield return uwr.SendWebRequest();
            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                // Get downloaded asset bundle
                Texture tex = DownloadHandlerTexture.GetContent(uwr);
                imageTex.texture = tex;
            }
        }
    }
    public void ShowAsset(GameObject assetView)
    {
        GameObject go = Instantiate(assetView);
        go.GetComponent<CatalogueItemManager_Graphic>().ObjectParse(tile, this);
    }
}
