using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    public HashSet<GameObject> objectsToHit = new HashSet<GameObject>(); // Use HashSet to prevent duplicates
    private float startTime;
    private bool hasExploded = false;
    public float duration;
    public float damage;
    public GameObject explosionVFXPrefab;
    private GameObject explosionVFX;

    public AudioSource audioSource;
    public AudioClip pinAudio;
    public AudioClip explosionAudio;

    private void Start()
    {
        if (!explosionVFXPrefab || !audioSource || !pinAudio || !explosionAudio)
        {
            Debug.LogError("Missing references in Grenade script.");
            return;
        }

        startTime = Time.time;
        audioSource.PlayOneShot(pinAudio);
    }

    private void Update()
    {
        if (!hasExploded && Time.time - startTime > duration)
        {
            Explode();
        }
    }

    private void Explode()
    {
        hasExploded = true;

        // Apply damage to all objects in range
        foreach (GameObject obj in objectsToHit)
        {
            if (obj)
            {
               

                if (obj.TryGetComponent<Player>(out Player player))
                {
                    player.TakeDamage(damage);
                }
                if (obj.TryGetComponent<Target>(out Target target))
                {
                    target.Hit(damage);
                    Debug.Log($"Damaging: {obj.name}");
                }
                else
                {
                    Debug.Log($"Hit something that is not a target: {obj.name}");
                }
            }
        }

        // Instantiate explosion effect
        explosionVFX = Instantiate(explosionVFXPrefab, transform.position, Quaternion.identity);
        audioSource.PlayOneShot(explosionAudio);

        // Hide grenade visuals
        foreach (MeshRenderer mesh in GetComponentsInChildren<MeshRenderer>())
        {
            mesh.enabled = false;
        }

        // Clear targets and destroy grenade after a delay
        objectsToHit.Clear();
        Destroy(gameObject, 5f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Target"))
        {
            objectsToHit.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Target"))
        {
            objectsToHit.Remove(other.gameObject);
        }
    }

    private void OnDestroy()
    {
        if (explosionVFX)
        {
            Destroy(explosionVFX);
        }
    }
}
