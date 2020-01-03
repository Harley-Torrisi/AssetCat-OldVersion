using Dropbox.Api;
using Dropbox.Api.Files;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CatalogueManager : MonoBehaviour
{
    private CatalogueDatabase catalogueDatabase = new CatalogueDatabase();
    private CatalogueManagerItems catalogueManagerItems;
    private DropboxClient dropboxClient;


    /// <summary>
    /// Tunnel to access private catalogueDatabase's UID, used for identifying this databse
    /// </summary>
    public string _DatabaseUID
    { get { return catalogueDatabase.uniqueID; } set { catalogueDatabase.uniqueID = value; } }
    /// <summary>
    /// Tunnel to access private catalogueDatabase's Total Assets Created, used for seting ID of new assets
    /// </summary>
    public int _CreatedAssetCount
    { get { return catalogueDatabase.createdAssetCount; }set { catalogueDatabase.createdAssetCount = value; } }    
    /// <summary>
    /// Tunnel to access private catalogueDatabase's list of Asset Item Details, used for refernecinf stored assets
    /// </summary>
    public List<CatalogueItemDetail> _CatalogueItemDetails
    { get { return catalogueDatabase.catalogueItemDetails; } set { catalogueDatabase.catalogueItemDetails = value; } }

    private void OnEnable()
    {
        dropboxClient = new DropboxClient(AvoEx.AesEncryptor.DecryptString(PlayerPrefs.GetString("Token")));
        DontDestroyOnLoad(this);
        SceneManager.sceneLoaded += OnSceneLoadedAsync;
        catalogueManagerItems = GetComponent<CatalogueManagerItems>();               
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoadedAsync;
    }

    public async void OnSceneLoadedAsync(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "01_SceneAuthentication":
                //Destroy(this.gameObject);
                break;
            case "02_DataLoad":
                await InitializeDatabase();
                await catalogueManagerItems.LoadAssets_Async(catalogueDatabase.catalogueItemDetails);
                SceneManager.LoadScene("03_SceneMainApplication");
                break;
            case "03_SceneMainApplication":
                Destroy(GameObject.Find("StartHerePanel"));
                break;
        }
    }

    /// <summary>
    /// InitializeDatabase Creates and Loads The Catalogue.db from Dropbox
    /// </summary>
    /// <returns></returns>
    private async Task InitializeDatabase()
    {
        cmd_File.DeleteFolder(Application.persistentDataPath + "/Temp", false);
        Stream stream = await cmd_Dropbox.DownloadAsync(dropboxClient, "/AppData/", "Catalogue.db");
        if (stream != null)
        {
            Debug.Log("LOG: Catalouge found");
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            catalogueDatabase = binaryFormatter.Deserialize(stream) as CatalogueDatabase;
            Debug.Log("Database Information...\n" + "Last Update: " + catalogueDatabase.lastUpdatedTime + "  |  Asset Count: " + catalogueDatabase.catalogueItemDetails.Count() + "  |  Total Assets Created: " + catalogueDatabase.createdAssetCount);
        }
        else
        {
            Debug.Log("LOG: Catalouge not found, now creating");
            catalogueDatabase = new CatalogueDatabase
            {
                uniqueID = Guid.NewGuid().ToString(),
                catalogueItemDetails = new List<CatalogueItemDetail>(),
                lastUpdatedTime = DateTime.Now.ToString()
            };
            Debug.Log((await cmd_Dropbox.UploadObjAsync(dropboxClient, catalogueDatabase, "/AppData/", "Catalogue.db") != null)
                ? "LOG: Catalouge created" : "LOG: Catalouge failed to create");
        }
    }

    /// <summary>
    /// Used to update Catalogue.db after changes are made to Catalogue.db
    /// </summary>
    public async void ResyncCatalogueDatabaseAsync()
    {
        catalogueDatabase.lastUpdatedTime = DateTime.Now.ToString();
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        using (MemoryStream memoryStream = new MemoryStream())
        {
            binaryFormatter.Serialize(memoryStream, catalogueDatabase);
            memoryStream.Position = 0;
            await dropboxClient.Files.UploadAsync("/AppData/" + "Catalogue.db", WriteMode.Overwrite.Instance, body: memoryStream);
        }
    }
}
