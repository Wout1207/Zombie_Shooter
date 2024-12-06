using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    private List<GameObject> objectsToHit = new List<GameObject>();
    private float time;
    private bool explode = false;
    public float duration;
    public float damage;
    public GameObject explosionVFXPrefab;
    private GameObject explosionVFX;

    public AudioSource audioSource;
    public AudioClip pinAudio;
    public AudioClip explosionAudio;
    // Start is called before the first frame update
    private void Start()
    {
        time = Time.time;
        audioSource.clip = pinAudio;
        audioSource.Play();
    }

    private void Update()
    {
        if (!explode && Time.time - time > duration)
        {
            foreach (GameObject obj in objectsToHit)
            {
                if (obj)
                {
                    Player player = obj.GetComponent<Player>();
                    if (player)
                    {
                        player.TakeDamage(damage);
                    }
                    Target target = obj.GetComponent<Target>();
                    if (target)
                    {
                        target.Hit(damage);
                    }
                }
            }
            explosionVFX = Instantiate(explosionVFXPrefab);
            explosionVFX.transform.position = transform.position;
            explode = true;
            audioSource.clip = explosionAudio;
            audioSource.Play();
            foreach (MeshRenderer mesh in GetComponentsInChildren<MeshRenderer>())
            {
                mesh.enabled = false;
            }
        }
        if (Time.time - time > duration+20)
        {
            Destroy(gameObject);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals("Player") || other.gameObject.tag.Equals("Target"))
        {
            if(!objectsToHit.Contains(other.gameObject))
            {
                objectsToHit.Add(other.gameObject);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name.Equals("Player") || other.gameObject.name.Contains("Target"))
        {
            if (objectsToHit.Contains(other.gameObject))
            {
                objectsToHit.Remove(other.gameObject);
            }
        }
    }

    private void OnDestroy()
    {
        Destroy(explosionVFX);
    }
}
