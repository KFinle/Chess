using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AudioManager : MonoBehaviour
{
    private List<string> _keyStrokeHistory;
    public AudioSource audioSource;
     public AudioClip[] myMusic;

     public AudioClip[] easterEggClip;
 
    void Awake()
    {
        _keyStrokeHistory = new List<string>();
    }
     void Start()
     {
        playRandomMusic();
     }
 
     void LateUpdate()
     {
         if (!audioSource.isPlaying)
         {
            playRandomMusic();
         }

        KeyCode keyPressed = DetectKeyPressed();
        AddKeyStrokeToHistory(keyPressed.ToString());
        if (keyPressed != KeyCode.None) Debug.Log(keyPressed);

        if(GetKeyStrokeHistory().Equals("UpArrow,UpArrow,DownArrow,DownArrow,LeftArrow,RightArrow,LeftArrow,RightArrow,B,A")) 
        {
            PlayEasterEgg();
            ClearKeyStrokeHistory();
        }
     }
 
     void playRandomMusic()
     {
        audioSource.clip = myMusic[UnityEngine.Random.Range(0, myMusic.Length)] as AudioClip;
        audioSource.Play();
     }

    private KeyCode DetectKeyPressed() 
    {
        foreach(KeyCode key in Enum.GetValues(typeof(KeyCode))) 
        {
            if(Input.GetKeyDown(key)) {
                return key;
            }
        }
        return KeyCode.None;
    }

        private void AddKeyStrokeToHistory(string keyStroke) {
        if(!keyStroke.Equals("None")) {
            _keyStrokeHistory.Add(keyStroke);
            if(_keyStrokeHistory.Count > 10) {
                _keyStrokeHistory.RemoveAt(0);
            }
        }
    }

    private string GetKeyStrokeHistory() 
    {
        return String.Join(",", _keyStrokeHistory.ToArray());
    }

    private void ClearKeyStrokeHistory() 
    {
        _keyStrokeHistory.Clear();
    }

    public void PlayEasterEgg()
    {
        audioSource.Stop();
        audioSource.clip = easterEggClip[UnityEngine.Random.Range(0, easterEggClip.Length)] as AudioClip;
        audioSource.Play();
    }
}