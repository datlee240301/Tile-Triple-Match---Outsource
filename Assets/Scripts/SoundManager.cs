using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;
    public AudioSource audioSource;
    [SerializeField] AudioClip match3TileSound;
    [SerializeField] AudioClip click1Tile;
    [SerializeField] AudioClip uiClickSound;
    [SerializeField] AudioClip winSound;
    [SerializeField] AudioClip loseSound;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
    }
    public void PlayMatch3TileSound()
    {
        audioSource.PlayOneShot(match3TileSound);
    }
    public void PlayClick1Tile()
    {
        audioSource.PlayOneShot(click1Tile);
    }
    public void PlayUIClickSound()
    {
        audioSource.PlayOneShot(uiClickSound);
    }
    public void PlayWinSound()
    {
        audioSource.PlayOneShot(winSound);
    }

    public void PlayLoseSound()
    {
        audioSource.PlayOneShot(loseSound);
    }
}