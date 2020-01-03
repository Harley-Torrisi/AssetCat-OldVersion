using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CatalogueItem_Model
{
    public int itemID;
    public string modifiedDate;
    public string friendlyName;

    public string textureColourPath;
    public string textureNormalPath;
    public string textureDisplacementPath;
    public string textureSpecularPath;
    public string textureOcclusionPath;
    public string modelPath;

    public string[] tags = new string[] { };
    public int itemTypeCategory = 0;
    public bool favourite = false;

    public byte[] thumnailData = new byte[] { };

    public CustomVector3 savePos;
    public CustomVector3 saveRot;
    public CustomVector3 saveScale;
}

[System.Serializable]
public class CustomVector3
{
    public float x, y, z;
    public CustomVector3(float X, float Y, float Z)
    {
        x = X;
        y = Y;
        z = Z;
    }
    public CustomVector3(Vector3 vector3)
    {
        x = vector3.x;
        y = vector3.y;
        z = vector3.z;
    }
    public Vector3 GetVector3()
    {
        return new Vector3(x, y, z);
    }
}
