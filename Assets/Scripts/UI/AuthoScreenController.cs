using Dropbox.Api;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class AuthoScreenController : MonoBehaviour {

    public Canvas canvas;
    public GameObject codeInputBox;
    public GameObject verifyButton;

    bool clearTokenOnLoad = false;

	void Start() {
    #if DEBUG
        if (clearTokenOnLoad)
            PlayerPrefs.SetString("Token", "");
    #endif
        codeInputBox.SetActive(false);
        verifyButton.SetActive(false);
        canvas.enabled = false;
        CheckUserLogonAsync();
    }

    async void CheckUserLogonAsync()
    {
        if (string.IsNullOrEmpty(PlayerPrefs.GetString("Token")))
        {
            canvas.enabled = true;
            Debug.Log("No Authentication  Present");
        }
        else
        {
            Dropbox.Api.Users.FullAccount account = await cmd_Dropbox.VerifyToken();
            if (account != null)
            {
                Debug.Log("Authentication Success");
                SceneManager.LoadSceneAsync("02_DataLoad", LoadSceneMode.Single);
            }
            else
            {
                MessageBox.Show("Error", "Dropbox Token Has Expired\n\nYou Must Register A New Token", () => {
                    canvas.enabled = true;
                });
                
            }
        }
    }

    public void DropBoxLogin()
    {
        cmd_Dropbox.LaunchAuthenticationRequest();
        Invoke("ShowInputField", 1);
    }

    private void ShowInputField()
    {
        codeInputBox.SetActive(true);
        verifyButton.SetActive(true);
    }

    //public async void CodeInputChnagedAsync(Text tex)
    //{
    //    if (string.IsNullOrEmpty(tex.text))
    //        return;
    //    OAuth2Response response = await cmd_Dropbox.ValidateAccessCode(tex.text);
    //    if (response != null)
    //    {
    //        MessageBox.Show("Success", "Authenticated successfully.\n\nYou can now start using Asset Cat", () =>
    //        {
    //            PlayerPrefs.SetString("Token", AvoEx.AesEncryptor.Encrypt(response.AccessToken));
    //            PlayerPrefs.Save();
    //            SceneManager.LoadSceneAsync("02_DataLoad");
    //        });
    //    }
    //    else
    //    {
    //        MessageBox.Show("Error", tex.text + "\n\nFailed To Authenticate\n\nEnter code again or get new code", () => { });
    //    }
    //}

    public async void VerifyTokenAsync(InputField tex)
    {
        if (string.IsNullOrEmpty(tex.text))
            return;
        OAuth2Response response = await cmd_Dropbox.ValidateAccessCode(tex.text);
        if (response != null)
        {
            MessageBox.Show("Success", "Authenticated successfully.\n\nYou can now start using Asset Cat", () =>
            {
                PlayerPrefs.SetString("Token", AvoEx.AesEncryptor.Encrypt(response.AccessToken));
                PlayerPrefs.Save();
                SceneManager.LoadSceneAsync("02_DataLoad");
            });
        }
        else
        {
            MessageBox.Show("Error", tex.text + "\n\nFailed To Authenticate\n\nEnter code again or get new code", () => { });
        }
    }

    public void CodeInputFieldClicked(InputField inputField)
    {
        inputField.text = GUIUtility.systemCopyBuffer;
    }
}
