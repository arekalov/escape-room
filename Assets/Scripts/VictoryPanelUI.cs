using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VictoryPanelUI : MonoBehaviour
{
    bool _wired;

    void OnEnable()
    {
        if (_wired) return;
        _wired = true;

        foreach (var btn in GetComponentsInChildren<Button>(true))
        {
            if (btn.name == "MenuButton")
            {
                btn.onClick.AddListener(() => {
                    Time.timeScale = 1f;
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                });
            }
        }
    }
}
