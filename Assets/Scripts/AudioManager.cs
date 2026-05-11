using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    const string KeyMusic = "MusicVolume";
    const string KeySFX   = "SFXVolume";

    public static float MusicVolume => PlayerPrefs.GetFloat(KeyMusic, 1f);
    public static float SFXVolume   => PlayerPrefs.GetFloat(KeySFX,   1f);

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        ApplyAll();
    }

    public static void SetMusic(float v)
    {
        v = Mathf.Clamp01(v);
        PlayerPrefs.SetFloat(KeyMusic, v);
        AudioListener.volume = v;
    }

    public static void SetSFX(float v)
    {
        v = Mathf.Clamp01(v);
        PlayerPrefs.SetFloat(KeySFX, v);
    }

    static void ApplyAll()
    {
        AudioListener.volume = MusicVolume;
    }
}
