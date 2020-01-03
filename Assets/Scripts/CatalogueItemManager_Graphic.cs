using Crosstales.FB;
using Dropbox.Api;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CatalogueItemManager_Graphic : MonoBehaviour {

    private ExtensionFilter[] extensions = new[] { new ExtensionFilter("All Supported Formats", "png", "jpg", "jpeg", "bmp") };

    private CatalogueItem_Graphic itemGraphic = new CatalogueItem_Graphic();
    private CatalogueItemThumnail_Graphic itemThumbnail;
    private CatalogueManager catalogueManager;

    private string tempDataPath;
    private string tempGraphicPath;
    private int sliceAnimPos = 0;
    private float sliceAnimSpeed = 0.1F;
    private float sliceAnimSpeedPos = 0;

    public List<Sprite> sliceSprites = new List<Sprite>();

    public Button buttonImport;
    public Toggle useSliceToggle;
    public InputField assetFriendlyName;
    public InputField tagsInputField;
    public Toggle favouritesToggle;
    public Image graphicImorted;
    public Image graphicImortedSliced;
    public Slider xSlider;
    public Slider ySlider;
    public Dropdown categoryDropDown;

    public GameObject panelSlice;

    private void Awake()
    {
        catalogueManager = GameObject.FindWithTag("CatalogueManager").GetComponent<CatalogueManager>();
        tempDataPath = Application.persistentDataPath + "/Temp/" + Guid.NewGuid().ToString() + "/";
        panelSlice.SetActive(false);
        StartCoroutine(DoAfterLoadAnim());
    }

    IEnumerator DoAfterLoadAnim()
    {
        yield return new WaitForSeconds(GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length + GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime);
        this.gameObject.transform.SetParent(GameObject.Find("Canvas").transform);
        if (itemGraphic.itemID != 0)
        {
            xSlider.value = itemGraphic.slicesX;
            ySlider.value = itemGraphic.slicesY;
            useSliceToggle.isOn = itemGraphic.isSliced;
        }
    }

    private void Update()
    {
        if (useSliceToggle.isOn)
        {
            sliceAnimSpeedPos += Time.deltaTime;
            if (sliceAnimSpeedPos >= sliceAnimSpeed)
            {
                sliceAnimSpeedPos = 0;
                if (sliceAnimPos >= sliceSprites.Count)
                    sliceAnimPos = 0;
                graphicImortedSliced.sprite = sliceSprites[sliceAnimPos];
                sliceAnimPos++;
            }
        }
    }

    public void ObjectParse(CatalogueItem_Graphic graphicParse, CatalogueItemThumnail_Graphic thumbnail)
    {
        this.itemGraphic = graphicParse;
        if (itemGraphic.itemID != 0)
        {
            //Existing Asset
            itemThumbnail = thumbnail;
            assetFriendlyName.text = itemGraphic.friendlyName;
            tagsInputField.text = string.Join("#", itemGraphic.tags);
            favouritesToggle.isOn = itemGraphic.favourite;
            
            
            categoryDropDown.value = itemGraphic.itemTypeCategory;
            StartCoroutine(SetGraphicTexture(Application.persistentDataPath + itemGraphic.graphicPath));
            buttonImport.interactable = false;
            buttonImport.GetComponentInChildren<Text>().text = "";

        }
    }

    public async void SaveAssetAsync()
    {
        if (string.IsNullOrEmpty(assetFriendlyName.text))
        {
            MessageBox.Show("Error", "Asset Name Is Missing", () => { });
            return;
        }

        if (itemGraphic.itemID == 0)
            if (string.IsNullOrEmpty(tempGraphicPath))
            {
                MessageBox.Show("Error", "Image Required", () => { });
                return;
            }

        this.GetComponent<Button>().interactable = false;
        LoadingPanelUI loadingPanelUI = GetComponentInChildren<LoadingPanelUI>(true);
        loadingPanelUI.gameObject.SetActive(true);
        loadingPanelUI.ChangeText("Please Wait", "Assets Uploading");

        const string itemFileName = "CatalogueItem.asscat";
        const string thumnailPrefabName = "AssetThumnail_Graphic";

        if (itemGraphic.itemID == 0)
        {
            catalogueManager._CreatedAssetCount++;
            CatalogueItemDetail itemDetail = new CatalogueItemDetail
            {
                ItemType = CatalogueItemDetail.ItemTypes.Graphic,
                ItemID = catalogueManager._CreatedAssetCount,
                CatalogueItemDirectory = "/Assets/Graphics/" + catalogueManager._CreatedAssetCount.ToString("D5") + "/",
                DateModified = DateTime.Now.ToString(),
                FriendlyName = assetFriendlyName.text,
                ItemTypeCategory = categoryDropDown.value,
            };

            string localAssetPath = "/" + catalogueManager._DatabaseUID + itemDetail.CatalogueItemDirectory + "/";
            cmd_File.DeleteFolder(Application.persistentDataPath + localAssetPath, false);
            Directory.CreateDirectory(Application.persistentDataPath + localAssetPath);

            string localGraphicPath = localAssetPath + "/" + Path.GetFileName(tempGraphicPath);
            File.Copy(tempGraphicPath, Application.persistentDataPath + localGraphicPath, true);

            itemGraphic = new CatalogueItem_Graphic
            {
                friendlyName = assetFriendlyName.text,
                itemID = catalogueManager._CreatedAssetCount,
                modifiedDate = DateTime.Now.ToString(),
                tags = tagsInputField.text.Split('#'),
                favourite = favouritesToggle.isOn,
                itemTypeCategory = categoryDropDown.value,
                isSliced = useSliceToggle.isOn,
                slicesX = xSlider.value,
                slicesY = ySlider.value,
                graphicPath = localGraphicPath,
            };

            cmd_File.SerializeObject(Application.persistentDataPath + localAssetPath, itemFileName, itemGraphic);
            catalogueManager._CatalogueItemDetails.Add(itemDetail);
            catalogueManager.ResyncCatalogueDatabaseAsync();

            using (DropboxClient dbx = new DropboxClient(AvoEx.AesEncryptor.DecryptString(PlayerPrefs.GetString("Token"))))
            {
                await cmd_Dropbox.UploadFileAsync(dbx, tempGraphicPath, itemDetail.CatalogueItemDirectory, Path.GetFileName(tempGraphicPath));
                await cmd_Dropbox.UploadObjAsync(dbx, itemGraphic, itemDetail.CatalogueItemDirectory, itemFileName);
                Debug.Log("LOG:" + DateTime.Now.ToString() + " - " + itemGraphic.friendlyName + " Created");
                MessageBox.Show("Boom Shaka Laka", "Asset Now Added", () =>
                {
                    GetComponent<PopupItemController>().HideDialog(0);
                });
            }
            GameObject go = Instantiate(Resources.Load(thumnailPrefabName) as GameObject, GameObject.FindWithTag("ThumbnailGrid").transform);
            go.SendMessage("ObjectParse", itemGraphic);
        }
        else
        {
            foreach (CatalogueItemDetail itemDetail in catalogueManager._CatalogueItemDetails)
            {
                if (itemDetail.ItemID == itemGraphic.itemID)
                {
                    itemDetail.DateModified = DateTime.Now.ToString();
                    itemGraphic.modifiedDate = DateTime.Now.ToString();
                    itemDetail.FriendlyName = assetFriendlyName.text;
                    itemGraphic.friendlyName = assetFriendlyName.text;
                    itemGraphic.tags = tagsInputField.text.Split('#');
                    itemGraphic.favourite = favouritesToggle.isOn;
                    itemGraphic.isSliced = useSliceToggle.isOn;
                    itemGraphic.itemTypeCategory = categoryDropDown.value;
                    itemGraphic.slicesX = xSlider.value;
                    itemGraphic.slicesY = ySlider.value;
                    itemThumbnail.lable.text = assetFriendlyName.text;
                    itemThumbnail.ObjectParse(itemGraphic);
                    catalogueManager.ResyncCatalogueDatabaseAsync();
                    using (DropboxClient dropboxClient = new DropboxClient(AvoEx.AesEncryptor.DecryptString(PlayerPrefs.GetString("Token"))))
                    {
                        await cmd_Dropbox.UploadObjAsync(dropboxClient, itemGraphic, itemDetail.CatalogueItemDirectory, itemFileName);
                        Debug.Log("LOG:" + DateTime.Now.ToString() + " - " + itemGraphic.friendlyName + " Updated");
                        MessageBox.Show("Boom Shaka Laka", "Asset Now Updated", () =>
                        {
                            GetComponent<PopupItemController>().HideDialog(0);
                        });
                        string localPath = Application.persistentDataPath + "/" + catalogueManager._DatabaseUID + itemDetail.CatalogueItemDirectory + "/";
                        cmd_File.SerializeObject(localPath, itemFileName, itemGraphic);
                        return;
                    }
                }
            }
        }
        loadingPanelUI.gameObject.SetActive(false);
        this.GetComponent<Button>().interactable = true;
    }

    public async void DeleteAsset()
    {
        if (itemGraphic.itemID != 0)
        {
            this.GetComponent<Button>().interactable = false;
            LoadingPanelUI loadingPanelUI = GetComponentInChildren<LoadingPanelUI>(true);
            loadingPanelUI.gameObject.SetActive(true);
            loadingPanelUI.ChangeText("Please Wait", "Deleting Asset");

            CatalogueItemDetail thisDetail = new CatalogueItemDetail();
            foreach (CatalogueItemDetail detail in catalogueManager._CatalogueItemDetails)
                if (detail.ItemID == this.itemGraphic.itemID)
                    thisDetail = detail;
            catalogueManager._CatalogueItemDetails.Remove(thisDetail);
            catalogueManager.ResyncCatalogueDatabaseAsync();

            using (DropboxClient dbx = new DropboxClient(AvoEx.AesEncryptor.DecryptString(PlayerPrefs.GetString("Token"))))
            {
                await cmd_Dropbox.DeleteAssetFolderAsync(dbx, "/Assets/Graphics/" + thisDetail.ItemID.ToString("D5"));
                Debug.Log("LOG:" + DateTime.Now.ToString() + " - " + itemGraphic.friendlyName + " Deleted");
            }

            foreach (CatalogueItemThumnail_Graphic goGraphice in GameObject.FindWithTag("ThumbnailGrid").GetComponentsInChildren<CatalogueItemThumnail_Graphic>())
            {
                if (goGraphice.assetID == this.itemGraphic.itemID)
                {
                    Destroy(goGraphice.gameObject);
                    break;
                }
            }
            loadingPanelUI.gameObject.SetActive(false);
            MessageBox.Show("Boom Shaka Laka", "Asset Now Deleted", () =>
            {
                GetComponent<PopupItemController>().HideDialog(0);
            });
            string localAssetPath = "/" + catalogueManager._DatabaseUID + "/Assets/Graphics/" + this.itemGraphic.itemID.ToString("D5") + "/";
            cmd_File.DeleteFolder(Application.persistentDataPath + localAssetPath, true);
        }
        else
        {
            GetComponent<PopupItemController>().HideDialog(0);
        }
    }

    public void ExportAsset()
    {
        if (itemGraphic.itemID == 0)
        {
            MessageBox.Show("Error", "Save Asset Before Exporting", () => { });
            return;
        }
        string path = FileBrowser.SaveFile("Select Save Location", "", Path.GetFileNameWithoutExtension(Application.persistentDataPath + itemGraphic.graphicPath), Path.GetExtension(Application.persistentDataPath + itemGraphic.graphicPath).Replace(".", ""));
        if (string.IsNullOrEmpty(path))
            return;
        File.Copy(Application.persistentDataPath + itemGraphic.graphicPath, path, true);
    }

    public void ImportGraphic()
    {
        string path = FileBrowser.OpenSingleFile("Select Texture", "", extensions);
        Directory.CreateDirectory(tempDataPath);
        if (!string.IsNullOrEmpty(path))
        {
            StartCoroutine(SetGraphicTexture(path));
            File.Copy(path, tempDataPath + Path.GetFileName(path), true);
            if (!string.IsNullOrEmpty(path))
            {
                var attr = File.GetAttributes(tempDataPath + Path.GetFileName(path));
                attr = attr & ~FileAttributes.ReadOnly;
                File.SetAttributes(tempDataPath + Path.GetFileName(path), attr);
            }
            graphicImorted.enabled = true;
            if (useSliceToggle.isOn)
            {
                panelSlice.SetActive(true);
                ToggleUseSlice();
            }
        }
        else
        {
            useSliceToggle.isOn = false;
            tempGraphicPath = null;
            graphicImorted.enabled = false;
            panelSlice.SetActive(false);
        }
    }

    private IEnumerator SetGraphicTexture(string path)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture("file:///" + path))
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
                graphicImorted.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one / 2, 100f);
                tempGraphicPath = tempDataPath + Path.GetFileName(path);
            }
        }
    }

    public void ToggleUseSlice()
    {
        if (useSliceToggle.isOn)
        {
            graphicImortedSliced.enabled = true;
            graphicImortedSliced.sprite = graphicImorted.sprite;
            if (tempGraphicPath != null)
                panelSlice.SetActive(true);
            if (itemGraphic.itemID != 0)
                panelSlice.SetActive(true);
            UpdateSlices();
        }
        else
        {
            graphicImortedSliced.enabled = false;
            panelSlice.SetActive(false);
        }
    }

    public void UpdateSlices()
    {
        xSlider.GetComponentInChildren<Text>().text = xSlider.value.ToString();
        ySlider.GetComponentInChildren<Text>().text = ySlider.value.ToString();
        sliceSprites = new List<Sprite>();

        long columnWidth = (long)graphicImorted.sprite.texture.width / (long)xSlider.value;
        long rowHeight = (long)graphicImorted.sprite.texture.height / (long)ySlider.value;

        long yOffset = 0;
        long xOffset = 0;
        for (int y = 0; y < ySlider.value; y++)
        {
            for (int x = 0; x < xSlider.value; x++)
            {
                Sprite sprite = Sprite.Create(graphicImorted.sprite.texture, new Rect(xOffset, yOffset, columnWidth, rowHeight), Vector2.one / 2, 100f);
                sliceSprites.Add(sprite);
                xOffset += columnWidth;
            }
            yOffset += rowHeight;
            xOffset = 0;
        }
    }
}
