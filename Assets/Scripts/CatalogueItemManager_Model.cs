using Crosstales.FB;
using Dropbox.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TriLib;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CatalogueItemManager_Model : MonoBehaviour
{
    private ExtensionFilter[] extensionsModels = new[] { new ExtensionFilter("All Supported Formats", "obj", "fbx", "blend", "dae", "3ds") };
    private ExtensionFilter[] extensionsTextures = new[] { new ExtensionFilter("All Supported Formats", "png", "jpg", "jpeg", "bmp") };

    private CatalogueManager catalogueManager;
    private CatalogueItemThumnail_Model itemThumbnail;
    private CatalogueItem_Model itemModel = new CatalogueItem_Model();

    private string tempDataPath;
    private string
        tempTextureColourPath,
        tempTextureNormalPath,
        tempTextureDisplacementPath,
        tempTextureSpecularPath,
        tempTextureOcclusionPath,
        tempModelPath;


    public GameObject modelViewParent;

    public RenderTexture renderTexture;
    public RectTransform renderTextureParent;
    public Camera renderCamera;
    public RawImage renderTextureImage;
    private RenderTexture renderTextureNew;

    public Material placeHolderMaterial;

    public RawImage thumnailColour;
    public RawImage thumnailNormal;
    public RawImage thumnailDisplacement;
    public RawImage thumnailSpecular;
    public RawImage thumnailOcclusion;

    public InputField assetFriendlyName;
    public InputField tagsInputField;
    public Dropdown categoryDropDown;
    public Toggle favouritesToggle;
    public GameObject editViewObject;

    public void ObjectParse(CatalogueItem_Model tileParse, CatalogueItemThumnail_Model thumbnail)
    {
        this.itemModel = tileParse;
        if (itemModel.itemID != 0)
        {
            //Existing Asset
            itemThumbnail = thumbnail;
            assetFriendlyName.text = itemModel.friendlyName;
            tagsInputField.text = string.Join("#", itemModel.tags);
            categoryDropDown.value = itemModel.itemTypeCategory;
            favouritesToggle.isOn = itemModel.favourite;
            editViewObject.SetActive(false);

            ImportModel(Application.persistentDataPath + itemModel.modelPath);
            ClearMaterialData();
            foreach (Renderer renderer in modelViewParent.GetComponentsInChildren<Renderer>())
            {
                if (!string.IsNullOrEmpty(itemModel.textureColourPath))
                    LoadTextureColour(Application.persistentDataPath + itemModel.textureColourPath, renderer);
                if (!string.IsNullOrEmpty(itemModel.textureNormalPath))
                    LoadTextureNormal(Application.persistentDataPath + itemModel.textureNormalPath, renderer);
                if (!string.IsNullOrEmpty(itemModel.textureDisplacementPath))
                    LoadTextureDisplacement(Application.persistentDataPath + itemModel.textureDisplacementPath, renderer);
                if (!string.IsNullOrEmpty(itemModel.textureSpecularPath))
                    LoadTextureSpecular(Application.persistentDataPath + itemModel.textureSpecularPath, renderer);
                if (!string.IsNullOrEmpty(itemModel.textureOcclusionPath))
                    LoadTextureOcclusion(Application.persistentDataPath + itemModel.textureOcclusionPath, renderer);
            }
            modelViewParent.transform.localPosition = itemModel.savePos.GetVector3();
            modelViewParent.transform.localEulerAngles = itemModel.saveRot.GetVector3();
            modelViewParent.transform.localScale = itemModel.saveScale.GetVector3();
        }
    }

    private void Start()
    {
        catalogueManager = GameObject.FindWithTag("CatalogueManager").GetComponent<CatalogueManager>();
        tempDataPath = Application.persistentDataPath + "/Temp/" + Guid.NewGuid().ToString() + "/";
        renderTextureImage.enabled = false;
        StartCoroutine(DoAfterLoadAnim());
    }

    IEnumerator DoAfterLoadAnim()
    {
        yield return new WaitForSeconds(GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length + GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime);
        this.gameObject.transform.SetParent(GameObject.Find("Canvas").transform);

        renderTextureNew = new RenderTexture(renderTexture);
        renderTextureNew.width = (int)renderTextureParent.rect.width;
        renderTextureNew.height = (int)renderTextureParent.rect.height;
        renderTextureNew.Create();
        renderCamera.targetTexture = renderTextureNew;
        renderTextureImage.texture = renderTextureNew;
        renderTextureImage.enabled = true;
    }

    public async void SaveAssetAsync()
    {
        if (string.IsNullOrEmpty(assetFriendlyName.text))
        {
            MessageBox.Show("Error", "Asset Name Is Missing", () => { });
            return;
        }

        if (itemModel.itemID == 0)
            if (string.IsNullOrEmpty(tempModelPath))
            {
                MessageBox.Show("Error", "Must Import Model", () => { });
                return;
            }

        this.GetComponent<Button>().interactable = false;
        LoadingPanelUI loadingPanelUI = GetComponentInChildren<LoadingPanelUI>(true);
        loadingPanelUI.gameObject.SetActive(true);
        loadingPanelUI.ChangeText("Please Wait", "Assets Uploading");

        const string itemFileName = "CatalogueItem.asscat";
        const string thumnailPrefabName = "AssetThumnail_Model";

        if (itemModel.itemID == 0)
        {
            itemModel = new CatalogueItem_Model
            {
                friendlyName = assetFriendlyName.text,
                itemID = itemModel.itemID,
                modifiedDate = DateTime.Now.ToString(),
                tags = tagsInputField.text.Split('#'),
                favourite = favouritesToggle.isOn,
                itemTypeCategory = categoryDropDown.value,
                savePos = new CustomVector3(modelViewParent.transform.localPosition),
                saveRot = new CustomVector3(modelViewParent.transform.localEulerAngles),
                saveScale = new CustomVector3(modelViewParent.transform.localScale),
            };
        }

        //Screenshot
        Texture2D tex = new Texture2D(renderTextureNew.height, renderTextureNew.height);
        RenderTexture.active = renderTextureNew;
        float offsetX = (renderTextureNew.width - renderTextureNew.height) / 2;
        tex.ReadPixels(new Rect(offsetX, 0, renderTextureNew.height, renderTextureNew.height), 0, 0);
        tex.Apply();
        itemModel.thumnailData = tex.EncodeToPNG();

        if (itemModel.itemID == 0)
        {
            //Copy All Files
            catalogueManager._CreatedAssetCount++;
            itemModel.itemID = catalogueManager._CreatedAssetCount;
            CatalogueItemDetail itemDetail = new CatalogueItemDetail
            {
                ItemType = CatalogueItemDetail.ItemTypes.Model,
                ItemID = catalogueManager._CreatedAssetCount,
                CatalogueItemDirectory = "/Assets/Models/" + catalogueManager._CreatedAssetCount.ToString("D5") + "/",
                DateModified = DateTime.Now.ToString(),
                FriendlyName = itemModel.friendlyName,
                ItemTypeCategory = categoryDropDown.value,
            };

            string localAssetPath = "/" + catalogueManager._DatabaseUID + itemDetail.CatalogueItemDirectory + "/";
            //Debug.Log(_LocalAssetPath);
            cmd_File.DeleteFolder(Application.persistentDataPath + localAssetPath, false);
            Directory.CreateDirectory(Application.persistentDataPath + localAssetPath);

            if (tempModelPath != null)
            {
                string path = localAssetPath + "/" + Path.GetFileName(tempModelPath);
                File.Copy(tempModelPath, Application.persistentDataPath + path, true);
                itemModel.modelPath = path;
            }

            if (tempTextureColourPath != null)
            {
                string path = localAssetPath + "/" + Path.GetFileName(tempTextureColourPath);
                File.Copy(tempTextureColourPath, Application.persistentDataPath + path, true);
                itemModel.textureColourPath = path;
            }

            if (tempTextureNormalPath != null)
            {
                string path = localAssetPath + "/" + Path.GetFileName(tempTextureNormalPath);
                File.Copy(tempTextureNormalPath, Application.persistentDataPath + path, true);
                itemModel.textureNormalPath = path;
            }

            if (tempTextureDisplacementPath != null)
            {
                string path = localAssetPath + "/" + Path.GetFileName(tempTextureDisplacementPath);
                File.Copy(tempTextureDisplacementPath, Application.persistentDataPath + path, true);
                itemModel.textureDisplacementPath = path;
            }

            if (tempTextureSpecularPath != null)
            {
                string path = localAssetPath + "/" + Path.GetFileName(tempTextureSpecularPath);
                File.Copy(tempTextureSpecularPath, Application.persistentDataPath + path, true);
                itemModel.textureSpecularPath = path;
            }

            if (tempTextureOcclusionPath != null)
            {
                string path = localAssetPath + "/" + Path.GetFileName(tempTextureOcclusionPath);
                File.Copy(tempTextureOcclusionPath, Application.persistentDataPath + path, true);
                itemModel.textureOcclusionPath = path;
            }

            cmd_File.SerializeObject(Application.persistentDataPath + localAssetPath, itemFileName, itemModel);
            catalogueManager._CatalogueItemDetails.Add(itemDetail);
            catalogueManager.ResyncCatalogueDatabaseAsync();

            using (DropboxClient dbx = new DropboxClient(AvoEx.AesEncryptor.DecryptString(PlayerPrefs.GetString("Token"))))
            {
                if (tempModelPath != null)
                    await cmd_Dropbox.UploadFileAsync(dbx, tempModelPath, itemDetail.CatalogueItemDirectory, Path.GetFileName(tempModelPath));

                if (tempTextureColourPath != null)
                    await cmd_Dropbox.UploadFileAsync(dbx, tempTextureColourPath, itemDetail.CatalogueItemDirectory, Path.GetFileName(tempTextureColourPath));

                if (tempTextureNormalPath != null)
                    await cmd_Dropbox.UploadFileAsync(dbx, tempTextureNormalPath, itemDetail.CatalogueItemDirectory, Path.GetFileName(tempTextureNormalPath));

                if (tempTextureDisplacementPath != null)
                    await cmd_Dropbox.UploadFileAsync(dbx, tempTextureDisplacementPath, itemDetail.CatalogueItemDirectory, Path.GetFileName(tempTextureDisplacementPath));

                if (tempTextureSpecularPath != null)
                    await cmd_Dropbox.UploadFileAsync(dbx, tempTextureSpecularPath, itemDetail.CatalogueItemDirectory, Path.GetFileName(tempTextureSpecularPath));

                if (tempTextureOcclusionPath != null)
                    await cmd_Dropbox.UploadFileAsync(dbx, tempTextureOcclusionPath, itemDetail.CatalogueItemDirectory, Path.GetFileName(tempTextureOcclusionPath));

                await cmd_Dropbox.UploadObjAsync(dbx, itemModel, itemDetail.CatalogueItemDirectory, itemFileName);
                Debug.Log("LOG:" + DateTime.Now.ToString() + " - " + itemModel.friendlyName + " Created");

                MessageBox.Show("Boom Shaka Laka", "Asset Now Added", () =>
                {
                    GetComponent<PopupItemController>().HideDialog(0);
                });
            }

            GameObject go = Instantiate(Resources.Load(thumnailPrefabName) as GameObject, GameObject.FindWithTag("ThumbnailGrid").transform);
            go.SendMessage("ObjectParse", itemModel);
        }
        else
        {
            foreach (CatalogueItemDetail itemDetail in catalogueManager._CatalogueItemDetails)
            {
                if (itemDetail.ItemID == itemModel.itemID)
                {
                    itemDetail.DateModified = DateTime.Now.ToString();
                    itemModel.modifiedDate = DateTime.Now.ToString();
                    itemDetail.FriendlyName = assetFriendlyName.text;
                    itemModel.friendlyName = assetFriendlyName.text;
                    itemModel.tags = tagsInputField.text.Split('#');
                    itemModel.favourite = favouritesToggle.isOn;
                    itemModel.itemTypeCategory = categoryDropDown.value;
                    itemModel.savePos = new CustomVector3(modelViewParent.transform.localPosition);
                    itemModel.saveRot = new CustomVector3(modelViewParent.transform.localEulerAngles);
                    itemModel.saveScale = new CustomVector3(modelViewParent.transform.localScale);
                    itemThumbnail.lable.text = assetFriendlyName.text;
                    itemThumbnail.ObjectParse(itemModel);
                    catalogueManager.ResyncCatalogueDatabaseAsync();
                    using (DropboxClient dropboxClient = new DropboxClient(AvoEx.AesEncryptor.DecryptString(PlayerPrefs.GetString("Token"))))
                    {
                        await cmd_Dropbox.UploadObjAsync(dropboxClient, itemModel, itemDetail.CatalogueItemDirectory, itemFileName);
                        Debug.Log("LOG:" + DateTime.Now.ToString() + " - " + itemModel.friendlyName + " Updated");
                        MessageBox.Show("Boom Shaka Laka", "Asset Now Updated", () =>
                        {
                            GetComponent<PopupItemController>().HideDialog(0);
                        });
                        string localPath = Application.persistentDataPath + "/" + catalogueManager._DatabaseUID + itemDetail.CatalogueItemDirectory + "/";
                        cmd_File.SerializeObject(localPath, itemFileName, itemModel);
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
        if (itemModel.itemID != 0)
        {
            this.GetComponent<Button>().interactable = false;
            LoadingPanelUI loadingPanelUI = GetComponentInChildren<LoadingPanelUI>(true);
            loadingPanelUI.gameObject.SetActive(true);
            loadingPanelUI.ChangeText("Please Wait", "Deleting Asset");

            CatalogueItemDetail thisDetail = new CatalogueItemDetail();
            foreach (CatalogueItemDetail detail in catalogueManager._CatalogueItemDetails)
                if (detail.ItemID == this.itemModel.itemID)
                    thisDetail = detail;
            catalogueManager._CatalogueItemDetails.Remove(thisDetail);
            catalogueManager.ResyncCatalogueDatabaseAsync();

            using (DropboxClient dbx = new DropboxClient(AvoEx.AesEncryptor.DecryptString(PlayerPrefs.GetString("Token"))))
            {
                await cmd_Dropbox.DeleteAssetFolderAsync(dbx, "/Assets/Models/" + thisDetail.ItemID.ToString("D5"));
                Debug.Log("LOG:" + DateTime.Now.ToString() + " - " + itemModel.friendlyName + " Deleted");
            }

            foreach (CatalogueItemThumnail_Model goTile in GameObject.FindWithTag("ThumbnailGrid").GetComponentsInChildren<CatalogueItemThumnail_Model>())
            {
                if (goTile.assetID == this.itemModel.itemID)
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
            string localAssetPath = "/" + catalogueManager._DatabaseUID + "/Assets/Models/" + this.itemModel.itemID.ToString("D5") + "/";
            cmd_File.DeleteFolder(Application.persistentDataPath + localAssetPath, true);
        }
        else
        {
            GetComponent<PopupItemController>().HideDialog(0);
        }
    }

    public void ExportAsset()
    {
        if (itemModel.itemID == 0)
        {
            MessageBox.Show("Error", "Save Asset Before Exporting", () => { });
            return;
        }
        string path = FileBrowser.SaveFile("Select Save Location", "", itemModel.friendlyName, "zip");
        if (string.IsNullOrEmpty(path))
            return;
        Directory.CreateDirectory(tempDataPath);
        string localAssetPath = "/" + catalogueManager._DatabaseUID + "/Assets/Models/" + this.itemModel.itemID.ToString("D5") + "/";
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


    public void LoadModelFromFile()
    {
        foreach (Transform trans in modelViewParent.transform)
            Destroy(trans.gameObject);
        thumnailColour.texture = null;
        thumnailNormal.texture = null;
        thumnailDisplacement.texture = null;
        thumnailSpecular.texture = null;
        thumnailOcclusion.texture = null;
        tempTextureColourPath = null;
        tempTextureNormalPath = null;
        tempTextureDisplacementPath = null;
        tempTextureSpecularPath = null;
        tempTextureOcclusionPath = null;
        tempModelPath = null;
        MessageBox.Show("Warning", "Ensure Filename Contains No Special Character", () =>
        {
            string path = FileBrowser.OpenSingleFile("Select Model", "", extensionsModels);
            if (!string.IsNullOrEmpty(path))
            {
                Directory.CreateDirectory(tempDataPath);
                ImportModel(path);
                tempModelPath = path;
                File.Copy(path, tempDataPath + Path.GetFileName(path), true);
                var attr = File.GetAttributes(tempDataPath + Path.GetFileName(path));
                attr = attr & ~FileAttributes.ReadOnly;
                File.SetAttributes(tempDataPath + Path.GetFileName(path), attr);
            }
                
            ClearMaterialData();
        });
    }

    private void ImportModel(string path)
    {
        using (var assetLoader = new AssetLoader())
        {
            var assetLoaderOptions = AssetLoaderOptions.CreateInstance();
            assetLoaderOptions.DontLoadLights = true;
            assetLoaderOptions.DontLoadCameras = true;
            assetLoaderOptions.DontLoadMaterials = false;
            assetLoader.LoadFromFile(path, assetLoaderOptions, modelViewParent); //Loads the model synchronously and stores the reference in myGameObject.
        }
    }

    private void ClearMaterialData()
    {
        foreach (Renderer renderer in modelViewParent.GetComponentsInChildren<Renderer>())
        {
            renderer.material.CopyPropertiesFromMaterial(placeHolderMaterial);
            renderer.material.shader = Shader.Find(placeHolderMaterial.shader.name);
            LoadTextureColour(null, renderer);
            LoadTextureNormal(null, renderer);
            LoadTextureDisplacement(null, renderer);
            LoadTextureSpecular(null, renderer);
            LoadTextureOcclusion(null, renderer);
        }
    }



    private void LoadTextureColour(string path, Renderer renderer)
    {
        WWW www = new WWW("file:///" + path);
        bool fileExists = !string.IsNullOrEmpty(path);
        thumnailColour.texture = fileExists ? www.texture : null;
        renderer.material.SetTexture("_MainTex", fileExists ? www.texture : null);
    }

    private void LoadTextureNormal(string path, Renderer renderer)
    {
        WWW www = new WWW("file:///" + path);
        bool fileExists = !string.IsNullOrEmpty(path);
        thumnailNormal.texture = fileExists ? www.texture : null;
        renderer.material.SetTexture("_BumpMap", fileExists ? www.texture : null);
    }

    private void LoadTextureDisplacement(string path, Renderer renderer)
    {
        WWW www = new WWW("file:///" + path);
        bool fileExists = !string.IsNullOrEmpty(path);
        thumnailDisplacement.texture = fileExists ? www.texture : null;
        renderer.material.SetFloat("_Parallax", fileExists ? 0.02f : 0.005f);
        renderer.material.SetTexture("_ParallaxMap", fileExists ? www.texture : null);
    }

    private void LoadTextureSpecular(string path, Renderer renderer)
    {
        WWW www = new WWW("file:///" + path);
        bool fileExists = !string.IsNullOrEmpty(path);
        thumnailSpecular.texture = fileExists ? www.texture : null;
        renderer.material.SetTexture("_SpecGlossMap", fileExists ? www.texture : null);
    }

    private void LoadTextureOcclusion(string path, Renderer renderer)
    {
        WWW www = new WWW("file:///" + path);
        bool fileExists = !string.IsNullOrEmpty(path);
        thumnailOcclusion.texture = fileExists ? www.texture : null;
        renderer.material.SetTexture("_OcclusionMap", fileExists ? www.texture : null);
    }

    public void LoadTextureFromFile(string type)
    {
        string path = FileBrowser.OpenSingleFile("Select Texture", "", extensionsTextures);
        Directory.CreateDirectory(tempDataPath);
        switch (type)
        {
            case "Colour":
                tempTextureColourPath = null;
                foreach (Renderer renderer in modelViewParent.GetComponentsInChildren<Renderer>())
                        LoadTextureColour(path, renderer);
                if (string.IsNullOrEmpty(path))
                    return;
                tempTextureColourPath = path;
                File.Copy(path, tempDataPath + Path.GetFileName(path), true);
                break;
            case "Normal":
                tempTextureNormalPath = null;
                foreach (Renderer renderer in modelViewParent.GetComponentsInChildren<Renderer>())
                    LoadTextureNormal(path, renderer);
                if (string.IsNullOrEmpty(path))
                    return;
                tempTextureNormalPath = path;
                File.Copy(path, tempDataPath + Path.GetFileName(path), true);
                break;
            case "Displacement":
                tempTextureDisplacementPath = null;
                foreach (Renderer renderer in modelViewParent.GetComponentsInChildren<Renderer>())
                    LoadTextureDisplacement(path, renderer);
                if (string.IsNullOrEmpty(path))
                    return;
                tempTextureDisplacementPath = path;
                File.Copy(path, tempDataPath + Path.GetFileName(path), true);
                break;
            case "Specular":
                tempTextureSpecularPath = null;
                foreach (Renderer renderer in modelViewParent.GetComponentsInChildren<Renderer>())
                    LoadTextureSpecular(path, renderer);
                if (string.IsNullOrEmpty(path))
                    return;
                tempTextureSpecularPath = path;
                File.Copy(path, tempDataPath + Path.GetFileName(path), true);
                break;
            case "Occlusion":
                tempTextureOcclusionPath = null;
                foreach (Renderer renderer in modelViewParent.GetComponentsInChildren<Renderer>())
                    LoadTextureOcclusion(path, renderer);
                if (string.IsNullOrEmpty(path))
                    return;
                tempTextureOcclusionPath = path;
                File.Copy(path, tempDataPath + Path.GetFileName(path), true);
                break;
        }
        if (!string.IsNullOrEmpty(path))
        {
            var attr = File.GetAttributes(tempDataPath + Path.GetFileName(path));
            attr = attr & ~FileAttributes.ReadOnly;
            File.SetAttributes(tempDataPath + Path.GetFileName(path), attr);
        }
    }

    public void AnimateToggle()
    {
        modelViewParent.GetComponent<Animation>().enabled = !modelViewParent.GetComponent<Animation>().enabled;
    }
}