/*
 * Copyright (c) 2020 Razeware LLC
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * Notwithstanding the foregoing, you may not use, copy, modify, merge, publish, 
 * distribute, sublicense, create a derivative work, and/or sell copies of the 
 * Software in any work that is designed, intended, or marketed for pedagogical or 
 * instructional purposes related to programming, coding, application development, 
 * or information technology.  Permission for such use, copying, modification,
 * merger, publication, distribution, sublicensing, creation of derivative works, 
 * or sale is expressly withheld.
 *    
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace RW.MonumentValley
{

    // monitors win condition and stops/pauses gameplay as needed
    public class GameManager2 : MonoBehaviour
    {
        // reference to player's controller component
        private PlayerController2 playerController;

        private GameObject player;
        public GameObject playerPrefab1;
        public GameObject playerPrefab2;

        private GameObject spinnerControl1;
        private GameObject spinnerControl2;

        // have we completed the win condition (i.e. reached the goal)?
        private bool isGameOver;
        public bool IsGameOver => isGameOver;

        // delay before restarting, etc.
        public float delayTime = 2f;

        // invoked on awake
        public UnityEvent awakeEvent;

        // invoked when starting the level
        public UnityEvent initEvent;

        // invoked before ending the level
        public UnityEvent endLevelEvent;

        // public UnityEvent updateLinkEvent;

        public bool anotherplayerGoal2 = false;

        private bool controllerflag = false;

        private PhotonView view;

        int mode;

        public SoundManager soundManager;
        public AudioSource[] audioBackGround;

        private void Awake()
        {
            awakeEvent.Invoke();

            spinnerControl1 = GameObject.Find("SpinnerControl1");
            spinnerControl2 = GameObject.Find("SpinnerControl2");

            if (playerPrefab1 == null || playerPrefab2 == null ) {
                Debug.LogError("<Color=Red><a>Missing</a></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'",this);
            } 
            
            else {
                Invoke("Instantiateplayers", 2.0f);
            }

            soundManager = FindObjectOfType<SoundManager>();
            audioBackGround = soundManager.transform.Find("BackGroundSound").GetComponents<AudioSource>();

            // audioBackGround[0].Stop();
            // audioBackGround[1].Stop();
            // audioBackGround[0].clip = soundManager.audioClips[8];
            // audioBackGround[0].Play();
            // audioBackGround[0].loop = true;
            // audioBackGround[0].volume = 0.3f;

        }

        // invoke any events at the start of gameplay
        private void Start()
        {           
            initEvent.Invoke();
        }

        private void Instantiateplayers(){
            if(PhotonNetwork.IsMasterClient){
                player = PhotonNetwork.Instantiate(this.playerPrefab1.name, new Vector3(-21f,8.5f,14f), Quaternion.identity, 0);
                playerController = player.GetComponent<PlayerController2>(); 
                spinnerControl1.transform.localScale = Vector3.zero;
                mode = 1;
            }

            else{
                player = PhotonNetwork.Instantiate(this.playerPrefab2.name, new Vector3(-9f,0.5f,0f), Quaternion.identity, 0);
                playerController = player.GetComponent<PlayerController2>(); 
                spinnerControl2.transform.localScale = Vector3.zero;
                mode = 2;
            }  
        }


        // check for win condition every frame
        private void Update(){

            if(playerController == null){
                playerController = player.GetComponent<PlayerController2>(); 
            }
            
            if (playerController!= null && playerController.HasReachedGoal(mode)){
                this.gameObject.GetComponent<PhotonView>().RPC("setAnotherPlayerGoal2", RpcTarget.Others);
                // disable player controls
                // playerController.EnableControls(false);
            }

            if(playerController!= null && playerController.HasReachedGoal(mode) && anotherplayerGoal2){
                 Win();
            }

        }
        
        [PunRPC]
        void setAnotherPlayerGoal2(){ 
            anotherplayerGoal2 = true;
        }
        
        public void audioClipPlay(int index){
            soundManager.audioClipPlay(index);
        }

        // win and end the level
        private void Win()
        { 
            // flag to ensure Win only triggers once
            if (isGameOver || (playerController == null))
            {
                return;
            }
            isGameOver = true;

            // play win animation
            StartCoroutine(WinRoutine());
        }

        // invoke end level event and wait
        private IEnumerator WinRoutine()
        {
            if (endLevelEvent != null)
                endLevelEvent.Invoke();

            // yield Animation time
            yield return new WaitForSeconds(delayTime);
        }

        // restart the scene
        public void Restart(float delay)
        {
            audioBackGround[0].Stop();
            StartCoroutine(RestartRoutine(delay));
        }


        // wait for a delay and restart the scene
        private IEnumerator RestartRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);

            SceneManager.LoadScene("LobbyScene");
            // SceneManager.LoadScene("LoadingScene2");
        }
    }
}
