using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Dropbox.Api;
using Dropbox.Api.Users;

public class OptionsController : MonoBehaviour {

    private CatalogueManager catalogueManager;

    public Image storagePercentImage;
    public Text storagePercentText;
    public Text emailAddressText;
    public  Text
            textModel,
            textAnimation,
            textAudio,
            textSkybox,
            textGraphic,
            textFont,
            textTile,
            textCodeSnip;
    private int
            modelCount,
            animationCount,
            audioCount,
            skyboxCount,
            graphicCount,
            fontCount,
            tileCount,
            codeSnipCount = 0;

    private void Start()
    {
        catalogueManager = GameObject.FindWithTag("CatalogueManager").GetComponent<CatalogueManager>();
        LoadAssetCounts();
        LoadStorageCountAsync();
    }

    private async void LoadStorageCountAsync()
    {
        using (DropboxClient dropboxClient = new DropboxClient(AvoEx.AesEncryptor.DecryptString(PlayerPrefs.GetString("Token"))))
        {
            FullAccount fullAccount = await dropboxClient.Users.GetCurrentAccountAsync();
            emailAddressText.text = fullAccount.Email;       
            SpaceUsage spaceUsage = await dropboxClient.Users.GetSpaceUsageAsync();
            IndividualSpaceAllocation individualSpace = spaceUsage.Allocation.AsIndividual.Value;
            double allocated = individualSpace.Allocated;
            allocated = allocated / 1024d / 1024d / 1024d;
            double used = spaceUsage.Used;
            used = used / 1024d / 1024d / 1024d;
            double percentage = used / allocated;
            storagePercentImage.fillAmount = (float)percentage;
            storagePercentText.text = used.ToString("n2") + "GB / " + allocated.ToString("n2") + "GB";
        }
    }

    private void LoadAssetCounts()
    {
       foreach(CatalogueItemDetail detail in catalogueManager._CatalogueItemDetails)
        {
            switch (detail.ItemType)
            {
                case CatalogueItemDetail.ItemTypes.Model:
                    modelCount ++;
                    break;
                case CatalogueItemDetail.ItemTypes.Animation:
                    animationCount++;
                    break;
                case CatalogueItemDetail.ItemTypes.Audio:
                    audioCount++;
                    break;
                case CatalogueItemDetail.ItemTypes.Skybox:
                    skyboxCount++;
                    break;
                case CatalogueItemDetail.ItemTypes.Graphic:
                    graphicCount++;
                    break;
                case CatalogueItemDetail.ItemTypes.Font:
                    fontCount++;
                    break;
                case CatalogueItemDetail.ItemTypes.Tile:
                    tileCount++;
                    break;
                case CatalogueItemDetail.ItemTypes.CodeSnip:
                    codeSnipCount++;
                    break;
            }
        }
        textModel.text = modelCount.ToString();
        textAnimation.text = animationCount.ToString();
        textAudio.text = audioCount.ToString();
        textSkybox.text = skyboxCount.ToString();
        textGraphic.text = graphicCount.ToString();
        textFont.text = fontCount.ToString();
        textTile.text = tileCount.ToString();
        textCodeSnip.text = codeSnipCount.ToString();
    }

    public void CloseApplication()
    {
        Application.Quit();
    }

    public void ClearLocalCache()
    {
        string localAssetPath = "/" + catalogueManager._DatabaseUID + "/";
        cmd_File.DeleteFolder(Application.persistentDataPath + localAssetPath, true);
        MessageBox.Show("Complete", "Asset Cat\nWill Now Reload", () =>
        {
            Destroy(GameObject.FindWithTag("CatalogueManager"));
            Invoke("InvokeSceneChange", 0.25f);
        });
    }

    public void SwitchAccount()
    {
        PlayerPrefs.DeleteKey("Token");
        PlayerPrefs.Save();
        MessageBox.Show("Complete", "Asset Cat\nWill Now Reload", () =>
        {
            Destroy(GameObject.FindWithTag("CatalogueManager"));
            Invoke("InvokeSceneChange", 0.25f);
        });
    }

    public void GoToWebsite()
    {
        Application.OpenURL("https://assetcat.app");
    }

    public void MoreStorage()
    {
        Application.OpenURL("https://harley-torrisi.itch.io/assetcat/devlog/74063/more-storage");
    }

    private void InvokeSceneChange()
    {
        SceneManager.LoadSceneAsync("04_SceneLoopBack", LoadSceneMode.Single);
    }
}
