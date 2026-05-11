using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryPanelUI : MonoBehaviour
{
    void OnEnable()
    {
        StartCoroutine(ReturnToMenu());
    }

    IEnumerator ReturnToMenu()
    {
        yield return new WaitForSecondsRealtime(5f);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
