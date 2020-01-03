using System.Collections.Generic;

[System.Serializable]
public class CatalogueDatabase
{
    public string uniqueID;
    public string lastUpdatedTime = null;
    public int createdAssetCount = 0; // toString('D5');//
    public List<CatalogueItemDetail> catalogueItemDetails = new List<CatalogueItemDetail>();
}
