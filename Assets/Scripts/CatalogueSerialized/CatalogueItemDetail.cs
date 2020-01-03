using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CatalogueItemDetail
{
    public string CatalogueItemDirectory = null;
    public int ItemID;
    public string DateModified = null;

    public enum ItemTypes { Undefined, Model, Audio, Graphic, Tile, Animation, Skybox, Font, CodeSnip }
    public ItemTypes ItemType = ItemTypes.Undefined;
    public int ItemTypeCategory = 0;

    public string FriendlyName = null;
    public string ImagePreviewPath = null;

    //public bool IsFavourite = false;
    //public string LastUpdatedTime = null;
    //public string ParentAssetDirectory = null;
}
