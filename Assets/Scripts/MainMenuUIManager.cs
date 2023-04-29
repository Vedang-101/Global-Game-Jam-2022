using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUIManager : MonoBehaviour
{
    public static MainMenuUIManager instance = null;
    private void Awake()
    {
        instance = this;
    }

    [Header("Main Menu HUD")]
    public Animator MainMenuHUD;

    [Header("Loading Menu HUD")]
    public Animator LoadingMenu;

    [Header("Multiplayer Logs")]
    public Text[] CreateRoomKey;

    public Button Start_JoinRoom;
    public Text[] Start_JoinRoomText;

    public Button Start_CreateRoom;
    public Text[] Start_CreateRoomText;

    [Header("Join Room Info")]
    public InputField Player2Name;
    public InputField RoomID;

    [Header("Create Room Info")]
    public InputField Player1Name;

    public void ShowCredits()
    {
        SoundManager.instance.PlayPickUp(Vector3.zero);
        MainMenuHUD.SetTrigger("Credits");
    }

    public void ShowMainMenu()
    {
        SoundManager.instance.PlayPickUp(Vector3.zero);
        MainMenuHUD.ResetTrigger("Multiplayer");
        MainMenuHUD.SetTrigger("Back");
    }

    public void ShowNextTutorialCard()
    {
        SoundManager.instance.PlayPickUp(Vector3.zero);
        MainMenuHUD.SetTrigger("TutorialNext");
    }

    public void ShowMuliplayer()
    {
        SoundManager.instance.PlayPickUp(Vector3.zero);
        MainMenuHUD.SetTrigger("Multiplayer");
    }

    public void ShowJoinRoom()
    {
        SoundManager.instance.PlayPickUp(Vector3.zero);
        Start_JoinRoom.enabled = true;
        Start_JoinRoomText[0].text = "Enter";
        Start_JoinRoomText[1].text = "Enter";
        NetworkManager.instance.currentState = NetworkManager.NetworkState.joiningRoom;
        MainMenuHUD.SetTrigger("JoinRoom");
    }

    public void UpdateJoinRoom(bool enabled)
    {
        if (enabled)
        {
            SoundManager.instance.PlayPickUp(Vector3.zero);
            Start_JoinRoom.enabled = false;
            Start_JoinRoomText[0].text = "Waiting for Player 1";
            Start_JoinRoomText[1].text = "Waiting for Player 1";
        } else
        {
            Start_JoinRoom.enabled = true;
            Start_JoinRoomText[0].text = "Enter";
            Start_JoinRoomText[1].text = "Enter";
        }
    }

    public void ShowCreateRoom(string key)
    {
        SoundManager.instance.PlayPickUp(Vector3.zero);
        CreateRoomKey[0].text = "Share this key with player 2 -\r\n" + key;
        CreateRoomKey[1].text = "Share this key with player 2 -\r\n" + key;
        Start_CreateRoom.enabled = false;
        Start_CreateRoomText[0].text = "Waiting for Player 2";
        Start_CreateRoomText[1].text = "Waiting for Player 2";

        MainMenuHUD.SetTrigger("CreateRoom");
    }

    public void UpdateCreateRoom(bool enabled)
    {
        if (enabled)
        {
            SoundManager.instance.PlayPickUp(Vector3.zero);
            Start_CreateRoom.enabled = true;
            Start_CreateRoomText[0].text = "Begin";
            Start_CreateRoomText[1].text = "Begin";
        } else
        {
            SoundManager.instance.PlayError(Vector3.zero);
            Start_CreateRoom.enabled = false;
            Start_CreateRoomText[0].text = "Waiting for Player 2";
            Start_CreateRoomText[1].text = "Waiting for Player 2";
        }
    }

    public void ShowLoading()
    {
        StopAllCoroutines();
        LoadingMenu.gameObject.SetActive(true);
        LoadingMenu.SetBool("Pop", true);
    }

    public void HideLoading()
    {
        LoadingMenu.SetBool("Pop", false);
        StartCoroutine(DisableLoadingMenu());
    }

    public void JoinRoom()
    {
        SoundManager.instance.PlayPickUp(Vector3.zero);
        string roomKey = RoomID.text;
        string Player2NickName = Player2Name.text;
        if (roomKey == "")
        {
            ShowJoinRoomError();
            return;
        }
        if (Player2NickName == "")
            Player2NickName = "Player 2";

        string random = "";
        for (int i = 0; i < 6; i++)
            random += (char)(Random.Range(0, 25) + 'A');

        NetworkManager.instance.JoinRoom(roomKey, Player2NickName+random);
    }

    IEnumerator DisableLoadingMenu()
    {
        yield return new WaitForSeconds(0.5f);
        LoadingMenu.gameObject.SetActive(false);
    }

    public void ShowJoinRoomError()
    {
        SoundManager.instance.PlayError(Vector3.zero);
        RoomID.text = "";
        MainMenuHUD.SetTrigger("JoinError");
    }

    public void BeginMultiplayerGame()
    {
        SoundManager.instance.PlayPickUp(Vector3.zero);
        string Player1NickName = Player1Name.text;
        if (Player1NickName == "")
            Player1NickName = "Player 1";
        string random = "";
        for (int i = 0; i < 6; i++)
            random += (char)(Random.Range(0, 25) + 'A');
        NetworkManager.instance.StartGame(Player1NickName+random);
    }
}
