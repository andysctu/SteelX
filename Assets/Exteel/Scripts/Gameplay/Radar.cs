using UnityEngine;

public class Radar : MonoBehaviour {
    private Camera radar;

    private Rect rect;
    private Texture2D texture;

    // Use this for initialization
    private void Start() {
        radar = GetComponent<Camera>();
        texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.blue);
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.Apply();
        Vector3 pos = radar.ViewportToScreenPoint(new Vector3(0.5f, 0.5f, 0f));
        rect = new Rect(pos.x, Screen.height - pos.y, 5f, 5f);
    }

    // Update is called once per frame
    private void OnGUI() {
        GUI.skin.box.normal.background = texture;
        GUI.Box(rect, GUIContent.none);
    }
}