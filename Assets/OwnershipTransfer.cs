using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;


public class OwnershipTransfer : MonoBehaviourPunCallbacks{
    void Start() {
        
        
        
    }
    void Update(){
        if (Input.GetKeyDown(KeyCode.Mouse0)){
            this.photonView.RequestOwnership();
            }        
    }

    
}
