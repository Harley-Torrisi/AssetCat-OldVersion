using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CatalogueItem_Tile
{
    public int _ItemID;
    public string _ModifiedDate;
    public string _FriendlyName;

    public string _TextureColourPath;
    public string _TextureNormalPath;
    public string _TextureDisplacementPath;
    public string _TextureSpecularPath;
    public string _TextureOcclusionPath;

    public string[] tags = new string[] { };
    public int itemTypeCategory = 0;
    public bool favourite = false;
}
