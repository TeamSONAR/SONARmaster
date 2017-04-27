using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class soundOnCollision : MonoBehaviour
{
    
    public AudioClip jumpSound;
    public AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }



    void OnTriggerEnter(Collider other)
    {
        audioSource.clip = jumpSound;
        audioSource.Play();

    }
}
