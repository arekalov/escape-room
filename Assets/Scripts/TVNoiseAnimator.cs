using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class TVNoiseAnimator : MonoBehaviour
{
    public TVController tvController;
    public int screenMaterialIndex = 2;

    [Header("Noise settings")]
    public float updateInterval = 0.04f;   // ~25 fps noise update
    public float minBrightness = 0.4f;
    public float maxBrightness = 1.0f;

    private MeshRenderer _renderer;
    private Material _noiseMat;           // instance material (not shared)
    private float _timer;
    private bool _initialized;

    void Awake()
    {
        _renderer = GetComponent<MeshRenderer>();
        if (tvController == null)
            tvController = GetComponent<TVController>();
    }

    void Update()
    {
        if (tvController == null || tvController.State != TVController.TVState.Noise)
        {
            // Release instance when not in noise state
            if (_initialized) { _initialized = false; _noiseMat = null; }
            return;
        }

        // Grab instance material once (creates a copy so we don't dirty the asset)
        if (!_initialized)
        {
            var mats = _renderer.materials;       // .materials gives instances
            _noiseMat = mats[screenMaterialIndex];
            _initialized = true;
        }

        _timer += Time.deltaTime;
        if (_timer < updateInterval) return;
        _timer = 0f;

        // Random UV scroll — makes noise pattern jump around
        var offset = new Vector2(Random.value, Random.value);
        _noiseMat.SetTextureOffset("_BaseMap", offset);
        _noiseMat.SetTextureOffset("_EmissionMap", offset);

        // Random brightness flicker
        float b = Random.Range(minBrightness, maxBrightness);
        _noiseMat.SetColor("_EmissionColor", new Color(b, b, b));

        // Occasional tiling stretch — simulates vertical hold loss
        if (Random.value < 0.05f)
        {
            float tileY = Random.Range(0.8f, 1.4f);
            _noiseMat.SetTextureScale("_BaseMap", new Vector2(1f, tileY));
            _noiseMat.SetTextureScale("_EmissionMap", new Vector2(1f, tileY));
        }
    }

    void OnDisable()
    {
        _initialized = false;
        _noiseMat = null;
    }
}
