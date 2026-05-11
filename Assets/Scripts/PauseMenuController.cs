using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    public static PauseMenuController Instance { get; private set; }

    GameObject _pausePanel, _settingsPanel;
    Slider _musicSlider, _sfxSlider;
    bool _paused;

    readonly List<Button> _buttons = new List<Button>();
    int _selectedIndex;

    void Awake() => Instance = this;

    void Start()
    {
        _pausePanel    = transform.Find("PausePanel")?.gameObject;
        _settingsPanel = transform.Find("PauseSettingsPanel")?.gameObject;
        _pausePanel?.SetActive(false);
        _settingsPanel?.SetActive(false);

        WireBtn(_pausePanel,    "ResumeButton",   Resume);
        WireBtn(_pausePanel,    "SettingsButton", ShowSettings);
        WireBtn(_pausePanel,    "MenuButton",     GoToMenu);
        WireBtn(_settingsPanel, "BackButton",     BackFromSettings);

        _musicSlider = FindDeep<Slider>(_settingsPanel, "MusicSlider");
        _sfxSlider   = FindDeep<Slider>(_settingsPanel, "SFXSlider");
        if (_musicSlider) { _musicSlider.value = AudioManager.MusicVolume; _musicSlider.onValueChanged.AddListener(AudioManager.SetMusic); }
        if (_sfxSlider)   { _sfxSlider.value   = AudioManager.SFXVolume;   _sfxSlider.onValueChanged.AddListener(AudioManager.SetSFX); }
    }

    void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.escapeKey.wasPressedThisFrame)
        {
            var gm = GameManager.Instance;
            if (gm != null && (gm.Stage == QuestStage.Cutscene || gm.Stage == QuestStage.Victory)) return;
            if (CodeInputPanel.Instance != null && CodeInputPanel.Instance.gameObject.activeSelf) return;
            if (_paused) Resume(); else Pause();
            return;
        }

        if (!_paused || _buttons.Count == 0) return;

        if (kb.downArrowKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame)
            Move(+1);
        else if (kb.upArrowKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame)
            Move(-1);

    }

    void Move(int dir)
    {
        _selectedIndex = (_selectedIndex + dir + _buttons.Count) % _buttons.Count;
        SelectCurrent();
    }

    public void Pause()
    {
        if (_paused) return;
        _paused = true;
        _pausePanel?.SetActive(true);
        _settingsPanel?.SetActive(false);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        var fpc = FindAnyObjectByType<FirstPersonController>();
        if (fpc) fpc.enabled = false;
        var pi = FindAnyObjectByType<PlayerInteraction>();
        if (pi) pi.enabled = false;
        RebuildButtons(_pausePanel);
    }

    public void Resume()
    {
        if (!_paused) return;
        _paused = false;
        _pausePanel?.SetActive(false);
        _settingsPanel?.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        var fpc = FindAnyObjectByType<FirstPersonController>(FindObjectsInactive.Include);
        if (fpc) fpc.enabled = true;
        var pi = FindAnyObjectByType<PlayerInteraction>(FindObjectsInactive.Include);
        if (pi) pi.enabled = true;
        _buttons.Clear();
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    void ShowSettings()
    {
        _pausePanel?.SetActive(false);
        _settingsPanel?.SetActive(true);
        RebuildButtons(_settingsPanel);
    }

    void BackFromSettings()
    {
        _pausePanel?.SetActive(true);
        _settingsPanel?.SetActive(false);
        RebuildButtons(_pausePanel);
    }

    void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
