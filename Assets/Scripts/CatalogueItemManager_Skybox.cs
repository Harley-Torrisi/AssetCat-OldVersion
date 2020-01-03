using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Crosstales.FB;
using Dropbox.Api;
using System.IO;
using System;
using UnityEngine.Networking;

public class CatalogueItemManager_Skybox : MonoBehaviour {

    private ExtensionFilter[] extensions = new[] { new ExtensionFilter("All Supported Formats", "png", "jpg", "jpeg", "bmp") };

    private CatalogueItem_Skybox itemSkybox = new CatalogueItem_Skybox();
    private CatalogueItemThumnail_Skybox itemThumbnail;
    private CatalogueManager catalogueManager;

    private Material skyboxMatSixClone;
    private Material skyboxMatOneClone;
    private Skybox skybox = new Skybox();
    private string tempDataPath;
    private string
        tempTextureFrontPath,
        tempTextureBackPath,
        tempTextureLeftPath,
        tempTextureRightPath,
        tempTextureUpPath,
        tempTextureDownPath;

    public Button
        buttonImportFront,
        buttonImportBack,
        buttonImportLeft,
        buttonImportRight,
        buttonImportUp,
        buttonImportDown;
    public Toggle skyboxTypeToggle;
    public Camera skyboxCamera;
    public InputField assetFriendlyName;
    public InputField tagsInputField;
    public Toggle favouritesToggle;
    public Texture fallbackTexture;
    public Texture fallbackButtonTexture;
    public Material skyboxMatSix;
    public Material skyboxMatOne;
    public GameObject texLoadViewGameObject;

    public List<Button> disableButtons = new List<Button>();

    public void ObjectParse(CatalogueItem_Skybox skyboxParse, CatalogueItemThumnail_Skybox thumbnail)
    {
        //skybox = skyboxCamera.GetComponent<Skybox>();
        //skyboxMatSixClone = new Material(skyboxMatSix);
        //skyboxMatOneClone = new Material(skyboxMatOne);
        this.itemSkybox = skyboxParse;
        if (itemSkybox.itemID != 0)
        {
            //Existing Asset
            itemThumbnail = thumbnail;
            assetFriendlyName.text = itemSkybox.friendlyName;
            tagsInputField.text = string.Join("#", itemSkybox.tags);
            favouritesToggle.isOn = itemSkybox.favourite;
            skyboxTypeToggle.isOn = !itemSkybox.isSixSided;
            SetButtonLoadAccesLevel(ButtonAccessLevel.None, false);
            ReloadTexturesFromFile();
            texLoadViewGameObject.SetActive(false);
        }
    }

    private void Awake()
    {
        catalogueManager = GameObject.FindWithTag("CatalogueManager").GetComponent<CatalogueManager>();
        tempDataPath = Application.persistentDataPath + "/Temp/" + Guid.NewGuid().ToString() + "/";
        skyboxMatSixClone = new Material(skyboxMatSix);
        skyboxMatOneClone = new Material(skyboxMatOne);
        skybox = skyboxCamera.GetComponent<Skybox>();
        skybox.material = skyboxTypeToggle.isOn ? skyboxMatOneClone : skyboxMatSixClone;
        InitializeSkybox();
    }

    private void InitializeSkybox()
    {
        SetSkyboxTexture("_FrontTex", fallbackTexture, skyboxMatSixClone);
        SetSkyboxTexture("_BackTex", fallbackTexture, skyboxMatSixClone);
        SetSkyboxTexture("_LeftTex", fallbackTexture, skyboxMatSixClone);
        SetSkyboxTexture("_RightTex", fallbackTexture, skyboxMatSixClone);
        SetSkyboxTexture("_UpTex", fallbackTexture, skyboxMatSixClone);
        SetSkyboxTexture("_DownTex", fallbackTexture, skyboxMatSixClone);
        SetSkyboxTexture("_MainTex", fallbackTexture, skyboxMatOneClone);
    }

