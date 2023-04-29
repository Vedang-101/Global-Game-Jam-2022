using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager instance = null;

    private string roomKey;

    public enum NetworkState
    {
        disconnected,
        connectedToServer,
        createdRoom,
        joiningRoom,
        joinedRoom
    };

    private void Awake()
    {
        instance = this;
    }

    public NetworkState currentState;

    public GameObject roomManager;

    private void Start()
    {
        if(RoomManager.Instance== null)
            Instantiate(roomManager, Vector3.zero, Quaternion.identity);
    }

    public void NetworkBack()
    {
        if (currentState == NetworkState.disconnected)
            return;
        else if(currentState == NetworkState.connectedToServer)
        {
            MainMenuUIManager.instance.ShowLoading();
            PhotonNetwork.Disconnect();
        }
        else if (currentState == NetworkState.createdRoom)
        {
            MainMenuUIManager.instance.ShowLoading();
            PhotonNetwork.LeaveRoom();
        } else if(currentState == NetworkState.joiningRoom)
        {
            currentState = NetworkState.connectedToServer;
        } else if (currentState == NetworkState.joinedRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        MainMenuUIManager.instance.HideLoading();
        currentState = NetworkState.disconnected;
    }

    public override void OnLeftRoom()
    {
        MainMenuUIManager.instance.HideLoading();
        if (currentState == NetworkState.createdRoom)
        {
            currentState = NetworkState.connectedToServer;
        }
        else if (currentState == NetworkState.joinedRoom)
        {
            currentState = NetworkState.joiningRoom;
            MainMenuUIManager.instance.ShowJoinRoomError();
            MainMenuUIManager.instance.UpdateJoinRoom(false);
        }
    }

    public void ConnectToServer()
    {
        MainMenuUIManager.instance.ShowLoading();
        if (currentState == NetworkState.disconnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if(PhotonNetwork.LocalPlayer == newMasterClient) {
            PhotonNetwork.LeaveRoom();
        }
    }

    public override void OnJoinedLobby()
    {
        if(currentState == NetworkState.disconnected)
            currentState = NetworkState.connectedToServer;
    
        MainMenuUIManager.instance.HideLoading();
        MainMenuUIManager.instance.ShowMuliplayer();
    }

    public void CreateNewRoom()
    {
        MainMenuUIManager.instance.ShowLoading();
        roomKey = "";
        for (int i = 0; i < 6; i++)
            roomKey += (char)(Random.Range(0, 25) + 'A');
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 2;
        PhotonNetwork.CreateRoom(roomKey, roomOptions);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        MainMenuUIManager.instance.UpdateCreateRoom(true);
    }

    public override void OnJoinedRoom()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            currentState = NetworkState.createdRoom;
            MainMenuUIManager.instance.HideLoading();
            MainMenuUIManager.instance.ShowCreateRoom(roomKey);
        } else
        {
            currentState = NetworkState.joinedRoom;
            MainMenuUIManager.instance.UpdateJoinRoom(true);
        }
    }

    public void JoinRoom(string roomName, string PlayerName)
    {
        MainMenuUIManager.instance.ShowLoading();
        PhotonNetwork.LocalPlayer.NickName = PlayerName;
        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        MainMenuUIManager.instance.HideLoading();
        MainMenuUIManager.instance.ShowJoinRoomError();
        Debug.Log("Failed to join room: " + message + "," + returnCode);
    }

    public void StartGame(string Player1Name)
    {
        PhotonNetwork.LocalPlayer.NickName = Player1Name;
        PhotonNetwork.LoadLevel(1);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        MainMenuUIManager.instance.UpdateCreateRoom(false);
    }
}