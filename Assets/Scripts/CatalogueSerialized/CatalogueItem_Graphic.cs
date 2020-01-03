using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CatalogueItem_Graphic
{
    public int itemID;
    public string modifiedDate;
    public string friendlyName;

    public string graphicPath;

    public bool isSliced;
    public float slicesX;
    public float slicesY;

    public string[] tags = new string[] { };
    public int itemTypeCategory = 0;
    public bool favourite = false;
}
