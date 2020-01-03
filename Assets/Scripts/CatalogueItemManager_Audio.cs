using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Crosstales.FB;
using System;
using System.IO;
using Dropbox.Api;
using NLayer;

public class CatalogueItemManager_Audio : MonoBehaviour {

    private ExtensionFilter[] extensions = new[] { new ExtensionFilter("All Supported Formats", "mp3", "ogg", "wav", "aiff", "aif", "mod", "it", "s3m", "xm") };
    private CatalogueItem_Audio itemAudio = new CatalogueItem_Audio();
    private CatalogueItemThumnail_Audio itemThumbnail;
    private CatalogueManager catalogueManager;

    private string tempDataPath;
    private string tempAudioFilePath;
    public InputField assetFriendlyName;
    public AudioSource audioSource;
    public RawImage waveformTexture;
    public Image waveformCurrentTexture;
    public Text playButtonText;
    public Button loadAssetButton;
    public InputField tagsInputField;
    public Dropdown categoryDropDown;
    public Toggle favouritesToggle;

    public void ObjectParse(CatalogueItem_Audio audioParse, CatalogueItemThumnail_Audio thumbnail)
    {
        this.itemAudio = audioParse;
        if(itemAudio.itemID != 0)
        {
            //Asset Exists
            itemThumbnail = thumbnail;
            assetFriendlyName.text = itemAudio.friendlyName;
            if (!string.IsNullOrEmpty(itemAudio.audioPath))
                LoadAudioFile(Application.persistentDataPath + itemAudio.audioPath);
            tagsInputField.text = string.Join("#", itemAudio.tags);
            categoryDropDown.value = itemAudio.itemTypeCategory;
            favouritesToggle.isOn = itemAudio.favourite;
            loadAssetButton.GetComponentInChildren<Text>().text = "";
            loadAssetButton.enabled = false;
        }
    }

   
    void Start ()
    {
        catalogueManager = GameObject.FindWithTag("CatalogueManager").GetComponent<CatalogueManager>();
        tempDataPath = Application.persistentDataPath + "/Temp/" + Guid.NewGuid().ToString() + "/";
        StartCoroutine(DoAfterLoadAnim());
    }

    IEnumerator DoAfterLoadAnim()
    {
        yield return new WaitForSeconds(GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length + GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime);
        this.gameObject.transform.SetParent(GameObject.Find("Canvas").transform);
    }


    private void Update()
    {
        if (audioSource.isPlaying)
        {
            waveformCurrentTexture.fillAmount = audioSource.time / audioSource.clip.length;
            playButtonText.text = "";
        }
        else
        {
            playButtonText.text = "";
        }
    }

    private void LoadAudioFile(string path)
    {
        audioSource.clip = null;
        if (Path.GetExtension(path).ToUpper().Contains("mp3".ToUpper()))
        {
            StartCoroutine(GetAudioClipMP3(path));
        }
        else
        {
            StartCoroutine(GetAudioClip(path));
        }
    }

    IEnumerator GetAudioClipMP3(string path)
    {
        MpegFile mpegFile = new MpegFile(path);
        float[] samples = new float[mpegFile.Length / sizeof(float)];
        mpegFile.ReadSamples(samples, 0, (int)(mpegFile.Length / sizeof(float)));
        AudioClip audioClip = AudioClip.Create(Path.GetFileNameWithoutExtension(path), (int)(mpegFile.Duration.TotalSeconds * mpegFile.SampleRate), mpegFile.Channels, mpegFile.SampleRate, false);
        audioClip.SetData(samples, 0);
        audioSource.clip = audioClip;
        Texture2D tex = PaintWaveformSpectrum(audioSource.clip, 0.5F, (int)waveformTexture.rectTransform.rect.width, (int)waveformTexture.rectTransform.rect.width, Color.white);
        waveformTexture.texture = tex;
        Sprite mySprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
        waveformCurrentTexture.sprite = mySprite;
        audioSource.Stop();
        waveformTexture.color = new Color(waveformTexture.color.r, waveformTexture.color.g, waveformTexture.color.b, 1);
        waveformCurrentTexture.color = new Color(waveformCurrentTexture.color.r, waveformCurrentTexture.color.g, waveformCurrentTexture.color.b, 1);
        mpegFile.Dispose();
        yield return null;
    }

