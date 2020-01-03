using Crosstales.FB;
using Dropbox.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class CatalogueItemManager_Tile : MonoBehaviour {

    //bool changesMade = false;

    private ExtensionFilter[] extensions = new[] { new ExtensionFilter("All Supported Formats", "png", "jpg", "jpeg", "bmp") };

    private CatalogueItem_Tile itemTile = new CatalogueItem_Tile();
    private CatalogueItemThumnail_Tile itemThumbnail;
    private CatalogueManager catalogueManager;

    private string tempDataPath;
    private string
        tempTextureColourPath,
        tempTextureNormalPath,
        tempTextureDisplacementPath,
        tempTextureSpecularPath,
        tempTextureOcclusionPath;

    public InputField assetFriendlyName;

    public List<Renderer> previewModelRenders;

    public RawImage thumnailColour;
    public RawImage thumnailNormal;
    public RawImage thumnailDisplacement;
    public RawImage thumnailSpecular;
    public RawImage thumnailOcclusion;

    public InputField tagsInputField;
    public Dropdown categoryDropDown;
    public Toggle favouritesToggle;
    public GameObject texLoadViewGameObject;
    public RenderTexture renderTexture;
    public RectTransform renderTextureParent;
    public Camera renderCamera;
    public RawImage renderTextureImage;

    public List<Button> disableButtons = new List<Button>();

    public Slider tileSlider;

    private RenderTexture renderTextureNew;

    public void ObjectParse(CatalogueItem_Tile tileParse, CatalogueItemThumnail_Tile thumbnail)
    {
        this.itemTile = tileParse;
        if (itemTile._ItemID != 0)
        {
            //Existing Asset
            itemThumbnail = thumbnail;
            assetFriendlyName.text = itemTile._FriendlyName;
            tagsInputField.text = string.Join("#", itemTile.tags);
            categoryDropDown.value = itemTile.itemTypeCategory;
            favouritesToggle.isOn = itemTile.favourite;
            foreach (Button button in disableButtons)
                button.interactable = false;
            if (!string.IsNullOrEmpty(itemTile._TextureColourPath))
                LoadTextureColour(Application.persistentDataPath + itemTile._TextureColourPath);
            if (!string.IsNullOrEmpty(itemTile._TextureNormalPath))
                LoadTextureNormal(Application.persistentDataPath + itemTile._TextureNormalPath);
            if (!string.IsNullOrEmpty(itemTile._TextureDisplacementPath))
                LoadTextureDisplacement(Application.persistentDataPath + itemTile._TextureDisplacementPath);
            if (!string.IsNullOrEmpty(itemTile._TextureSpecularPath))
                LoadTextureSpecular(Application.persistentDataPath + itemTile._TextureSpecularPath);
            if (!string.IsNullOrEmpty(itemTile._TextureOcclusionPath))
                LoadTextureOcclusion(Application.persistentDataPath + itemTile._TextureOcclusionPath);
            texLoadViewGameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        ClearMatData();
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

    private void ClearMatData()
    {
        foreach (Renderer render in previewModelRenders)
        {
            render.material.SetTexture("_MainTex", null);
            render.material.SetTexture("_BumpMap", null);
            render.material.SetTexture("_SpecGlossMap", null);
            render.material.SetFloat("_Parallax", 0.005f);
            render.material.mainTextureScale = new Vector2(tileSlider.value, tileSlider.value);
            render.material.SetFloat("_BumpScale", 1);
        }
    }

    public void SelectTextureFromFile(string textureType)
    {
        string path = FileBrowser.OpenSingleFile("Select Texture", "", extensions);
        Directory.CreateDirectory(tempDataPath);
        switch (textureType)
        {
            case "Colour":
                tempTextureColourPath = null;
                LoadTextureColour(path);
                if (string.IsNullOrEmpty(path))
                    return;
                tempTextureColourPath = path;
                File.Copy(path, tempDataPath + Path.GetFileName(path), true);
                break;
            case "Normal":
                tempTextureNormalPath = null;
                LoadTextureNormal(path);
                if (string.IsNullOrEmpty(path))
                    return;
                tempTextureNormalPath = path;
                File.Copy(path, tempDataPath + Path.GetFileName(path), true);
                break;
            case "Displacement":
                tempTextureDisplacementPath = null;
                LoadTextureDisplacement(path);
                if (string.IsNullOrEmpty(path))
                    return;
                tempTextureDisplacementPath = path;
                File.Copy(path, tempDataPath + Path.GetFileName(path), true);
                break;
            case "Specular":
                tempTextureSpecularPath = null;
                LoadTextureSpecular(path);
                if (string.IsNullOrEmpty(path))
                    return;
                tempTextureSpecularPath = path;
                File.Copy(path, tempDataPath + Path.GetFileName(path), true);
                break;
            case "Occlusion":
                tempTextureOcclusionPath = null;
                LoadTextureOcclusion(path);
                if (string.IsNullOrEmpty(path))
                    return;
                tempTextureOcclusionPath = path;
                File.Copy(path, tempDataPath + Path.GetFileName(path), true);
                break;
        }
        //changesMade = true;
        if (!string.IsNullOrEmpty(path))
        {
            var attr = File.GetAttributes(tempDataPath + Path.GetFileName(path));
            attr = attr & ~FileAttributes.ReadOnly;
            File.SetAttributes(tempDataPath + Path.GetFileName(path), attr);
        }

    }

    public async void SaveAssetAsync()
    {
        if (string.IsNullOrEmpty(assetFriendlyName.text))
        {
            MessageBox.Show("Error", "Asset Name Is Missing", () => { });
            return;
        }

        this.GetComponent<Button>().interactable = false;
        LoadingPanelUI loadingPanelUI = GetComponentInChildren<LoadingPanelUI>(true);
        loadingPanelUI.gameObject.SetActive(true);
        loadingPanelUI.ChangeText("Please Wait", "Assets Uploading");

        const string _ItemFileName = "CatalogueItem.asscat";
        const string _ThumnailPrefabName = "AssetThumnail_Tile";

        if (itemTile._ItemID == 0)
        {
            itemTile = new CatalogueItem_Tile
            {
                _FriendlyName = assetFriendlyName.text,
                _ItemID = itemTile._ItemID,
                _ModifiedDate = DateTime.Now.ToString(),
                tags = tagsInputField.text.Split('#'),
                favourite = favouritesToggle.isOn,
                itemTypeCategory = categoryDropDown.value,
                //Does not seem to be neccasary any more
                //_TextureColourPath = Application.persistentDataPath + itemTile._TextureColourPath,
                //_TextureNormalPath = Application.persistentDataPath + itemTile._TextureNormalPath,
                //_TextureDisplacementPath = Application.persistentDataPath + itemTile._TextureDisplacementPath,
                //_TextureSpecularPath = Application.persistentDataPath + itemTile._TextureSpecularPath,
                //_TextureOcclusionPath = Application.persistentDataPath + itemTile._TextureOcclusionPath,
            };
        }
        

        if (itemTile._ItemID == 0) //New Asset
        {
            //Copy All Files

            catalogueManager._CreatedAssetCount++;
            itemTile._ItemID = catalogueManager._CreatedAssetCount;
            CatalogueItemDetail itemDetail = new CatalogueItemDetail
            {
                ItemType = CatalogueItemDetail.ItemTypes.Tile,
                ItemID = catalogueManager._CreatedAssetCount,
                CatalogueItemDirectory = "/Assets/Tiles/" + catalogueManager._CreatedAssetCount.ToString("D5") + "/",
                DateModified = DateTime.Now.ToString(),
                FriendlyName = itemTile._FriendlyName,
                ItemTypeCategory = categoryDropDown.value,
            };

            string _LocalAssetPath = "/" + catalogueManager._DatabaseUID + itemDetail.CatalogueItemDirectory + "/";
            //Debug.Log(_LocalAssetPath);
            cmd_File.DeleteFolder(Application.persistentDataPath + _LocalAssetPath, false);

            Directory.CreateDirectory(Application.persistentDataPath + _LocalAssetPath);
            
            if (tempTextureColourPath != null)
            {
                string path = _LocalAssetPath + "/" + Path.GetFileName(tempTextureColourPath);
                File.Copy(tempTextureColourPath, Application.persistentDataPath + path, true);
                itemTile._TextureColourPath = path;
            }
            if (tempTextureNormalPath != null)
            {
                string path = _LocalAssetPath + "/" + Path.GetFileName(tempTextureNormalPath);
                File.Copy(tempTextureNormalPath, Application.persistentDataPath + path, true);
                itemTile._TextureNormalPath = path;
            }
            if (tempTextureDisplacementPath != null)
            {
                string path = _LocalAssetPath + "/" + Path.GetFileName(tempTextureDisplacementPath);
                File.Copy(tempTextureDisplacementPath, Application.persistentDataPath + path, true);
                itemTile._TextureDisplacementPath = path;
            }
            if (tempTextureSpecularPath != null)
            {
                string path = _LocalAssetPath + "/" + Path.GetFileName(tempTextureSpecularPath);
                File.Copy(tempTextureSpecularPath, Application.persistentDataPath + path, true);
                itemTile._TextureSpecularPath = path;
            }
            if (tempTextureOcclusionPath != null)
            {
                string path = _LocalAssetPath + "/" + Path.GetFileName(tempTextureOcclusionPath);
                File.Copy(tempTextureOcclusionPath, Application.persistentDataPath + path, true);
                itemTile._TextureOcclusionPath = path;
            }

            cmd_File.SerializeObject(Application.persistentDataPath + _LocalAssetPath, _ItemFileName, itemTile);
            catalogueManager._CatalogueItemDetails.Add(itemDetail);
            catalogueManager.ResyncCatalogueDatabaseAsync();

            using (DropboxClient _DropboxClient = new DropboxClient(AvoEx.AesEncryptor.DecryptString(PlayerPrefs.GetString("Token"))))
            {
                if (tempTextureColourPath != null)
                    await cmd_Dropbox.UploadFileAsync(_DropboxClient, tempTextureColourPath, itemDetail.CatalogueItemDirectory, Path.GetFileName(tempTextureColourPath));

                if (tempTextureNormalPath != null)
                    await cmd_Dropbox.UploadFileAsync(_DropboxClient, tempTextureNormalPath, itemDetail.CatalogueItemDirectory, Path.GetFileName(tempTextureNormalPath));

                if (tempTextureDisplacementPath != null)
                    await cmd_Dropbox.UploadFileAsync(_DropboxClient, tempTextureDisplacementPath, itemDetail.CatalogueItemDirectory, Path.GetFileName(tempTextureDisplacementPath));

                if (tempTextureSpecularPath != null)
                    await cmd_Dropbox.UploadFileAsync(_DropboxClient, tempTextureSpecularPath, itemDetail.CatalogueItemDirectory, Path.GetFileName(tempTextureSpecularPath));

                if (tempTextureOcclusionPath != null)
                    await cmd_Dropbox.UploadFileAsync(_DropboxClient, tempTextureOcclusionPath, itemDetail.CatalogueItemDirectory, Path.GetFileName(tempTextureOcclusionPath));

                await cmd_Dropbox.UploadObjAsync(_DropboxClient, itemTile, itemDetail.CatalogueItemDirectory, _ItemFileName);
                Debug.Log("LOG:" + DateTime.Now.ToString() + " - " + itemTile._FriendlyName + " Created");
                
                MessageBox.Show("Boom Shaka Laka", "Asset Now Added", () =>
                {
                    GetComponent<PopupItemController>().HideDialog(0);
                });
            }

            GameObject go = Instantiate(Resources.Load(_ThumnailPrefabName) as GameObject, GameObject.FindWithTag("ThumbnailGrid").transform);
            go.SendMessage("ObjectParse", itemTile);

        }
        else
        {
            foreach (CatalogueItemDetail itemDetail in catalogueManager._CatalogueItemDetails)
            {
                if (itemDetail.ItemID == itemTile._ItemID)
                {
                    itemDetail.DateModified = DateTime.Now.ToString();
                    itemTile._ModifiedDate = DateTime.Now.ToString();
                    itemDetail.FriendlyName = assetFriendlyName.text;
                    itemTile._FriendlyName = assetFriendlyName.text;
                    itemTile.tags = tagsInputField.text.Split('#');
                    itemTile.favourite = favouritesToggle.isOn;
                    itemTile.itemTypeCategory = categoryDropDown.value;
                    itemThumbnail.lable.text = assetFriendlyName.text;
                    itemThumbnail.ObjectParse(itemTile);
                    catalogueManager.ResyncCatalogueDatabaseAsync();
                    using (DropboxClient dropboxClient = new DropboxClient(AvoEx.AesEncryptor.DecryptString(PlayerPrefs.GetString("Token"))))
                    {
                        await cmd_Dropbox.UploadObjAsync(dropboxClient, itemTile, itemDetail.CatalogueItemDirectory, _ItemFileName);
                        Debug.Log("LOG:" + DateTime.Now.ToString() + " - " + itemTile._FriendlyName + " Updated");
                        MessageBox.Show("Boom Shaka Laka", "Asset Now Updated", () =>
                        {
                            GetComponent<PopupItemController>().HideDialog(0);
                        });
                        string localPath = Application.persistentDataPath + "/" + catalogueManager._DatabaseUID + itemDetail.CatalogueItemDirectory + "/";
                        cmd_File.SerializeObject(localPath, _ItemFileName, itemTile);
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
        if (itemTile._ItemID != 0)
        {
            this.GetComponent<Button>().interactable = false;
            LoadingPanelUI loadingPanelUI = GetComponentInChildren<LoadingPanelUI>(true);
            loadingPanelUI.gameObject.SetActive(true);
            loadingPanelUI.ChangeText("Please Wait", "Deleting Asset");

            CatalogueItemDetail thisDetail = new CatalogueItemDetail();
            foreach (CatalogueItemDetail detail in catalogueManager._CatalogueItemDetails)
                if (detail.ItemID == this.itemTile._ItemID)
                    thisDetail = detail;
            catalogueManager._CatalogueItemDetails.Remove(thisDetail);
            catalogueManager.ResyncCatalogueDatabaseAsync();

            using (DropboxClient dbx = new DropboxClient(AvoEx.AesEncryptor.DecryptString(PlayerPrefs.GetString("Token"))))
            {
                await cmd_Dropbox.DeleteAssetFolderAsync(dbx, "/Assets/Tiles/" + thisDetail.ItemID.ToString("D5"));
                Debug.Log("LOG:" + DateTime.Now.ToString() + " - " + itemTile._FriendlyName + " Deleted");
            }
            foreach (CatalogueItemThumnail_Tile goTile in GameObject.FindWithTag("ThumbnailGrid").GetComponentsInChildren<CatalogueItemThumnail_Tile>())
            {
                if (goTile.assetID == this.itemTile._ItemID)
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
            string localAssetPath = "/" + catalogueManager._DatabaseUID + "/Assets/Tiles/" + this.itemTile._ItemID.ToString("D5") + "/";
            cmd_File.DeleteFolder(Application.persistentDataPath + localAssetPath, true);
        }
        else
        {
            GetComponent<PopupItemController>().HideDialog(0);
        }
    }

    public void ExportAsset()
    {
        if (itemTile._ItemID == 0)
        {
            MessageBox.Show("Error", "Save Asset Before Exporting", () => { });
            return;
        }
        string path = FileBrowser.SaveFile("Select Save Location", "", itemTile._FriendlyName, "zip");
        if (string.IsNullOrEmpty(path))
            return;
        Directory.CreateDirectory(tempDataPath);
        string localAssetPath = "/" + catalogueManager._DatabaseUID + "/Assets/Tiles/" + this.itemTile._ItemID.ToString("D5") + "/";
        string[] files = Directory.GetFiles(Application.persistentDataPath + localAssetPath);
        foreach (string file in files)
        {
            string name = Path.GetFileName(file);
            if (name != "CatalogueItem.asscat")
            {
                string dest = Path.Combine(tempDataPath, name);
                File.Copy(file, dest, true);
            }
        }
        System.IO.Compression.ZipFile.CreateFromDirectory(tempDataPath, path, System.IO.Compression.CompressionLevel.Fastest, false);
    }

    private void LoadTextureColour(string path)
    {
        WWW www = new WWW("file:///" + path);
        bool fileExists = !string.IsNullOrEmpty(path);
        thumnailColour.texture = fileExists ? www.texture : null;
        SetTextureMap("_MainTex", fileExists ? www.texture : null);
    }

    private void LoadTextureNormal(string path)
    {
        WWW www = new WWW("file:///" + path);
        bool fileExists = !string.IsNullOrEmpty(path);
        thumnailNormal.texture = fileExists ? www.texture : null;
        SetTextureMap("_BumpMap", fileExists ? www.texture : null);
    }

    private void LoadTextureDisplacement(string path)
    {
        WWW www = new WWW("file:///" + path);
        bool fileExists = !string.IsNullOrEmpty(path);
        thumnailDisplacement.texture = fileExists ? www.texture : null;
        SetTexureFloat("_Parallax", fileExists ? 0.02f : 0.005f);
        SetTextureMap("_ParallaxMap", fileExists ? www.texture : null);
    }

    private void LoadTextureSpecular(string path)
    {
        WWW www = new WWW("file:///" + path);
        bool fileExists = !string.IsNullOrEmpty(path);
        thumnailSpecular.texture = fileExists ? www.texture : null;
        SetTextureMap("_SpecGlossMap", fileExists ? www.texture : null);
    }

    private void LoadTextureOcclusion(string path)
    {
        WWW www = new WWW("file:///" + path);
        bool fileExists = !string.IsNullOrEmpty(path);
        thumnailOcclusion.texture = fileExists ? www.texture : null;
        SetTextureMap("_OcclusionMap", fileExists ? www.texture : null);
    }

    private void SetTextureMap(string keyword, Texture tex)
    {
        foreach (Renderer render in previewModelRenders)
            render.material.SetTexture(keyword, tex);
    }

    private void SetTextureKeyword(string keyword, bool enable = true)
    {
        if (enable)
            foreach (Renderer render in previewModelRenders)
                render.material.EnableKeyword(keyword);
        else
            foreach (Renderer render in previewModelRenders)
                render.material.DisableKeyword(keyword);
    }

    private void SetTexureFloat(string keyword, float value)
    {
        foreach (Renderer render in previewModelRenders)
            render.material.SetFloat(keyword, value);
    }

    public void UpdateTileAmount(Text updateText)
    {
        foreach (Renderer render in previewModelRenders)
            render.material.mainTextureScale = new Vector2(tileSlider.value, tileSlider.value);
        updateText.text = tileSlider.value.ToString();
    }

    public void SelectModelPreview(int typeIndex)
    {
        foreach (Renderer renderer in previewModelRenders)
            renderer.enabled = false;
        previewModelRenders[typeIndex].enabled = true;
    }
}
