using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Crosstales.FB;
using System.IO;
using Dropbox.Api;
using System;
using System.Linq;
using System.Runtime.InteropServices;

public class CatalogueItemManager_Font : MonoBehaviour {

    private ExtensionFilter[] extensions = new[] { new ExtensionFilter("All Supported Formats", "ttf", "otf") };
    private CatalogueItem_Font itemFont = new CatalogueItem_Font();
    private CatalogueItemThumnail_Font itemThumbnail;
    private CatalogueManager catalogueManager;

    private string tempDataPath;
    private string tempFontFilePath;
    public InputField assetFriendlyName;
    public RawImage previewImage;
    public Text loadFontButtonText;
    public Button loadFontButton;
    public InputField tagsInputField;
    public Toggle favouritesToggle;

    private System.Drawing.Font font;
    private System.Drawing.Text.PrivateFontCollection fontCollection;
    private int fontRenderSize = 38;
    const string renderTextString =
                "abcdefghijklmnopqrstuvwxyz\n" +
                "ABCDEFGHIJKLMNOTQRSTUVWXYZ\n" +
                "0123456789,:,;(*!?'/\")£$€%^&-+@\n" +
                "The Quick Brown Fox Jumps Over The Lazy Dog";
    const string renderTextStringShort = "The Quick Brown Fox";

    public void ObjectParse(CatalogueItem_Font fontParse, CatalogueItemThumnail_Font thumbnail)
    {
        this.itemFont = fontParse;
        if (itemFont.itemID != 0)
        {
            //Asset Exists
            itemThumbnail = thumbnail;
            assetFriendlyName.text = itemFont.friendlyName;
            favouritesToggle.isOn = itemFont.favourite;
            tagsInputField.text = string.Join("#", itemFont.tags);
            if (!string.IsNullOrEmpty(itemFont.fontPath))
            {
                StartCoroutine(SetLoadFont(Application.persistentDataPath + itemFont.fontPath));
                loadFontButtonText.GetComponentInChildren<Text>().text = "";
                loadFontButton.enabled = false;
            }
        }
    }

    IEnumerator SetLoadFont(string path)
    {
        yield return new WaitForSeconds(GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length + GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime);
        previewImage.enabled = true;
        LoadFontFile(path);
    }

    void Start()
    {
        catalogueManager = GameObject.FindWithTag("CatalogueManager").GetComponent<CatalogueManager>();
        tempDataPath = Application.persistentDataPath + "/Temp/" + Guid.NewGuid().ToString() + "/";
        previewImage.enabled = false;
        
    }