    public async void SaveAssetAsync()
    {
        if (string.IsNullOrEmpty(assetFriendlyName.text))
        {
            MessageBox.Show("Error", "Asset Name Is Missing", () => { });
            return;
        }

        if (itemSkybox.itemID == 0)
            if (string.IsNullOrEmpty(tempTextureFrontPath))
            {
                MessageBox.Show("Error", "Minimum:\nFront Texture Required", () => { });
                return;
            }

        this.GetComponent<Button>().interactable = false;
        LoadingPanelUI loadingPanelUI = GetComponentInChildren<LoadingPanelUI>(true);
        loadingPanelUI.gameObject.SetActive(true);
        loadingPanelUI.ChangeText("Please Wait", "Assets Uploading");

        const string itemFileName = "CatalogueItem.asscat";
        const string thumnailPrefabName = "AssetThumnail_Skybox";

        if (itemSkybox.itemID == 0)
        {
            itemSkybox = new CatalogueItem_Skybox
            {
                friendlyName = assetFriendlyName.text,
                itemID = itemSkybox.itemID,
                modifiedDate = DateTime.Now.ToString(),
                tags = tagsInputField.text.Split('#'),
                favourite = favouritesToggle.isOn,
                itemTypeCategory = skyboxTypeToggle.isOn ? 2 : 1,
                isSixSided = !skyboxTypeToggle.isOn,
            };

            catalogueManager._CreatedAssetCount++;
            itemSkybox.itemID = catalogueManager._CreatedAssetCount;
            CatalogueItemDetail itemDetail = new CatalogueItemDetail
            {
                ItemType = CatalogueItemDetail.ItemTypes.Skybox,
                ItemID = catalogueManager._CreatedAssetCount,
                CatalogueItemDirectory = "/Assets/Skyboxes/" + catalogueManager._CreatedAssetCount.ToString("D5") + "/",
                DateModified = DateTime.Now.ToString(),
                FriendlyName = itemSkybox.friendlyName,
                ItemTypeCategory = skyboxTypeToggle.isOn ? 2 : 1,
            };

            string localAssetPath = "/" + catalogueManager._DatabaseUID + itemDetail.CatalogueItemDirectory + "/";
            cmd_File.DeleteFolder(Application.persistentDataPath + localAssetPath, false);
            Directory.CreateDirectory(Application.persistentDataPath + localAssetPath);

            if (tempTextureFrontPath != null)
            {
                string path = localAssetPath + "/" + Path.GetFileName(tempTextureFrontPath);
                File.Copy(tempTextureFrontPath, Application.persistentDataPath + path, true);
                itemSkybox.textureFrontPath = path;
            }
            if (tempTextureBackPath != null && !skyboxTypeToggle.isOn)
            {
                string path = localAssetPath + "/" + Path.GetFileName(tempTextureBackPath);
                File.Copy(tempTextureBackPath, Application.persistentDataPath + path, true);
                itemSkybox.textureBackPath = path;
            }
            if (tempTextureLeftPath != null && !skyboxTypeToggle.isOn)
            {
                string path = localAssetPath + "/" + Path.GetFileName(tempTextureLeftPath);
                File.Copy(tempTextureLeftPath, Application.persistentDataPath + path, true);
                itemSkybox.textureLeftPath = path;
            }
            if (tempTextureRightPath != null && !skyboxTypeToggle.isOn)
            {
                string path = localAssetPath + "/" + Path.GetFileName(tempTextureRightPath);
                File.Copy(tempTextureRightPath, Application.persistentDataPath + path, true);
                itemSkybox.textureRightPath = path;
            }
            if (tempTextureUpPath != null && !skyboxTypeToggle.isOn)
            {
                string path = localAssetPath + "/" + Path.GetFileName(tempTextureUpPath);
                File.Copy(tempTextureUpPath, Application.persistentDataPath + path, true);
                itemSkybox.textureUpPath = path;
            }
            if (tempTextureDownPath != null && !skyboxTypeToggle.isOn)
            {
                string path = localAssetPath + "/" + Path.GetFileName(tempTextureDownPath);
                File.Copy(tempTextureDownPath, Application.persistentDataPath + path, true);
                itemSkybox.textureDownPath = path;
            }

            cmd_File.SerializeObject(Application.persistentDataPath + localAssetPath, itemFileName, itemSkybox);
            catalogueManager._CatalogueItemDetails.Add(itemDetail);
            catalogueManager.ResyncCatalogueDatabaseAsync();

            using (DropboxClient dbx = new DropboxClient(AvoEx.AesEncryptor.DecryptString(PlayerPrefs.GetString("Token"))))
            {
                if (tempTextureFrontPath != null)
                    await cmd_Dropbox.UploadFileAsync(dbx, tempTextureFrontPath, itemDetail.CatalogueItemDirectory, Path.GetFileName(tempTextureFrontPath));
                if (tempTextureBackPath != null && !skyboxTypeToggle.isOn)
                    await cmd_Dropbox.UploadFileAsync(dbx, tempTextureBackPath, itemDetail.CatalogueItemDirectory, Path.GetFileName(tempTextureBackPath));
                if (tempTextureLeftPath != null && !skyboxTypeToggle.isOn)
                    await cmd_Dropbox.UploadFileAsync(dbx, tempTextureLeftPath, itemDetail.CatalogueItemDirectory, Path.GetFileName(tempTextureLeftPath));
                if (tempTextureRightPath != null && !skyboxTypeToggle.isOn)
                    await cmd_Dropbox.UploadFileAsync(dbx, tempTextureRightPath, itemDetail.CatalogueItemDirectory, Path.GetFileName(tempTextureRightPath));
                if (tempTextureUpPath != null && !skyboxTypeToggle.isOn)
                    await cmd_Dropbox.UploadFileAsync(dbx, tempTextureUpPath, itemDetail.CatalogueItemDirectory, Path.GetFileName(tempTextureUpPath));
                if (tempTextureDownPath != null && !skyboxTypeToggle.isOn)
                    await cmd_Dropbox.UploadFileAsync(dbx, tempTextureDownPath, itemDetail.CatalogueItemDirectory, Path.GetFileName(tempTextureDownPath));

                await cmd_Dropbox.UploadObjAsync(dbx, itemSkybox, itemDetail.CatalogueItemDirectory, itemFileName);
                Debug.Log("LOG:" + DateTime.Now.ToString() + " - " + itemSkybox.friendlyName + " Created");

                MessageBox.Show("Boom Shaka Laka", "Asset Now Added", () =>
                {
                    GetComponent<PopupItemController>().HideDialog(0);
                });
            }

            GameObject go = Instantiate(Resources.Load(thumnailPrefabName) as GameObject, GameObject.FindWithTag("ThumbnailGrid").transform);
            go.SendMessage("ObjectParse", itemSkybox);
        }

        else
        {
            foreach (CatalogueItemDetail itemDetail in catalogueManager._CatalogueItemDetails)
            {
                if (itemDetail.ItemID == itemSkybox.itemID)
                {
                    itemDetail.DateModified = DateTime.Now.ToString();
                    itemSkybox.modifiedDate = DateTime.Now.ToString();
                    itemDetail.FriendlyName = assetFriendlyName.text;
                    itemSkybox.friendlyName = assetFriendlyName.text;
                    itemSkybox.tags = tagsInputField.text.Split('#');
                    itemSkybox.favourite = favouritesToggle.isOn;
                    itemThumbnail.lable.text = assetFriendlyName.text;
                    itemThumbnail.ObjectParse(itemSkybox);
                    catalogueManager.ResyncCatalogueDatabaseAsync();
                    using (DropboxClient dropboxClient = new DropboxClient(AvoEx.AesEncryptor.DecryptString(PlayerPrefs.GetString("Token"))))
                    {
                        await cmd_Dropbox.UploadObjAsync(dropboxClient, itemSkybox, itemDetail.CatalogueItemDirectory, itemFileName);
                        Debug.Log("LOG:" + DateTime.Now.ToString() + " - " + itemSkybox.friendlyName + " Updated");
                        MessageBox.Show("Boom Shaka Laka", "Asset Now Updated", () =>
                        {
                            GetComponent<PopupItemController>().HideDialog(0);
                        });
                        string localPath = Application.persistentDataPath + "/" + catalogueManager._DatabaseUID + itemDetail.CatalogueItemDirectory + "/";
                        cmd_File.SerializeObject(localPath, itemFileName, itemSkybox);
                        return;
                    }
                }
            }
        }
        loadingPanelUI.gameObject.SetActive(false);
        this.GetComponent<Button>().interactable = true;
    }

