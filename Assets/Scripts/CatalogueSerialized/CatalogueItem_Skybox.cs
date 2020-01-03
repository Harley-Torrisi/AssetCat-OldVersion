using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CatalogueItem_Skybox
{
    public int itemID;
    public string modifiedDate;
    public string friendlyName;

    public string textureFrontPath;
    public string textureBackPath;
    public string textureLeftPath;
    public string textureRightPath;
    public string textureUpPath;
    public string textureDownPath;

    public bool isSixSided;

    public string[] tags = new string[] { };
    public int itemTypeCategory = 0;
    public bool favourite = false;

}
