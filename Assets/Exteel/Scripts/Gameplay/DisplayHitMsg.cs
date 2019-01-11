using UnityEngine;
using UnityEngine.UI;

public class DisplayHitMsg : MonoBehaviour {
    public enum HitMsg { HIT, KILL, DEFENSE, CRITICAL, HEAL }
    [SerializeField]private GameObject HitMsgPrefab;
    [SerializeField]private Transform HitMsgTransform;

#if UNITY_EDITOR
    [NamedArrayAttribute(new string[] { "HIT", "KILL", "DEFENSE", "CRITICAL", "HEAL"})]
#endif
    [SerializeField]private Sprite[] sprites = new Sprite[5];    
    private HUDText currentHUDText;//current hud text that is displaying to the target player ; only one play at the same time
    private HUDText[] HUDTexts;

    private void Start() {
        InitHUDTexts();
    }

    private void InitHUDTexts() {
        HUDTexts = new HUDText[System.Enum.GetNames(typeof(HitMsg)).Length];

        for (int i = 0; i < HUDTexts.Length; i++) {
            if(sprites[i] == null) {
                HUDTexts[i] = null;
                continue;
            }
            GameObject g = Instantiate(HitMsgPrefab, HitMsgTransform);
            g.GetComponent<Image>().sprite = sprites[i];
            g.transform.localPosition = Vector3.zero;
            HUDTexts[i] = new HUDText();
            g.name = sprites[i].name;
            //Init
            HUDTexts[i].Init(g);
        }
    }

    private void Update() {
        if(currentHUDText != null) {
            currentHUDText.Update();
        }
    }

    public void Display(HitMsg type, Camera cam) {
        if(HUDTexts[(int)type] == null)
            return;

        if (currentHUDText != null) {
            currentHUDText.StopDisplay();
        }

        HUDTexts[(int)type].Display(cam);
        currentHUDText = HUDTexts[(int)type];
    }
}