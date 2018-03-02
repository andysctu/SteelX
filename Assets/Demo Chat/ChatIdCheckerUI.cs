using UnityEngine;
using System.Collections;

using UnityEngine.UI;

/// <summary>
/// This is used in the Editor Splash to properly inform the developer about the chat AppId requirement.
/// </summary>
[ExecuteInEditMode]
public class ChatIdCheckerUI : MonoBehaviour {

	public Text Description;

	void Update () {
	
		if ( string.IsNullOrEmpty(ChatSettings.Instance.AppId))
		{
            Description.text = "<Color=Red>WARNING:</Color>\nTo run this demo, please set the Chat AppId in the ChatSettings file.";
		}else{
			Description.text = string.Empty;
		}
	}
}
