using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Photon.Realtime;

public class PlayerManager : MonoBehaviour
{
    PhotonView PV;

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
    }

    private void Start()
    {
        if (PV.IsMine)
            CreatePlayer();
    }

    void CreatePlayer()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GameObject player = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player_1"), InstantiateManager.instance.Player1Spawn.position, InstantiateManager.instance.Player1Spawn.rotation);
            NetworkedGameManager.instance.playerObject = player.GetComponent<NetworkedPlayerController>();
        }
        else
        {
            GameObject player = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player_2"), InstantiateManager.instance.Player2Spawn.position, InstantiateManager.instance.Player2Spawn.rotation);
            NetworkedGameManager.instance.playerObject = player.GetComponent<NetworkedPlayerController>();
        }
    }
}
