using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSound : MonoBehaviour
{
    // Start is called before the first frame update

    public AudioSource audioSource;
    public AudioClip audioWalk;
    
    public void PlayWalkAudio(){
        print("walk");
        audioSource.PlayOneShot(audioWalk);
    } 
}
