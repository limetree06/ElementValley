using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    // Start is called before the first frame update

    public AudioSource audioBackGroundSource;
    public AudioSource audioEffectSource;
    public AudioClip[] audioClips;


    public void audioClipPlay(int index){
        audioEffectSource.PlayOneShot(audioClips[index]);
    }

}
