using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIAudio : MonoBehaviour
{
    [SerializeField] AudioClip hover, click;
    AudioSource audioSource;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void SoundOnHover()
    {
        audioSource.PlayOneShot(hover);
    }

    public void SoundOnClick()
    {
        audioSource.PlayOneShot(click);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
