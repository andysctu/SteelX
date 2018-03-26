using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof (ChatNewGui))]
public class NamePickNewGui : MonoBehaviour
{
	private const string UserNamePlayerPref = "NamePickUserName";
	
	public ChatNewGui chatNewComponent;
	
	public InputField idInput;
	
	public void Start()
	{
		this.chatNewComponent = FindObjectOfType<ChatNewGui>();
		
		
		string prefsName = PlayerPrefs.GetString(NamePickNewGui.UserNamePlayerPref);
		if (!string.IsNullOrEmpty(prefsName))
		{
			this.idInput.text = prefsName;
		}
	}
	
	
	// new UI will fire "EndEdit" event also when loosing focus. So check "enter" key and only then StartChat.
	public void EndEditOnEnter()
	{
		if (Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.KeypadEnter))
		{
			this.StartChat();
		}
	}
	
	public void StartChat()
	{
		ChatNewGui chatNewComponent = FindObjectOfType<ChatNewGui>();
		chatNewComponent.UserName = this.idInput.text.Trim();
		chatNewComponent.Connect();
		enabled = false;
		
		PlayerPrefs.SetString(NamePickNewGui.UserNamePlayerPref, chatNewComponent.UserName);
	}
}