    public async void DeleteAssetAsync()
    {
        if (itemSkybox.itemID != 0)
        {
            this.GetComponent<Button>().interactable = false;
            LoadingPanelUI loadingPanelUI = GetComponentInChildren<LoadingPanelUI>(true);
            loadingPanelUI.gameObject.SetActive(true);
            loadingPanelUI.ChangeText("Please Wait", "Deleting Asset");

            CatalogueItemDetail thisDetail = new CatalogueItemDetail();
            foreach (CatalogueItemDetail detail in catalogueManager._CatalogueItemDetails)
                if (detail.ItemID == this.itemSkybox.itemID)
                    thisDetail = detail;
            catalogueManager._CatalogueItemDetails.Remove(thisDetail);
            catalogueManager.ResyncCatalogueDatabaseAsync();

            using (DropboxClient dbx = new DropboxClient(AvoEx.AesEncryptor.DecryptString(PlayerPrefs.GetString("Token"))))
            {
                await cmd_Dropbox.DeleteAssetFolderAsync(dbx, "/Assets/Skyboxes/" + thisDetail.ItemID.ToString("D5"));
                Debug.Log("LOG:" + DateTime.Now.ToString() + " - " + itemSkybox.friendlyName + " Deleted");
            }

            foreach (CatalogueItemThumnail_Skybox goTile in GameObject.FindWithTag("ThumbnailGrid").GetComponentsInChildren<CatalogueItemThumnail_Skybox>())
            {
                if (goTile.assetID == this.itemSkybox.itemID)
                {
                    Destroy(goTile.gameObject);
                    break;
                }
            }
            loadingPanelUI.gameObject.SetActive(false);
            MessageBox.Show("Boom Shaka Laka", "Asset Now Deleted", () =>
            {
                GetComponent<PopupItemController>().HideDialog(0);
            });
            string localAssetPath = "/" + catalogueManager._DatabaseUID + "/Assets/Skyboxes/" + this.itemSkybox.itemID.ToString("D5") + "/";
            cmd_File.DeleteFolder(Application.persistentDataPath + localAssetPath, true);
        }
        else
        {
            GetComponent<PopupItemController>().HideDialog(0);
        }
    }

