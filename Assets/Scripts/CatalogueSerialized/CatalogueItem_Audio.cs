using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CatalogueItem_Audio {

    public int itemID;
    public string modifiedDate;
    public string friendlyName;
    public string[] tags = new string[] { };
    public string audioPath;
    public byte[] thumnailData = new byte[] { };
    public int itemTypeCategory = 0;
    public bool favourite = false;
}
