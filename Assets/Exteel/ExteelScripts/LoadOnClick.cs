using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class LoadOnClick : MonoBehaviour {

	public string LoginURL = "https://afternoon-temple-1885.herokuapp.com/login";
	public InputField user, pass;
	public Text error;

	public void Login(){
		WWWForm form = new WWWForm();
		form.AddField("username", user.text);
		form.AddField("password", pass.text);

		WWW www = new WWW(LoginURL, form);

		while (!www.isDone) {}
	
		if (www.responseHeaders["STATUS"] == "HTTP/1.1 200 OK") {
			Application.LoadLevel (1);
		}
		else {
			error.gameObject.SetActive(true);
		}
	}
}