    public void ExportAsset()
    {
        if (itemSkybox.itemID == 0)
        {
            MessageBox.Show("Error", "Save Asset Before Exporting", () => { });
            return;
        }
        string path = FileBrowser.SaveFile("Select Save Location", "", itemSkybox.friendlyName, "zip");
        if (string.IsNullOrEmpty(path))
            return;
        Directory.CreateDirectory(tempDataPath);
        string localAssetPath = "/" + catalogueManager._DatabaseUID + "/Assets/Skyboxes/" + this.itemSkybox.itemID.ToString("D5") + "/";
        string[] files = Directory.GetFiles(Application.persistentDataPath + localAssetPath);
        foreach (string file in files)
        {
            string name = Path.GetFileName(file);
            if (name != "CatalogueItem.asscat")
            {
                string dest = Path.Combine(tempDataPath, name);
                File.Copy(file, dest);
            }
        }
        System.IO.Compression.ZipFile.CreateFromDirectory(tempDataPath, path, System.IO.Compression.CompressionLevel.Fastest, false);
    }

    private void ReloadTexturesFromFile()
    {
        if (!string.IsNullOrEmpty(itemSkybox.textureFrontPath))
        {
            StartCoroutine(SetSkyboxTextureFromURL("_FrontTex", itemSkybox.textureFrontPath, skyboxMatSixClone));
            StartCoroutine(SetSkyboxTextureFromURL("_MainTex", itemSkybox.textureFrontPath, skyboxMatOneClone));
        }
        if (!string.IsNullOrEmpty(itemSkybox.textureBackPath))
        {
            StartCoroutine(SetSkyboxTextureFromURL("_BackTex", itemSkybox.textureBackPath, skyboxMatSixClone));
        }
        if (!string.IsNullOrEmpty(itemSkybox.textureLeftPath))
        {
            StartCoroutine(SetSkyboxTextureFromURL("_LeftTex", itemSkybox.textureLeftPath, skyboxMatSixClone));
        }
        if (!string.IsNullOrEmpty(itemSkybox.textureRightPath))
        {
            StartCoroutine(SetSkyboxTextureFromURL("_RightTex", itemSkybox.textureRightPath, skyboxMatSixClone));
        }
        if (!string.IsNullOrEmpty(itemSkybox.textureUpPath))
        {
            StartCoroutine(SetSkyboxTextureFromURL("_UpTex", itemSkybox.textureUpPath, skyboxMatSixClone));
        }
        if (!string.IsNullOrEmpty(itemSkybox.textureDownPath))
        {
            StartCoroutine(SetSkyboxTextureFromURL("_DownTex", itemSkybox.textureDownPath, skyboxMatSixClone));
        }
    }

    

