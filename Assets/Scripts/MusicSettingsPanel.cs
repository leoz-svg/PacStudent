using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class MusicSettingsPanel : MonoBehaviour
{
    public GameObject overlay;
    public UnityEngine.UI.Button openButton, closeButton;
    public UnityEngine.UI.Button[] levelButtons;
    public UnityEngine.UI.Toggle musicToggle;
    public UnityEngine.UI.Slider volumeSlider;
    public TextMeshProUGUI volumeLabel, stateLabel;

    void Awake()
    {
        openButton.onClick.AddListener(Open);
        closeButton.onClick.AddListener(Close);
        musicToggle.onValueChanged.AddListener(SetEnabled);
        volumeSlider.onValueChanged.AddListener(SetVolume);
        Refresh();
        overlay.SetActive(false);
    }
    void OnEnable() { MusicSettings.Changed += Refresh; }
    void OnDisable() { MusicSettings.Changed -= Refresh; MusicSettings.Save(); }
    void Update() { if (overlay.activeSelf && Input.GetKeyDown(KeyCode.Escape)) Close(); }
    public void Open()
    {
        Refresh();
        overlay.SetActive(true);
        openButton.interactable = false;
        foreach (var button in levelButtons) button.interactable = false;
        overlay.transform.SetAsLastSibling();
        if (EventSystem.current) EventSystem.current.SetSelectedGameObject(musicToggle.gameObject);
    }
    public void Close()
    {
        MusicSettings.Save();
        overlay.SetActive(false);
        openButton.interactable = true;
        foreach (var button in levelButtons) button.interactable = true;
        if (EventSystem.current) EventSystem.current.SetSelectedGameObject(openButton.gameObject);
    }
    void SetEnabled(bool value) { MusicSettings.SetEnabled(value); }
    void SetVolume(float value) { MusicSettings.SetVolume(value); }
    void Refresh()
    {
        musicToggle.SetIsOnWithoutNotify(MusicSettings.Enabled);
        volumeSlider.SetValueWithoutNotify(MusicSettings.Volume);
        volumeLabel.text = Mathf.RoundToInt(MusicSettings.Volume * 100) + "%";
        stateLabel.text = MusicSettings.Enabled ? "ON" : "OFF";
    }
    void OnDestroy()
    {
        if (openButton) openButton.onClick.RemoveListener(Open);
        if (closeButton) closeButton.onClick.RemoveListener(Close);
        if (musicToggle) musicToggle.onValueChanged.RemoveListener(SetEnabled);
        if (volumeSlider) volumeSlider.onValueChanged.RemoveListener(SetVolume);
    }
}
