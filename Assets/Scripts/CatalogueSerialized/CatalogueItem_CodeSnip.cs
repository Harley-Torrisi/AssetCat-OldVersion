using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CatalogueItem_CodeSnip
{
    public int _ItemID;
    public string _ModifiedDate;
    public string _FriendlyName;

    public string _CodeText;

    public string[] tags = new string[] { };
    public int itemTypeCategory = 0;
    public bool favourite = false;
}
