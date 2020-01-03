using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Dropbox.Api;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Dropbox.Api.Files;
using System.Runtime.Serialization;

public class cmd_Dropbox : MonoBehaviour {

    private const string _AppKey = "pj3bryojg3y1rsf";
    private const string _AppSecret = "p4iajydn1x6eoor";
    
    public static void LaunchAuthenticationRequest()
    {
        Uri uri = DropboxOAuth2Helper.GetAuthorizeUri(_AppKey);
        Application.OpenURL(uri.AbsoluteUri);
    }

    public static async Task<Stream> DownloadAsync(DropboxClient dbx, string folder, string file)
    {
        try
        {
            await dbx.Files.GetMetadataAsync((folder + "/" + file).Replace("//", "/"));
            var response = await dbx.Files.DownloadAsync((folder + "/" + file).Replace("//", "/"));
            return await response.GetContentAsStreamAsync();
        }
        catch
        {
            return null;
        }
    }

    public static async Task<Byte[]> DownloadFolderAsync(DropboxClient dbx, string folder)
    {
        //await dbx.Files.GetMetadataAsync(folder);
        using (var response = await dbx.Files.DownloadZipAsync(folder))
        {
            return await response.GetContentAsByteArrayAsync();
        }
    }

    public static async Task<FileMetadata> UploadObjAsync(DropboxClient dbx, object obj, string folder, string file)
    {
        //.net
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        using (MemoryStream memoryStream = new MemoryStream())
        {
            binaryFormatter.Serialize(memoryStream, obj);
            memoryStream.Position = 0;
            try
            {
                return await dbx.Files.UploadAsync((folder + "/" + file).Replace("//", "/"), WriteMode.Overwrite.Instance, body: memoryStream);
            }
            catch
            {
                Debug.Log("Upload Failed");
                return null;
            }
        }        
    }

    public static async Task<FileMetadata> UploadFileAsync(DropboxClient dbx, string filePath, string targetFolder, string targetFile)
    {
        using (Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            stream.Position = 0;
            try
            {
                return await dbx.Files.UploadAsync((targetFolder + "/" + targetFile).Replace("//", "/"), WriteMode.Overwrite.Instance, body: stream);
            }
            catch
            {
                Debug.Log("Upload Failed");
                return null;
            }
        }
    }

    public static async Task<DeleteResult> DeleteAssetFolderAsync(DropboxClient dbx, string folderPath)
    {
        return await dbx.Files.DeleteV2Async(folderPath);
    }

    public static async Task<OAuth2Response> ValidateAccessCode(string code)
    {
        try
        {
            OAuth2Response response = await DropboxOAuth2Helper.ProcessCodeFlowAsync(code, _AppKey, _AppSecret);
            return response;
        }
        catch (OAuth2Exception e)
        {
            Debug.Log(e.Message);
            return null;
        }
    }

    public static async Task<Dropbox.Api.Users.FullAccount> VerifyToken()
    {
        using (var dbx = new DropboxClient(AvoEx.AesEncryptor.DecryptString(PlayerPrefs.GetString("Token"))))
        {
            try
            {
                var full = await dbx.Users.GetCurrentAccountAsync();
                return full;
            }
            catch (AuthException e)
            {
                Debug.Log(e.Message);
                return null;
            }
        }
    }
}
