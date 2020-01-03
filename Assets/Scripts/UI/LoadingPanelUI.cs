using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingPanelUI : MonoBehaviour {

    public Text headerText, messageText;

	public void ChangeText(string header, string message)
    {
        headerText.text = header;
        messageText.text = message;
    }
}