    public async void SaveAssetAsync()
    {
        if(string.IsNullOrEmpty(assetFriendlyName.text))
        {
            MessageBox.Show("Error", "Asset Name Is Missing", () => { });
            return;
        }
        if (itemFont.itemID == 0)
            if (string.IsNullOrEmpty(tempFontFilePath))
            {
                MessageBox.Show("Error", "No Font File Selected", () => { });
                return;
            }

        this.GetComponent<Button>().interactable = false;
        LoadingPanelUI loadingPanelUI = GetComponentInChildren<LoadingPanelUI>(true);
        loadingPanelUI.gameObject.SetActive(true);
        loadingPanelUI.ChangeText("Please Wait", "Assets Uploading");

        const string catalogueItemFileName = "CatalogueItem.asscat";
        const string itemThumnailPrefabName = "AssetThumnail_Font";

        if (itemFont.itemID == 0) //New Asset
        {
            catalogueManager._CreatedAssetCount++;

            CatalogueItemDetail itemDetail = new CatalogueItemDetail
            {
                ItemType = CatalogueItemDetail.ItemTypes.Font,
                ItemID = catalogueManager._CreatedAssetCount,
                CatalogueItemDirectory = "/Assets/Fonts/" + catalogueManager._CreatedAssetCount.ToString("D5") + "/",
                DateModified = DateTime.Now.ToString(),
                FriendlyName = assetFriendlyName.text,
                ItemTypeCategory = 0,
            };

            string localAssetPath = "/" + catalogueManager._DatabaseUID + itemDetail.CatalogueItemDirectory + "/";
            string localFontPath = localAssetPath + "/" + Path.GetFileName(tempFontFilePath);
            cmd_File.DeleteFolder(Application.persistentDataPath + localAssetPath, false);
            Directory.CreateDirectory(Application.persistentDataPath + localAssetPath);

            File.Copy(tempFontFilePath, Application.persistentDataPath + localFontPath, true);

            itemFont = new CatalogueItem_Font
            {
                friendlyName = assetFriendlyName.text,
                itemID = catalogueManager._CreatedAssetCount,
                modifiedDate = DateTime.Now.ToString(),
                fontPath = localFontPath,
                thumnailData = RenderFontTextToBitmapArray(256, 256, new System.Drawing.Font(fontCollection.Families[0], 28), renderTextStringShort, System.Drawing.Brushes.GhostWhite),
                tags = tagsInputField.text.Split('#'),
                itemTypeCategory = 0,
                favourite = favouritesToggle.isOn,
            };


            cmd_File.SerializeObject(Application.persistentDataPath + localAssetPath, catalogueItemFileName, itemFont);
            catalogueManager._CatalogueItemDetails.Add(itemDetail);
            catalogueManager.ResyncCatalogueDatabaseAsync();

            using (DropboxClient dbx = new DropboxClient(AvoEx.AesEncryptor.DecryptString(PlayerPrefs.GetString("Token"))))
            {
                await cmd_Dropbox.UploadFileAsync(dbx, tempFontFilePath, itemDetail.CatalogueItemDirectory, Path.GetFileName(tempFontFilePath));
                await cmd_Dropbox.UploadObjAsync(dbx, itemFont, itemDetail.CatalogueItemDirectory, catalogueItemFileName);
                Debug.Log("LOG:" + DateTime.Now.ToString() + " - " + itemFont.friendlyName + " Created");
                MessageBox.Show("Boom Shaka Laka", "Asset Now Added", () =>
                {
                    GetComponent<PopupItemController>().HideDialog(0);
                });
            }

            GameObject go = Instantiate(Resources.Load(itemThumnailPrefabName) as GameObject, GameObject.FindWithTag("ThumbnailGrid").transform);
            go.SendMessage("ObjectParse", itemFont);
        }
        else //Update Asset
        {
            foreach (CatalogueItemDetail itemDetail in catalogueManager._CatalogueItemDetails)
            {
                if (itemDetail.ItemID == itemFont.itemID)
                {
                    itemDetail.DateModified = DateTime.Now.ToString();
                    itemFont.modifiedDate = DateTime.Now.ToString();
                    itemDetail.FriendlyName = assetFriendlyName.text;
                    itemFont.friendlyName = assetFriendlyName.text;
                    itemFont.tags = tagsInputField.text.Split('#');
                    itemFont.favourite = favouritesToggle.isOn;
                    itemFont.itemTypeCategory = 0;
                    itemThumbnail.lable.text = assetFriendlyName.text;
                    itemThumbnail.ObjectParse(itemFont);
                    catalogueManager.ResyncCatalogueDatabaseAsync();
                    using (DropboxClient dropboxClient = new DropboxClient(AvoEx.AesEncryptor.DecryptString(PlayerPrefs.GetString("Token"))))
                    {
                        await cmd_Dropbox.UploadObjAsync(dropboxClient, itemFont, itemDetail.CatalogueItemDirectory, catalogueItemFileName);
                        Debug.Log("LOG:" + DateTime.Now.ToString() + " - " + itemFont.friendlyName + " Updated");
                        MessageBox.Show("Boom Shaka Laka", "Asset Now Updated", () =>
                        {
                            GetComponent<PopupItemController>().HideDialog(0);
                        });
                    }
                    string localPath = Application.persistentDataPath + "/" + catalogueManager._DatabaseUID + itemDetail.CatalogueItemDirectory + "/";
                    cmd_File.SerializeObject(localPath, catalogueItemFileName, itemFont);
                    return;
                }
            }
        }
        loadingPanelUI.gameObject.SetActive(false);
        this.GetComponent<Button>().interactable = true;
    }