    public void ImportTexture(string tile)
    {
        string path = FileBrowser.OpenSingleFile("Select Texture", "", extensions);
        Directory.CreateDirectory(tempDataPath);
        if (!string.IsNullOrEmpty(path))
        {
            WWW www = new WWW("file:///" + path);
            File.Copy(path, tempDataPath + Path.GetFileName(path), true);
            if (!string.IsNullOrEmpty(path))
            {
                var attr = File.GetAttributes(tempDataPath + Path.GetFileName(path));
                attr = attr & ~FileAttributes.ReadOnly;
                File.SetAttributes(tempDataPath + Path.GetFileName(path), attr);
            }
            switch (tile)
            {
                case "FRONT":
                    buttonImportFront.GetComponentInChildren<RawImage>().texture = www.texture;
                    SetSkyboxTexture("_FrontTex", www.texture, skyboxMatSixClone);
                    SetSkyboxTexture("_MainTex", www.texture, skyboxMatOneClone);
                    tempTextureFrontPath = tempDataPath + Path.GetFileName(path);
                    break;
                case "BACK":
                    buttonImportBack.GetComponentInChildren<RawImage>().texture = www.texture;
                    SetSkyboxTexture("_BackTex", www.texture, skyboxMatSixClone);
                    tempTextureBackPath = tempDataPath + Path.GetFileName(path);
                    break;
                case "LEFT":
                    buttonImportLeft.GetComponentInChildren<RawImage>().texture = www.texture;
                    SetSkyboxTexture("_LeftTex", www.texture, skyboxMatSixClone);
                    tempTextureLeftPath = tempDataPath + Path.GetFileName(path);
                    break;
                case "RIGHT":
                    buttonImportRight.GetComponentInChildren<RawImage>().texture = www.texture;
                    SetSkyboxTexture("_RightTex", www.texture, skyboxMatSixClone);
                    tempTextureRightPath = tempDataPath + Path.GetFileName(path);
                    break;
                case "UP":
                    buttonImportUp.GetComponentInChildren<RawImage>().texture = www.texture;
                    SetSkyboxTexture("_UpTex", www.texture, skyboxMatSixClone);
                    tempTextureUpPath = tempDataPath + Path.GetFileName(path);
                    break;
                case "DOWN":
                    buttonImportDown.GetComponentInChildren<RawImage>().texture = www.texture;
                    SetSkyboxTexture("_DownTex", www.texture, skyboxMatSixClone);
                    tempTextureDownPath = tempDataPath + Path.GetFileName(path);
                    break;
            }
        }
        else
        {
            switch (tile)
            {
                case "FRONT":
                    tempTextureFrontPath = null;
                    buttonImportFront.GetComponentInChildren<RawImage>().texture = fallbackButtonTexture;
                    SetSkyboxTexture("_FrontTex", fallbackTexture, skyboxMatSixClone);
                    SetSkyboxTexture("_MainTex", fallbackTexture, skyboxMatOneClone);
                    break;
                case "BACK":
                    tempTextureBackPath = null;
                    buttonImportBack.GetComponentInChildren<RawImage>().texture = fallbackButtonTexture;
                    SetSkyboxTexture("_BackTex", fallbackTexture, skyboxMatSixClone);
                    break;
                case "LEFT":
                    tempTextureLeftPath = null;
                    buttonImportLeft.GetComponentInChildren<RawImage>().texture = fallbackButtonTexture;
                    SetSkyboxTexture("_LeftTex", fallbackTexture, skyboxMatSixClone);
                    break;
                case "RIGHT":
                    tempTextureRightPath = null;
                    buttonImportRight.GetComponentInChildren<RawImage>().texture = fallbackButtonTexture;
                    SetSkyboxTexture("_RightTex", fallbackTexture, skyboxMatSixClone);
                    break;
                case "UP":
                    tempTextureUpPath = null;
                    buttonImportUp.GetComponentInChildren<RawImage>().texture = fallbackButtonTexture;
                    SetSkyboxTexture("_UpTex", fallbackTexture, skyboxMatSixClone);
                    break;
                case "DOWN":
                    tempTextureDownPath = null;
                    buttonImportDown.GetComponentInChildren<RawImage>().texture = fallbackButtonTexture;
                    SetSkyboxTexture("_DownTex", fallbackTexture, skyboxMatSixClone);
                    break;
            }
        }
    }

