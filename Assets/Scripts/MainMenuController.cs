using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    GameObject _mainPanel, _settingsPanel;
    Slider _musicSlider, _sfxSlider;

    readonly List<Button> _buttons = new List<Button>();
    int _selectedIndex;

    void Awake()
    {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Start()
    {
        _mainPanel     = transform.Find("MainMenuPanel")?.gameObject;
        _settingsPanel = transform.Find("SettingsPanel")?.gameObject;

        WireBtn(_mainPanel,     "StartButton",    StartGame);
        WireBtn(_mainPanel,     "SettingsButton", ShowSettings);
        WireBtn(_settingsPanel, "BackButton",     ShowMain);

        _musicSlider = FindDeep<Slider>(_settingsPanel, "MusicSlider");
        _sfxSlider   = FindDeep<Slider>(_settingsPanel, "SFXSlider");
        if (_musicSlider) { _musicSlider.value = AudioManager.MusicVolume; _musicSlider.onValueChanged.AddListener(AudioManager.SetMusic); }
        if (_sfxSlider)   { _sfxSlider.value   = AudioManager.SFXVolume;   _sfxSlider.onValueChanged.AddListener(AudioManager.SetSFX); }

        ShowMain();
    }

    void ShowMain()
    {
        _mainPanel?.SetActive(true);
        _settingsPanel?.SetActive(false);
        RebuildButtons(_mainPanel);
    }

    void ShowSettings()
    {
        _mainPanel?.SetActive(false);
        _settingsPanel?.SetActive(true);
        RebuildButtons(_settingsPanel);
    }

    void RebuildButtons(GameObject panel)
    {
        _buttons.Clear();
        if (panel == null) return;
        foreach (var btn in panel.GetComponentsInChildren<Button>(true))
            _buttons.Add(btn);
        _selectedIndex = 0;
        SelectCurrent();
    }

    void SelectCurrent()
    {
        if (_buttons.Count == 0 || EventSystem.current == null) return;
        EventSystem.current.SetSelectedGameObject(_buttons[_selectedIndex].gameObject);
    }

    void StartGame()
    {
        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }

    static void WireBtn(GameObject parent, string name, UnityEngine.Events.UnityAction action)
    {
        var btn = FindDeep<Button>(parent, name);
        btn?.onClick.AddListener(action);
    }

    static T FindDeep<T>(GameObject root, string name) where T : Component
    {
        if (root == null) return null;
        foreach (var c in root.GetComponentsInChildren<T>(true))
            if (c.name == name) return c;
        return null;
    }
}
