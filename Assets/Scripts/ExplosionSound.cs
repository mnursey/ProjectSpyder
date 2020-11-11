using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionSound : MonoBehaviour
{
    public List<AudioClip> explosionClips;
    public AudioSource explosionSource;

    // Start is called before the first frame update
    void Start()
    {
        PlayExplosionAudio();
    }


    private void PlayExplosionAudio()
    {
        if (explosionSource != null)
        {
            int hitClipIndex = Mathf.FloorToInt(Random.Range(0, explosionClips.Count));
            explosionSource.clip = explosionClips[hitClipIndex];
            explosionSource.Play();
        }
        else
        {
            Debug.LogError("No hit source found! Assign in editor");
        }
    }
}