    public async void DeleteAssetAsync()
    {
        if (itemFont.itemID != 0)
        {
            //Do Delete
            this.GetComponent<Button>().interactable = false;
            LoadingPanelUI loadingPanelUI = GetComponentInChildren<LoadingPanelUI>(true);
            loadingPanelUI.gameObject.SetActive(true);
            loadingPanelUI.ChangeText("Please Wait", "Deleting Asset");

            CatalogueItemDetail thisDetail = new CatalogueItemDetail();
            foreach (CatalogueItemDetail detail in catalogueManager._CatalogueItemDetails)
                if (detail.ItemID == this.itemFont.itemID)
                    thisDetail = detail;
            catalogueManager._CatalogueItemDetails.Remove(thisDetail);
            
            catalogueManager.ResyncCatalogueDatabaseAsync();
            //Debug.Log(thisDetail.CatalogueItemDirectory);
            
            using (DropboxClient dbx = new DropboxClient(AvoEx.AesEncryptor.DecryptString(PlayerPrefs.GetString("Token"))))
            {
                Debug.Log("/Assets/Fonts/" + thisDetail.ItemID);
                await cmd_Dropbox.DeleteAssetFolderAsync(dbx, "/Assets/Fonts/" + thisDetail.ItemID.ToString("D5"));
                Debug.Log("LOG:" + DateTime.Now.ToString() + " - " + itemFont.friendlyName + " Deleted");
            }

            string localAssetPath = "/" + catalogueManager._DatabaseUID + "/Assets/Fonts/" + this.itemFont.itemID.ToString("D5") + "/";
            cmd_File.DeleteFolder(Application.persistentDataPath + localAssetPath, true);

            foreach (CatalogueItemThumnail_Font goFont in GameObject.FindWithTag("ThumbnailGrid").GetComponentsInChildren<CatalogueItemThumnail_Font>())
            {
                if (goFont.assetID == this.itemFont.itemID)
                {
                    Destroy(goFont.gameObject);
                    break;
                }
            }
            loadingPanelUI.gameObject.SetActive(false);
            MessageBox.Show("Boom Shaka Laka", "Asset Now Deleted", () =>
            {
                GetComponent<PopupItemController>().HideDialog(0);
            });
        }
        else
        {
           GetComponent<PopupItemController>().HideDialog(0);
        }
    }

    public void ExportAsset()
    {
        if (itemFont.itemID == 0)
        {
            MessageBox.Show("Error", "Save Asset Before Exporting", () => { });
            return;
        }
        string path = FileBrowser.SaveFile("Select Save Location", "", Path.GetFileNameWithoutExtension(Application.persistentDataPath + itemFont.fontPath), Path.GetExtension(Application.persistentDataPath + itemFont.fontPath).Replace(".", ""));
        if (string.IsNullOrEmpty(path))
            return;
        File.Copy(Application.persistentDataPath + itemFont.fontPath, path, true);
    }

    public void SelectFontFile()
    {
        string path = FileBrowser.OpenSingleFile("Select Font Asset", "", extensions);
        Directory.CreateDirectory(tempDataPath);
        tempFontFilePath = null;
        if (string.IsNullOrEmpty(path))
        {
            //Canceled
            previewImage.enabled = false;
            loadFontButtonText.enabled = true;
            return;
        }
        previewImage.enabled = true;
        loadFontButtonText.enabled = false;
        tempFontFilePath = path;
        File.Copy(path, tempDataPath + Path.GetFileName(path), true);
        var attr = File.GetAttributes(tempDataPath + Path.GetFileName(path));
        attr = attr & ~FileAttributes.ReadOnly;
        File.SetAttributes(tempDataPath + Path.GetFileName(path), attr);
        LoadFontFile(path);
    }

    private void LoadFontFile(string path)
    {
        fontCollection = new System.Drawing.Text.PrivateFontCollection();
        fontCollection.AddFontFile(path);
        font = new System.Drawing.Font(fontCollection.Families[0], fontRenderSize);
        Texture2D texture = new Texture2D((int)previewImage.rectTransform.rect.width, (int)previewImage.rectTransform.rect.height);
        byte[] data = RenderFontTextToBitmapArray(texture.width, texture.height, font, renderTextString, System.Drawing.Brushes.GhostWhite);
        texture.LoadImage(data);
        previewImage.texture = texture;
    }

    private byte[] RenderFontTextToBitmapArray(int width, int height, System.Drawing.Font font, string textToRender, System.Drawing.Brush brush)
    {
        System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(width, height);
        System.Drawing.RectangleF rectf = new System.Drawing.RectangleF(0, 0, bmp.Width, bmp.Height);       
        System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bmp);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
        System.Drawing.StringFormat format = new System.Drawing.StringFormat()
        {
            Alignment = System.Drawing.StringAlignment.Center,
            LineAlignment = System.Drawing.StringAlignment.Center
        };

        g.DrawString(textToRender, font, brush, rectf, format);

        g.Flush();
        using (MemoryStream ms = new MemoryStream())
        {
            bmp.Save(ms, bmp.RawFormat);
            return ms.ToArray();
        }
    }

    public void ChangeFontRenderSize(Slider slider)
    {
        fontRenderSize = (int)slider.value;
        if (tempFontFilePath != null)
            LoadFontFile(tempFontFilePath);
        else if(itemFont.fontPath != null)
            LoadFontFile(Application.persistentDataPath + itemFont.fontPath);
    }
}
