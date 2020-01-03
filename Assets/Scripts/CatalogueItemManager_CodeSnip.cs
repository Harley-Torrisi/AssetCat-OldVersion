using Dropbox.Api;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class CatalogueItemManager_CodeSnip : MonoBehaviour {

    private CatalogueItem_CodeSnip _ItemCodeSnip = new CatalogueItem_CodeSnip();
    private CatalogueManager _CatalogueManager;

    private CatalogueItemThumnail_CodeSnip _ItemThumbnail;

    public CodeEditor _CodeEditor;
    public InputField _AssetName;
    public InputField tagsInputField;
    public Toggle favouritesToggle;


    public void ObjectParse(CatalogueItem_CodeSnip codeSnipParse, CatalogueItemThumnail_CodeSnip thumbnail)
    {
        this._ItemCodeSnip = codeSnipParse;
        if (_ItemCodeSnip._ItemID != 0)
        {
            //Existing Asset
            _ItemThumbnail = thumbnail;
            _AssetName.text = _ItemCodeSnip._FriendlyName;
            _CodeEditor.mainInput.text = _ItemCodeSnip._CodeText;
            _CodeEditor.mainText.SetText(_ItemCodeSnip._CodeText, true);
            _CodeEditor.WriteEvent(_CodeEditor.mainInput.text);
            favouritesToggle.isOn = _ItemCodeSnip.favourite;
            tagsInputField.text = string.Join("#", _ItemCodeSnip.tags);
        }
    }

    public async void SaveAssetAsync()
    {
        if (string.IsNullOrEmpty(_AssetName.text))
        {
            MessageBox.Show("Error", "Asset Name Is Missing", () => { });
            return;
        }

        this.GetComponent<Button>().interactable = false;
        LoadingPanelUI loadingPanelUI = GetComponentInChildren<LoadingPanelUI>(true);
        loadingPanelUI.gameObject.SetActive(true);
        loadingPanelUI.ChangeText("Please Wait", "Uploading");

        const string _ItemFileName = "CatalogueItem.asscat";
        const string _ThumnailPrefabName = "AssetThumnail_ConeSnip";
        _CatalogueManager = GameObject.FindWithTag("CatalogueManager").GetComponent<CatalogueManager>();
        
        _ItemCodeSnip = new CatalogueItem_CodeSnip
        {
            _FriendlyName = _AssetName.text,
            _CodeText = _CodeEditor.mainText.text,
            _ItemID = _ItemCodeSnip._ItemID,
            _ModifiedDate = DateTime.Now.ToString(),
            tags = tagsInputField.text.Split('#'),
            itemTypeCategory = 0,
            favourite = favouritesToggle.isOn,
        };

        if (_ItemCodeSnip._ItemID == 0) //New Asset
        {
            _CatalogueManager._CreatedAssetCount++;
            _ItemCodeSnip._ItemID = _CatalogueManager._CreatedAssetCount;
            CatalogueItemDetail _ItemDetail = new CatalogueItemDetail
            {
                ItemType = CatalogueItemDetail.ItemTypes.CodeSnip,
                ItemID = _CatalogueManager._CreatedAssetCount,
                CatalogueItemDirectory = "/Assets/CodeSnips/" + _CatalogueManager._CreatedAssetCount.ToString("D5") + "/",
                DateModified = DateTime.Now.ToString(),
                FriendlyName = _ItemCodeSnip._FriendlyName,
                ItemTypeCategory = 0,
            };

            _CatalogueManager._CatalogueItemDetails.Add(_ItemDetail);
            _CatalogueManager.ResyncCatalogueDatabaseAsync();

            string _LocalAssetPath = Application.persistentDataPath + "/" + _CatalogueManager._DatabaseUID + _ItemDetail.CatalogueItemDirectory + "/";
            cmd_File.DeleteFolder(_LocalAssetPath, false);

            using (DropboxClient _DropboxClient = new DropboxClient(AvoEx.AesEncryptor.DecryptString(PlayerPrefs.GetString("Token"))))
            {
                await cmd_Dropbox.UploadObjAsync(_DropboxClient, _ItemCodeSnip, _ItemDetail.CatalogueItemDirectory, _ItemFileName);
                Debug.Log("LOG:" + DateTime.Now.ToString() + " - " + _ItemCodeSnip._FriendlyName + " Created");
                MessageBox.Show("Boom Shaka Laka", "Asset Now Added", () =>
                {
                    GetComponent<PopupItemController>().HideDialog(0);
                });
            }

            Directory.CreateDirectory(_LocalAssetPath);
            cmd_File.SerializeObject(_LocalAssetPath, _ItemFileName, _ItemCodeSnip);
            GameObject go = Instantiate(Resources.Load(_ThumnailPrefabName) as GameObject, GameObject.FindWithTag("ThumbnailGrid").transform);
            go.SendMessage("ObjectParse", _ItemCodeSnip);
        }
        else
        {
            foreach (CatalogueItemDetail _itemDetail in _CatalogueManager._CatalogueItemDetails)
            {
                if (_itemDetail.ItemID == _ItemCodeSnip._ItemID)
                {
                    _itemDetail.DateModified = DateTime.Now.ToString();
                    _itemDetail.FriendlyName = _ItemCodeSnip._FriendlyName;
                    _ItemThumbnail.lable.text = _ItemCodeSnip._FriendlyName;
                    _ItemThumbnail.ObjectParse(_ItemCodeSnip);
                    _CatalogueManager.ResyncCatalogueDatabaseAsync();
                    using (DropboxClient dropboxClient = new DropboxClient(AvoEx.AesEncryptor.DecryptString(PlayerPrefs.GetString("Token"))))
                    {
                        await cmd_Dropbox.UploadObjAsync(dropboxClient, _ItemCodeSnip, _itemDetail.CatalogueItemDirectory, _ItemFileName);
                        Debug.Log("LOG:" + DateTime.Now.ToString() + " - " + _ItemCodeSnip._FriendlyName + " Updated");
                        MessageBox.Show("Boom Shaka Laka", "Asset Now Updated", () =>
                        {
                            GetComponent<PopupItemController>().HideDialog(0);
                        });
                    }
                    string localPath = Application.persistentDataPath + "/" + _CatalogueManager._DatabaseUID + _itemDetail.CatalogueItemDirectory + "/";
                    cmd_File.SerializeObject(localPath, _ItemFileName, _ItemCodeSnip);
                    return;
                }
            }
        }
        loadingPanelUI.gameObject.SetActive(false);
        this.GetComponent<Button>().interactable = true;
    }

    public async void DeleteAssetAsync()
    {
        if (_ItemCodeSnip._ItemID != 0)
        {
            this.GetComponent<Button>().interactable = false;
            LoadingPanelUI loadingPanelUI = GetComponentInChildren<LoadingPanelUI>(true);
            loadingPanelUI.gameObject.SetActive(true);
            loadingPanelUI.ChangeText("Please Wait", "Deleting Asset");
            _CatalogueManager = GameObject.FindWithTag("CatalogueManager").GetComponent<CatalogueManager>();
            CatalogueItemDetail thisDetail = new CatalogueItemDetail();
            foreach (CatalogueItemDetail detail in _CatalogueManager._CatalogueItemDetails)
                if (detail.ItemID == this._ItemCodeSnip._ItemID)
                    thisDetail = detail;
            _CatalogueManager._CatalogueItemDetails.Remove(thisDetail);
            _CatalogueManager.ResyncCatalogueDatabaseAsync();

            using (DropboxClient dbx = new DropboxClient(AvoEx.AesEncryptor.DecryptString(PlayerPrefs.GetString("Token"))))
            {
                await cmd_Dropbox.DeleteAssetFolderAsync(dbx, "/Assets/CodeSnips/" + thisDetail.ItemID.ToString("D5"));
                Debug.Log("LOG:" + DateTime.Now.ToString() + " - " + _ItemCodeSnip._FriendlyName + " Deleted");
            }

            foreach (CatalogueItemThumnail_CodeSnip goCodeSnip in GameObject.FindWithTag("ThumbnailGrid").GetComponentsInChildren<CatalogueItemThumnail_CodeSnip>())
            {
                if (goCodeSnip.assetID == this._ItemCodeSnip._ItemID)
                {
                    Destroy(goCodeSnip.gameObject);
                    break;
                }
            }
            loadingPanelUI.gameObject.SetActive(false);
            MessageBox.Show("Boom Shaka Laka", "Asset Now Deleted", () =>
            {
                GetComponent<PopupItemController>().HideDialog(0);
            });
            string localAssetPath = "/" + _CatalogueManager._DatabaseUID + "/Assets/CodeSnips/" + this._ItemCodeSnip._ItemID.ToString("D5") + "/";
            cmd_File.DeleteFolder(Application.persistentDataPath + localAssetPath, true);
        }
        else
        {
            GetComponent<PopupItemController>().HideDialog(0);
        }
    }
}
