using System.Collections;
using UnityEngine;

public class BookBurnable : MonoBehaviour, IInteractable
{
    [Header("Required item")]
    public ItemData lighterItem;

    [Header("Key to reveal after burn")]
    public GameObject keyObject;

    [Header("Burn effect prefab (looping fire — stopped when book is gone)")]
    public GameObject burnParticlePrefab;

    bool _burned;

    public void Interact(ItemData usedItem)
    {
        if (_burned) return;
        if (usedItem == null || usedItem != lighterItem) return;

        _burned = true;
        InventoryManager.Instance?.RemoveItem(lighterItem);

        if (keyObject != null)
        {
            keyObject.transform.position = transform.position;
            keyObject.SetActive(true);
        }

        StartCoroutine(BurnAnimation());
    }

    public string GetHintText(ItemData usedItem)
    {
        if (_burned) return "";
        return (usedItem != null && usedItem == lighterItem)
            ? "[F] Поджечь зажигалкой"
            : "";
    }

    IEnumerator BurnAnimation()
    {
        Vector3 origScale = transform.localScale;
        Vector3 origPos   = transform.localPosition;

        // Spawn fire — track the instance so we can stop it
        GameObject fireInstance = null;
        if (burnParticlePrefab != null)
            fireInstance = Instantiate(burnParticlePrefab, transform.position + Vector3.up * 0.05f, Quaternion.identity);

        // Brief flare — scale up slightly as it catches fire
        for (float t = 0f; t < 0.15f; t += Time.deltaTime)
        {
            float p = t / 0.15f;
            transform.localScale = origScale * (1f + 0.08f * Mathf.Sin(p * Mathf.PI));
            yield return null;
        }

        // Burn down — shrink and float upward
        for (float t = 0f; t < 0.7f; t += Time.deltaTime)
        {
            float p    = t / 0.7f;
            float ease = p * p;
            transform.localScale    = origScale * (1f - ease);
            transform.localPosition = origPos + Vector3.up * p * 0.25f;
            yield return null;
        }

        // Stop fire emitters so existing particles finish naturally, then destroy
        if (fireInstance != null)
        {
            foreach (var ps in fireInstance.GetComponentsInChildren<ParticleSystem>())
            {
                var em = ps.emission;
                em.enabled = false;
            }
            Destroy(fireInstance, 1.5f);
        }

        gameObject.SetActive(false);
    }
}
