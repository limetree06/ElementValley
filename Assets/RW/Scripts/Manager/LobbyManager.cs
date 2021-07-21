using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public InputField createInput;
    public InputField joinInput;
    RoomOptions roomOptions = new RoomOptions();

    public SoundManager soundManager;
    public AudioSource[] audioBackGround;

    public void Awake() {
        soundManager = FindObjectOfType<SoundManager>();
        audioBackGround = soundManager.transform.Find("BackGroundSound").GetComponents<AudioSource>();

        if(!audioBackGround[0].isPlaying){
            audioBackGround[0].clip = soundManager.audioClips[7];
            audioBackGround[1].clip = soundManager.audioClips[6];
            audioBackGround[0].Play();
            audioBackGround[1].Play();
            audioBackGround[0].loop = true;
            audioBackGround[1].loop = true;
            audioBackGround[0].volume = 0.5f;
            audioBackGround[1].volume = 0.5f;
        }
    }
    public void CreateRoom(){
        roomOptions.MaxPlayers = 2;
        PhotonNetwork.CreateRoom(createInput.text, roomOptions);
    }

    public void JoinRoom(){
        PhotonNetwork.JoinRoom(joinInput.text);
    }

    public override void OnJoinedRoom(){
        PhotonNetwork.LoadLevel("Level1Scene");
        // StartCoroutine(JoinRoutine(1));
    }

    public void audioClipPlay(int index){
        soundManager.audioClipPlay(index);
    }

    // wait for a delay and restart the scene
    private IEnumerator JoinRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        PhotonNetwork.LoadLevel("Level1Scene");
    }

}