    IEnumerator GetAudioClip(string path)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file:///" + path, AudioType.UNKNOWN))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError)
            {
                Debug.Log(www.error);
            }
            else
            {
                AudioClip myClip = DownloadHandlerAudioClip.GetContent(www);
                audioSource.clip = myClip;
                Texture2D tex = PaintWaveformSpectrum(audioSource.clip, 0.5F, (int)waveformTexture.rectTransform.rect.width, (int)waveformTexture.rectTransform.rect.width, Color.white);
                waveformTexture.texture = tex;
                Sprite mySprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                waveformCurrentTexture.sprite = mySprite;
                audioSource.Stop();
                waveformTexture.color = new Color(waveformTexture.color.r, waveformTexture.color.g, waveformTexture.color.b, 1);
                waveformCurrentTexture.color = new Color(waveformCurrentTexture.color.r, waveformCurrentTexture.color.g, waveformCurrentTexture.color.b, 1);
            }
        }
    }

    public void SetPlayState(string command)
    {
        if (audioSource.clip == null)
            return;
        switch (command)
        {
            case "PlayPause":
                if (audioSource.isPlaying)
                {
                    audioSource.Pause();
                }
                else
                {
                    audioSource.Play(); 
                }    
                break;
            case "Back":
                audioSource.Stop();
                audioSource.Play();
                break;
            case "Loop":
                audioSource.loop = !audioSource.loop;
                if (!audioSource.isPlaying)
                {
                    audioSource.Stop();
                    audioSource.Play();
                }
                break;
        }
        
    }

    public void SelectAudioFile()
    {
        waveformCurrentTexture.fillAmount = 0;
        string path = FileBrowser.OpenSingleFile("Select Audio Asset", "", extensions);
        Directory.CreateDirectory(tempDataPath);
        tempAudioFilePath = null;
        if (string.IsNullOrEmpty(path))
        {
            waveformTexture.color = new Color(waveformTexture.color.r, waveformTexture.color.g, waveformTexture.color.b, 0);
            waveformCurrentTexture.color = new Color(waveformCurrentTexture.color.r, waveformCurrentTexture.color.g, waveformCurrentTexture.color.b, 0);
            return;
        }
        tempAudioFilePath = path;
        File.Copy(path, tempDataPath + Path.GetFileName(path), true);
        if (!string.IsNullOrEmpty(path))
        {
            var attr = File.GetAttributes(tempDataPath + Path.GetFileName(path));
            attr = attr & ~FileAttributes.ReadOnly;
            File.SetAttributes(tempDataPath + Path.GetFileName(path), attr);
        }
        LoadAudioFile(path);
        //Alpha added at LoadFiles
    }

    public async void SaveAssetAsyn()
    {
        if (string.IsNullOrEmpty(assetFriendlyName.text))
        {
            MessageBox.Show("Error", "Asset Name Is Missing", () => { });
            return;
        }
        if (itemAudio.itemID == 0)
            if (string.IsNullOrEmpty(tempAudioFilePath))
            {
                MessageBox.Show("Error", "No Audio File Selected", () => { });
                return;
            }

        this.GetComponent<Button>().interactable = false;
        LoadingPanelUI loadingPanelUI = GetComponentInChildren<LoadingPanelUI>(true);
        loadingPanelUI.gameObject.SetActive(true);
        loadingPanelUI.ChangeText("Please Wait", "Uploading");

        const string catalogueItemFileName = "CatalogueItem.asscat";
        const string itemThumnailPrefabName = "AssetThumnail_Audio";

        if (itemAudio.itemID == 0) //New Asset
        {
            catalogueManager._CreatedAssetCount++;

            CatalogueItemDetail itemDetail = new CatalogueItemDetail
            {
                ItemType = CatalogueItemDetail.ItemTypes.Audio,
                ItemID = catalogueManager._CreatedAssetCount,
                CatalogueItemDirectory = "/Assets/Audio/" + catalogueManager._CreatedAssetCount.ToString("D5") + "/",
                DateModified = DateTime.Now.ToString(),
                FriendlyName = assetFriendlyName.text,
                ItemTypeCategory = categoryDropDown.value,
            };

            string localAssetPath = "/" + catalogueManager._DatabaseUID + itemDetail.CatalogueItemDirectory + "/";
            string localAudioPath = localAssetPath + "/" + Path.GetFileName(tempAudioFilePath);
            cmd_File.DeleteFolder(Application.persistentDataPath + localAssetPath, false);
            Directory.CreateDirectory(Application.persistentDataPath + localAssetPath);

            File.Copy(tempAudioFilePath, Application.persistentDataPath + localAudioPath, true);

            itemAudio = new CatalogueItem_Audio
            {
                friendlyName = assetFriendlyName.text,
                itemID = catalogueManager._CreatedAssetCount,
                modifiedDate = DateTime.Now.ToString(),
                audioPath = localAudioPath,
                thumnailData = waveformCurrentTexture.sprite.texture.EncodeToPNG(),
                tags = tagsInputField.text.Split('#'),
                itemTypeCategory = categoryDropDown.value,
                favourite = favouritesToggle.isOn,
            };
            cmd_File.SerializeObject(Application.persistentDataPath + localAssetPath, catalogueItemFileName, itemAudio);
            catalogueManager._CatalogueItemDetails.Add(itemDetail);
            catalogueManager.ResyncCatalogueDatabaseAsync();

            using (DropboxClient dbx = new DropboxClient(AvoEx.AesEncryptor.DecryptString(PlayerPrefs.GetString("Token"))))
            {
                await cmd_Dropbox.UploadFileAsync(dbx, tempAudioFilePath, itemDetail.CatalogueItemDirectory, Path.GetFileName(tempAudioFilePath));
                await cmd_Dropbox.UploadObjAsync(dbx, itemAudio, itemDetail.CatalogueItemDirectory, catalogueItemFileName);
                Debug.Log("LOG:" + DateTime.Now.ToString() + " - " + itemAudio.friendlyName + " Created");
                MessageBox.Show("Boom Shaka Laka", "Asset Now Added", () =>
                {
                    GetComponent<PopupItemController>().HideDialog(0);
                });
            }

            GameObject go = Instantiate(Resources.Load(itemThumnailPrefabName) as GameObject, GameObject.FindWithTag("ThumbnailGrid").transform);
            go.SendMessage("ObjectParse", itemAudio);
        }
        else //Update Asset
        {
            foreach(CatalogueItemDetail itemDetail in catalogueManager._CatalogueItemDetails)
            {
                if(itemDetail.ItemID == itemAudio.itemID)
                {
                    itemDetail.DateModified = DateTime.Now.ToString();
                    itemAudio.modifiedDate = DateTime.Now.ToString();
                    itemDetail.FriendlyName = assetFriendlyName.text;
                    itemAudio.friendlyName = assetFriendlyName.text;
                    itemAudio.tags = tagsInputField.text.Split('#');
                    itemAudio.favourite = favouritesToggle.isOn;
                    itemAudio.itemTypeCategory = categoryDropDown.value;
                    itemThumbnail.lable.text = assetFriendlyName.text;
                    itemThumbnail.ObjectParse(itemAudio);
                    catalogueManager.ResyncCatalogueDatabaseAsync();
                    using (DropboxClient dropboxClient = new DropboxClient(AvoEx.AesEncryptor.DecryptString(PlayerPrefs.GetString("Token"))))
                    {
                        await cmd_Dropbox.UploadObjAsync(dropboxClient, itemAudio, itemDetail.CatalogueItemDirectory, catalogueItemFileName);
                        Debug.Log("LOG:" + DateTime.Now.ToString() + " - " + itemAudio.friendlyName + " Updated");
                        MessageBox.Show("Boom Shaka Laka", "Asset Now Updated", () =>
                        {
                            GetComponent<PopupItemController>().HideDialog(0);
                        });
                    }
                    string localPath = Application.persistentDataPath + "/" + catalogueManager._DatabaseUID + itemDetail.CatalogueItemDirectory + "/";
                    cmd_File.SerializeObject(localPath, catalogueItemFileName, itemAudio);
                    return;
                }
            }
        }
        loadingPanelUI.gameObject.SetActive(false);
        this.GetComponent<Button>().interactable = true;
    }

    public async void DeleteAssetSaync()
    {
        if (itemAudio.itemID != 0)
        {
            this.GetComponent<Button>().interactable = false;
            LoadingPanelUI loadingPanelUI = GetComponentInChildren<LoadingPanelUI>(true);
            loadingPanelUI.gameObject.SetActive(true);
            loadingPanelUI.ChangeText("Please Wait", "Deleting Asset");

            CatalogueItemDetail thisDetail = new CatalogueItemDetail();
            foreach (CatalogueItemDetail detail in catalogueManager._CatalogueItemDetails)
                if (detail.ItemID == this.itemAudio.itemID)
                    thisDetail = detail;
            catalogueManager._CatalogueItemDetails.Remove(thisDetail);
            catalogueManager.ResyncCatalogueDatabaseAsync();

            using (DropboxClient dbx = new DropboxClient(AvoEx.AesEncryptor.DecryptString(PlayerPrefs.GetString("Token"))))
            {
                await cmd_Dropbox.DeleteAssetFolderAsync(dbx, "/Assets/Audio/" + thisDetail.ItemID.ToString("D5"));
                Debug.Log("LOG:" + DateTime.Now.ToString() + " - " + itemAudio.friendlyName + " Deleted");
            }
            foreach (CatalogueItemThumnail_Audio goAudio in GameObject.FindWithTag("ThumbnailGrid").GetComponentsInChildren<CatalogueItemThumnail_Audio>())
            {
                if (goAudio.assetID == this.itemAudio.itemID)
                {
                    Destroy(goAudio.gameObject);
                    break;
                }
            }
            loadingPanelUI.gameObject.SetActive(false);
            MessageBox.Show("Boom Shaka Laka", "Asset Now Deleted", () =>
            {
                GetComponent<PopupItemController>().HideDialog(0);
            });
            string localAssetPath = "/" + catalogueManager._DatabaseUID + "/Assets/Audio/" + this.itemAudio.itemID.ToString("D5") + "/";
            cmd_File.DeleteFolder(Application.persistentDataPath + localAssetPath, true);
        }
        else
        {
            GetComponent<PopupItemController>().HideDialog(0);
        }
    }

    public void ExportAsset()
    {
        if (itemAudio.itemID == 0)
        {
            MessageBox.Show("Error", "Save Asset Before Exporting", () => { });
            return;
        }
        string path = FileBrowser.SaveFile("Select Save Location", "", Path.GetFileNameWithoutExtension(Application.persistentDataPath + itemAudio.audioPath), Path.GetExtension(Application.persistentDataPath + itemAudio.audioPath).Replace(".",""));
        if (string.IsNullOrEmpty(path))
            return;
        File.Copy(Application.persistentDataPath + itemAudio.audioPath, path, true);
    }

    private Texture2D PaintWaveformSpectrum(AudioClip audio, float saturation, int width, int height, Color col)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        float[] samples = new float[audio.samples];
        float[] waveform = new float[width];
        audio.GetData(samples, 0);
        int packSize = (audio.samples / width) + 1;
        int s = 0;
        for (int i = 0; i < audio.samples; i += packSize)
        {
            waveform[s] = Mathf.Abs(samples[i]);
            s++;
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                tex.SetPixel(x, y, Color.clear);
            }
        }

        for (int x = 0; x < waveform.Length; x++)
        {
            for (int y = 0; y <= waveform[x] * ((float)height * .75f); y++)
            {
                tex.SetPixel(x, (height / 2) + y, col);
                tex.SetPixel(x, (height / 2) - y, col);
            }
        }
        tex.Apply();
        return tex;
    }
}
