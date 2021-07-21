using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;


public class CreateAndJoinRooms : MonoBehaviourPunCallbacks
{

    public InputField createInput;
    public InputField joinInput;
    RoomOptions roomOptions = new RoomOptions();
    
    public void CreateRoom(){
        roomOptions.MaxPlayers = 2;
        PhotonNetwork.CreateRoom("create", roomOptions);
    }

    public void JoinRoom(){
        PhotonNetwork.JoinRoom("create");
    }

    public override void OnJoinedRoom(){
        PhotonNetwork.LoadLevel("SuhyeonLevel");
    }

}