using Dropbox.Api;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CatalogueManagerItems : MonoBehaviour {

    private CatalogueManager catalogueManager;

    private object currentCatalogueItem;

    private Transform _LoadContainer;

    //Grid for displaying asset thumnails
    private GameObject _ThumnailViewGrid;

    //Thunails for asset view
    //public GameObject _AssetThumnailCodesnip;

    //Stored Asset List
    private List<GameObject> _AssetThumnailsList = new List<GameObject>();

    private DropboxClient dropboxClient;

    private void OnEnable()
    {
        catalogueManager = GetComponent<CatalogueManager>();
        SceneManager.sceneLoaded += OnSceneLoaded;
        _LoadContainer = GetComponentInChildren<Transform>();
        dropboxClient = new DropboxClient(AvoEx.AesEncryptor.DecryptString(PlayerPrefs.GetString("Token")));
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        
        switch (scene.name)
        {
            case "01_SceneAuthentication":
                _AssetThumnailsList = new List<GameObject>();
                break;
            case "03_SceneMainApplication":
                _ThumnailViewGrid = GameObject.FindWithTag("ThumbnailGrid");
                foreach (GameObject go in _AssetThumnailsList)
                    go.transform.SetParent(_ThumnailViewGrid.transform);
                break;
        }
    }

    public async Task LoadAssets_Async(List<CatalogueItemDetail> itemDetails)
    {
        bool curruptedFound = false;
        List<CatalogueItemDetail> currptedDetails = new List<CatalogueItemDetail>();

        Image _loadingBarFill = GameObject.Find("LoadingBarFill").GetComponent<Image>();
        Text _LoadingBartext = GameObject.Find("LoadingBarText").GetComponent<Text>();
        int count = 1;
        foreach (CatalogueItemDetail itemDetail in itemDetails)
        {
            float amount = (float)count / (float)itemDetails.Count;
            _loadingBarFill.fillAmount = amount;
            _LoadingBartext.text = "Proccessing: " + itemDetail.FriendlyName;
            try
            {
                switch (itemDetail.ItemType)
                {
                    case CatalogueItemDetail.ItemTypes.CodeSnip:
                        await LoadAsset_Async(itemDetail, "CatalogueItem.asscat", "AssetThumnail_ConeSnip");
                        break;
                    case CatalogueItemDetail.ItemTypes.Tile:
                        await LoadAsset_Async(itemDetail, "CatalogueItem.asscat", "AssetThumnail_Tile");
                        break;
                    case CatalogueItemDetail.ItemTypes.Audio:
                        await LoadAsset_Async(itemDetail, "CatalogueItem.asscat", "AssetThumnail_Audio");
                        break;
                    case CatalogueItemDetail.ItemTypes.Font:
                        await LoadAsset_Async(itemDetail, "CatalogueItem.asscat", "AssetThumnail_Font");
                        break;
                    case CatalogueItemDetail.ItemTypes.Skybox:
                        await LoadAsset_Async(itemDetail, "CatalogueItem.asscat", "AssetThumnail_Skybox");
                        break;
                    case CatalogueItemDetail.ItemTypes.Graphic:
                        await LoadAsset_Async(itemDetail, "CatalogueItem.asscat", "AssetThumnail_Graphic");
                        break;
                    case CatalogueItemDetail.ItemTypes.Model:
                        await LoadAsset_Async(itemDetail, "CatalogueItem.asscat", "AssetThumnail_Model");
                        break;
                }
            }
            catch
            {
                curruptedFound = true;
                currptedDetails.Add(itemDetail);
                Debug.Log("Currupted Asset Found, Removing...");
            }
            count++;
        }
        if (curruptedFound)
        {
            foreach (CatalogueItemDetail itemDetail in currptedDetails)
            {
                catalogueManager._CatalogueItemDetails.Remove(itemDetail);
                await DeleteCurrptedFiles(itemDetail);
            }  
            catalogueManager.ResyncCatalogueDatabaseAsync();
        }
    }

    public async Task LoadAsset_Async(CatalogueItemDetail itemDetail, string itemFileName, string thumnailPrefabName)
    {
        currentCatalogueItem = null;
        string localPath = Application.persistentDataPath + "/" + catalogueManager._DatabaseUID + itemDetail.CatalogueItemDirectory + "/";
        if (cmd_File.FileExists(localPath, itemFileName))
        {
            object itemLocal = cmd_File.DeserializeObject(localPath, itemFileName);
            switch (itemDetail.ItemType)
            {
                case CatalogueItemDetail.ItemTypes.CodeSnip:
                    if (itemDetail.DateModified == (itemLocal as CatalogueItem_CodeSnip)._ModifiedDate)
                        currentCatalogueItem = itemLocal;
                    else
                    {
                        Debug.Log("downloading: " + itemDetail.ItemID + " - " + itemDetail.FriendlyName);
                        await DownloadAssetAsnc(itemDetail, localPath, itemFileName);
                    }
                    break;
                case CatalogueItemDetail.ItemTypes.Tile:
                    if (itemDetail.DateModified == (itemLocal as CatalogueItem_Tile)._ModifiedDate)
                        currentCatalogueItem = itemLocal;
                    else
                    {
                        Debug.Log("downloading: " + itemDetail.ItemID + " - " + itemDetail.FriendlyName);
                        await DownloadAssetAsnc(itemDetail, localPath, itemFileName);
                    }
                    break;
                case CatalogueItemDetail.ItemTypes.Audio:
                    if (itemDetail.DateModified == (itemLocal as CatalogueItem_Audio).modifiedDate)
                        currentCatalogueItem = itemLocal;
                    else
                    {
                        Debug.Log("downloading: " + itemDetail.ItemID + " - " + itemDetail.FriendlyName);
                        await DownloadAssetAsnc(itemDetail, localPath, itemFileName);
                    }
                    break;
                case CatalogueItemDetail.ItemTypes.Font:
                    if (itemDetail.DateModified == (itemLocal as CatalogueItem_Font).modifiedDate)
                        currentCatalogueItem = itemLocal;
                    else
                    {
                        Debug.Log("downloading: " + itemDetail.ItemID + " - " + itemDetail.FriendlyName);
                        await DownloadAssetAsnc(itemDetail, localPath, itemFileName);
                    }
                    break;
                case CatalogueItemDetail.ItemTypes.Skybox:
                    if (itemDetail.DateModified == (itemLocal as CatalogueItem_Skybox).modifiedDate)
                        currentCatalogueItem = itemLocal;
                    else
                    {
                        Debug.Log("downloading: " + itemDetail.ItemID + " - " + itemDetail.FriendlyName);
                        await DownloadAssetAsnc(itemDetail, localPath, itemFileName);
                    }
                    break;
                case CatalogueItemDetail.ItemTypes.Graphic:
                    if (itemDetail.DateModified == (itemLocal as CatalogueItem_Graphic).modifiedDate)
                        currentCatalogueItem = itemLocal;
                    else
                    {
                        Debug.Log("downloading: " + itemDetail.ItemID + " - " + itemDetail.FriendlyName);
                        await DownloadAssetAsnc(itemDetail, localPath, itemFileName);
                    }
                    break;
                case CatalogueItemDetail.ItemTypes.Model:
                    if (itemDetail.DateModified == (itemLocal as CatalogueItem_Model).modifiedDate)
                        currentCatalogueItem = itemLocal;
                    else
                    {
                        Debug.Log("downloading: " + itemDetail.ItemID + " - " + itemDetail.FriendlyName);
                        await DownloadAssetAsnc(itemDetail, localPath, itemFileName);
                    }
                    break;
            }
        }
        else
        {
            await DownloadAssetAsnc(itemDetail, localPath, itemFileName);
        }
        if (currentCatalogueItem == null)
            return;
        GameObject go = Instantiate(Resources.Load(thumnailPrefabName) as GameObject, _LoadContainer);
        go.SendMessage("ObjectParse", currentCatalogueItem);
        _AssetThumnailsList.Add(go);
    }

    private async Task DownloadAssetAsnc(CatalogueItemDetail itemDetail, string localPath, string itemFileName)
    {
        cmd_File.DeleteFolder(localPath, false);
        byte[] data = await cmd_Dropbox.DownloadFolderAsync(dropboxClient, itemDetail.CatalogueItemDirectory);
        Directory.CreateDirectory(localPath.Replace(itemDetail.ItemID.ToString("D5"), ""));
        string zipPath = localPath.Replace(itemDetail.ItemID.ToString("D5"), "") + itemDetail.ItemID.ToString("D5") + ".zip";
        File.WriteAllBytes(zipPath, data);
        ZipFile.ExtractToDirectory(zipPath, localPath.Replace(itemDetail.ItemID.ToString("D5"), ""));
        File.Delete(zipPath);
        currentCatalogueItem = cmd_File.DeserializeObject(localPath, itemFileName);
    }

    private async Task DeleteCurrptedFiles(CatalogueItemDetail itemDetail)
    {
        string localPath = Application.persistentDataPath + "/" + catalogueManager._DatabaseUID + itemDetail.CatalogueItemDirectory + "/";
        cmd_File.DeleteFolder(localPath, false);
        Debug.Log(itemDetail.CatalogueItemDirectory.Remove(itemDetail.CatalogueItemDirectory.Length - 1));
        await cmd_Dropbox.DeleteAssetFolderAsync(dropboxClient, itemDetail.CatalogueItemDirectory.Remove(itemDetail.CatalogueItemDirectory.Length - 1));
    }
}
