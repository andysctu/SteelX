using UnityEngine;
using UnityEngine.UI;

public class EscPanel : MonoBehaviour {
    [SerializeField] private Button ExitGame, Settings, Continue;
    [SerializeField] private GameObject OptionsPanel, SettingsPanel;
    private GameManager gm;

    //Settings Panel
    private GameObject Volume, MouseSensitivity, QualitySetting, FPS;
    private Button ExitSettingsPanel, ApplyQualityButton;
    private Scrollbar Volume_scrollbar, MS_scrollbar;
    private Text vol_text, ms_text;
    private Dropdown QS_dropdown, FPS_dropdown;

    private void Start() {
        FindGameManager();
        InitComponents();
        InitButtons();

        UpdateValues();
    }

    private void FindGameManager() {
        gm = FindObjectOfType<GameManager>();
    }

    private void InitButtons() {
        ExitGame.onClick.AddListener(() => gm.ExitGame());
        Continue.onClick.AddListener(() => EnableEscPanel());
        Settings.onClick.AddListener(() => SettingsPanel.gameObject.SetActive(true));
        ExitSettingsPanel.onClick.AddListener(() => SettingsPanel.gameObject.SetActive(false));
        ApplyQualityButton.onClick.AddListener(() => ApplyQualitySetting());
    }

    private void InitComponents() {
        Volume = SettingsPanel.transform.Find("Volume").gameObject;
        MouseSensitivity = SettingsPanel.transform.Find("MouseSensitivity").gameObject;
        QualitySetting = SettingsPanel.transform.Find("QualitySetting").gameObject;
        FPS = SettingsPanel.transform.Find("FPS").gameObject;
        ExitSettingsPanel = SettingsPanel.transform.Find("Exit").GetComponent<Button>();

        ApplyQualityButton = QualitySetting.transform.Find("Apply").GetComponent<Button>();

        Volume_scrollbar = Volume.GetComponentInChildren<Scrollbar>();
        MS_scrollbar = MouseSensitivity.GetComponentInChildren<Scrollbar>();
        QS_dropdown = QualitySetting.GetComponentInChildren<Dropdown>();
        FPS_dropdown = FPS.GetComponentInChildren<Dropdown>();

        vol_text = Volume.transform.Find("DisplayValue").GetComponent<Text>();
        ms_text = MouseSensitivity.transform.Find("DisplayValue").GetComponent<Text>();

        Volume_scrollbar.onValueChanged.AddListener(SetVolume);
        MS_scrollbar.onValueChanged.AddListener(SetMouseSensitivity);
        FPS_dropdown.onValueChanged.AddListener(SetFPS);
    }

    private void UpdateValues() {
        GetQualityLevel();

        vol_text.text = UserData.generalVolume.ToString();
        Volume_scrollbar.value = UserData.generalVolume;

        ms_text.text = (UserData.cameraRotationSpeed / 10f).ToString();
        MS_scrollbar.value = (UserData.cameraRotationSpeed / 10f);

        for(int i = 0; i < FPS_dropdown.options.Count; i++) {
            if(int.Parse(FPS_dropdown.options[i].text) == UserData.preferredFrameRate) {
                FPS_dropdown.value = i;
                break;
            }
        }
    }

    private void GetQualityLevel() {
        string quality = QualitySettings.names[QualitySettings.GetQualityLevel()];
        int i = 0;

        while (i < QS_dropdown.options.Count) {
            if (QS_dropdown.options[i].text == quality) {
                QS_dropdown.value = i;
                break;
            }

            i++;
        }
    }

    public void EnableEscPanel() {
        bool b = !OptionsPanel.activeSelf;

        gm.SetBlockInput(BlockInputSet.Elements.OnEscPanel, b);
        OptionsPanel.SetActive(b);

        if (!b)SettingsPanel.gameObject.SetActive(false);        
    }

    public void SetVolume(float v) {
        vol_text.text = v.ToString();
        AudioListener.volume = v;
        UserData.generalVolume = v;
    }

    public void SetMouseSensitivity(float s) {
        ms_text.text = s.ToString();
        UserData.cameraRotationSpeed = s * 10;
    }

    public void SetFPS(int select) {
        int fps = int.Parse(FPS_dropdown.options[select].text);
        Application.targetFrameRate = fps;
        UserData.preferredFrameRate = fps;
    }

    public void ApplyQualitySetting() {
        string[] names = QualitySettings.names;
        int i = 0;

        while(i < names.Length) {
            if(QS_dropdown.captionText.text == names[i]) {
                QualitySettings.SetQualityLevel(i , true);
                Debug.Log("Set Quality : "+ QS_dropdown.captionText.text);
                break;
            }

            i++;
        }
    }
}