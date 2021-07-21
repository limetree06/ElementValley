using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using Photon.Pun;

namespace RW.MonumentValley
{

    // monitors win condition and stops/pauses gameplay as needed
    public class LoadingManager : MonoBehaviourPunCallbacks{

        // delay before restarting, etc.
        public float delayTime;

        // invoked on awake
        public UnityEvent awakeEvent;

        // invoked when starting the level
        public UnityEvent initEvent;

        // invoked before ending the level
        public UnityEvent endLevelEvent;

        private PlayerMover playerMover;

        public SoundManager soundManager;
        public AudioSource[] audioBackGround;


        private void Awake(){
            awakeEvent.Invoke();
        }

        // invoke any events at the start of gameplay
        private void Start(){
            PhotonNetwork.ConnectUsingSettings();
            playerMover = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMover>();
            soundManager = FindObjectOfType<SoundManager>();
            audioBackGround = soundManager.transform.Find("BackGroundSound").GetComponents<AudioSource>();
            audioBackGround[0].volume = 0.5f;
            audioBackGround[1].volume = 0.5f;
            
            initEvent.Invoke();
        }
        public override void OnConnectedToMaster(){
            PhotonNetwork.JoinLobby();
            playerMover.OnStartMoving();
        }

        public override void OnJoinedLobby(){
            StartCoroutine(LoadRoutine());
        }

        // invoke end level event and wait
        private IEnumerator LoadRoutine()
        {
            yield return new WaitForSeconds(delayTime);
            if (endLevelEvent != null)
                endLevelEvent.Invoke();

            // yield Animation time
            yield return new WaitForSeconds(delayTime);
        }

        // start the scene
        public void GameStart(float delay)
        {
            StartCoroutine(StartRoutine(delay));
        }

        // wait for a delay and start the scene
        private IEnumerator StartRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            SceneManager.LoadScene("LobbyScene");
        }
    

        // check for win condition every frame
        private void Update()
        {
        
        }

    }
}