using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    const string KeyMusic = "MusicVolume";
    const string KeySFX   = "SFXVolume";

    public static float MusicVolume => PlayerPrefs.GetFloat(KeyMusic, 1f);
    public static float SFXVolume   => PlayerPrefs.GetFloat(KeySFX,   1f);

    [Header("SFX Clips")]
    public AudioClip bottleClip;
    public AudioClip flameClip;
    public AudioClip mainDoorClip;
    public AudioClip menuThickClip;
    public AudioClip safeDoorClip;
    public AudioClip takeThinClip;
    public AudioClip throwClip;

    [Header("Music / Radio Clips")]
    public AudioClip standUpClip;
    public AudioClip phoneMusicClip;
    public AudioClip goodJobClip;
    public AudioClip footstepsClip;

    AudioSource _musicSrc;   // phone_music (loop)
    AudioSource _radioSrc;   // stand_up / good_job (one-shot radio)
    AudioSource _sfxSrc;     // SFX one-shots
    AudioSource _stepsSrc;   // footsteps (loop)

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _musicSrc = MakeSource(loop: true,  vol: MusicVolume);
        _radioSrc = MakeSource(loop: false, vol: MusicVolume);
        _sfxSrc   = MakeSource(loop: false, vol: SFXVolume);
        _stepsSrc = MakeSource(loop: true,  vol: SFXVolume * 0.35f);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        StopAllCoroutines();
        _musicSrc.Stop();
        _radioSrc.Stop();
        _stepsSrc.Stop();
    }

    AudioSource MakeSource(bool loop, float vol)
    {
        var src = gameObject.AddComponent<AudioSource>();
        src.loop = loop;
        src.playOnAwake = false;
        src.volume = vol;
        return src;
    }

    // ── Volume settings ────────────────────────────────────────
    public static void SetMusic(float v)
    {
        v = Mathf.Clamp01(v);
        PlayerPrefs.SetFloat(KeyMusic, v);
        if (Instance == null) return;
        Instance._musicSrc.volume = v;
        Instance._radioSrc.volume = v;
    }

    public static void SetSFX(float v)
    {
        v = Mathf.Clamp01(v);
        PlayerPrefs.SetFloat(KeySFX, v);
        if (Instance == null) return;
        Instance._sfxSrc.volume   = v;
        Instance._stepsSrc.volume = v * 0.35f;
    }

    // ── SFX one-shots ──────────────────────────────────────────
    static void PlayClip(AudioClip clip)
    {
        if (Instance == null || clip == null) return;
        Instance._sfxSrc.PlayOneShot(clip);
    }

    public static void PlayBottle()   => PlayClip(Instance?.bottleClip);
    public static void PlayFlame()    => PlayClip(Instance?.flameClip);
    public static void PlayMainDoor() => PlayClip(Instance?.mainDoorClip);
    public static void PlaySafeDoor() => PlayClip(Instance?.safeDoorClip);
    public static void PlayTake()     => PlayClip(Instance?.takeThinClip);
    public static void PlayThrow()    => PlayClip(Instance?.throwClip);

    public static void PlayMenuFocus()
    {
        if (Instance == null || Instance.menuThickClip == null) return;
        Instance._sfxSrc.PlayOneShot(Instance.menuThickClip, 0.55f);
    }

    // ── Footsteps ──────────────────────────────────────────────
    public static void SetFootsteps(bool active)
    {
        if (Instance == null || Instance.footstepsClip == null) return;
        if (active && !Instance._stepsSrc.isPlaying)
        {
            Instance._stepsSrc.clip = Instance.footstepsClip;
            Instance._stepsSrc.Play();
        }
        else if (!active && Instance._stepsSrc.isPlaying)
        {
            Instance._stepsSrc.Stop();
        }
    }

    // ── Cutscene radio ─────────────────────────────────────────
    public static void PlayStandUp()
    {
        if (Instance == null || Instance.standUpClip == null) return;
        Instance._radioSrc.Stop();
        Instance._radioSrc.clip   = Instance.standUpClip;
        Instance._radioSrc.volume = MusicVolume;
        Instance._radioSrc.Play();
    }

    // ── Background music ───────────────────────────────────────
    public static void StartPhoneMusic(float fadeIn = 3f)
    {
        if (Instance == null || Instance.phoneMusicClip == null) return;
        Instance.StartCoroutine(Instance.FadeInMusic(fadeIn));
    }

    IEnumerator FadeInMusic(float duration)
    {
        _musicSrc.clip   = phoneMusicClip;
        _musicSrc.volume = 0f;
        _musicSrc.Play();
        float target = MusicVolume;
        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            _musicSrc.volume = Mathf.Lerp(0f, target, t / duration);
            yield return null;
        }
        _musicSrc.volume = target;
    }

    // ── Victory ────────────────────────────────────────────────
    public static void PlayGoodJob()
    {
        if (Instance == null || Instance.goodJobClip == null) return;
        Instance.StartCoroutine(Instance.CrossfadeToGoodJob());
    }

    IEnumerator CrossfadeToGoodJob()
    {
        float start = _musicSrc.volume;
        for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime)
        {
            _musicSrc.volume = Mathf.Lerp(start, 0f, t);
            yield return null;
        }
        _musicSrc.Stop();

        _radioSrc.Stop();
        _radioSrc.loop   = false;
        _radioSrc.clip   = goodJobClip;
        _radioSrc.volume = MusicVolume;
        _radioSrc.Play();
    }
}