    private void SetSkyboxTexture(string keyword, Texture tex, Material mat)
    {
        mat.SetTexture(keyword, tex);
    }

    private IEnumerator SetSkyboxTextureFromURL(string keyword, string texURL, Material mat)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture("file:///" + Application.persistentDataPath + texURL))
        {
            yield return uwr.SendWebRequest();
            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                // Get downloaded asset bundle
                Texture2D tex = DownloadHandlerTexture.GetContent(uwr);
                mat.SetTexture(keyword, tex);
            }
        }
    }

    public void SetSkyboxType(Toggle toggle)
    {
        skybox.material = toggle.isOn ? skyboxMatOneClone : skyboxMatSixClone;
        SetButtonLoadAccesLevel(toggle.isOn ? ButtonAccessLevel.Single : ButtonAccessLevel.All);
        skyboxCamera.fieldOfView = toggle.isOn ? 120 : skyboxCamera.fieldOfView;
    }

    private enum ButtonAccessLevel { All, Single, None }
    private void SetButtonLoadAccesLevel(ButtonAccessLevel level, bool showToggle = true)
    {
        switch (level)
        {
            case ButtonAccessLevel.All:
                buttonImportFront.interactable = true;
                buttonImportBack.interactable = true;
                buttonImportLeft.interactable = true;
                buttonImportRight.interactable = true;
                buttonImportUp.interactable = true;
                buttonImportDown.interactable = true;
                break;
            case ButtonAccessLevel.Single:
                buttonImportFront.interactable = true;
                buttonImportBack.interactable = false;
                buttonImportLeft.interactable = false;
                buttonImportRight.interactable = false;
                buttonImportUp.interactable = false;
                buttonImportDown.interactable = false;
                break;
            case ButtonAccessLevel.None:
                buttonImportFront.interactable = false;
                buttonImportBack.interactable = false;
                buttonImportLeft.interactable = false;
                buttonImportRight.interactable = false;
                buttonImportUp.interactable = false;
                buttonImportDown.interactable = false;
                break;
        }
        skyboxTypeToggle.gameObject.SetActive(showToggle);
    }
}
