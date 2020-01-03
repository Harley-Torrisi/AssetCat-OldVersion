/*********************************************************************************************************
 ******************************************USAGE EXAMPLE**************************************************
 *********************************************************************************************************
MessageBox.Show("New Catalague", "Select\n\nEmpty Folder\n- with -\nWrite Access", () =>
{
    string newPath = FileBrowser.OpenSingleFolder("Select Empty Folder", Application.persistentDataPath);
    if (string.IsNullOrEmpty(newPath))
        return;
    if (!cmd_File.IsFolderEmpty(newPath))
        MessgeBox.Show("Error!", "Folder is not empty", () => { });
    else if (!cmd_File.IsDirectoryWritable(newPath))
        MessgeBox.Show("Error!", "Folder is not writable", () => { });
    else
        MessgeBox.Show("Yay!", "Now We Can Make Something Happen", () => { });
});

-OR- Non Embedded

MessageBox.Show("New Catalague", "You Clicked Save", () => { });
**********************************************************************************************************/

using System;
using UnityEngine;

public class MessageBox
{
    private const string prefabePath = @"MessageBox";
    public static void Show(string title, string message, Action onDoneCallback)
    {
        MessageBoxDialog go = (UnityEngine.Object.Instantiate(Resources.Load(prefabePath)) as GameObject).GetComponent<MessageBoxDialog>();
        go.titleText.text = title;
        go.messageText.text = message;
        go.OnDone += onDoneCallback;
    }
}
