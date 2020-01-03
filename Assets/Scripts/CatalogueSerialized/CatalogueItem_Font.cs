using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CatalogueItem_Font {

    public int itemID;
    public string modifiedDate;
    public string friendlyName;
    public string[] assetTags;

    public string fontPath;
    public byte[] thumnailData = new byte[] { };

    public string[] tags = new string[] { };
    public int itemTypeCategory = 0;
    public bool favourite = false;

}
