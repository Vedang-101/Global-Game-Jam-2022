using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkedUIManager : MonoBehaviourPunCallbacks
{
    [Header("Objectives")]
    public GameObject[] ObjectivesHolder;
    public GameObject ObjectiveCardPrefab;
    public float DistanceBetweenTwoCards = 140f;
    public Animator[] PlayerSets;
    public Animator[] OpponentSets;
    int PlayerSetIdx = -1;
    int OpponentSetIdx = -1;

    [Header("Inventory Text")]
    public Text MirrorCountText;
    public Text SplitterCountText;
    public Animator MirrorCross;
    public Animator SplitterCross;

    public Text[] TimerText;

    [Header("UI Animations")]
    public Animator MainHUD;
    public Animator SuccessHUD;
    public Animator GameFinishedHUD;
    public Animator PauseHUD;
    public Animator TutHUD;

    [Header("EndGame Refrences")]
    public GameObject Player1EndGameReference;
    public GameObject Player2EndGameReference;
    public Transform EndGameReferencePosition;
    public Text[] EndGameText;

    int TutIndex = 0;

    public List<ObjectiveUI>[] Objectives = new List<ObjectiveUI>[2];
    public void UpdateTicks()
    {
        foreach (ObjectiveUI objective in Objectives[0])
        {
            if (objective.ObjectiveReference.IsInColor != objective.ObjectiveReference.WasInColor)
            {
                objective.setTick(objective.ObjectiveReference.IsInColor);
            }
        }
        foreach (ObjectiveUI objective in Objectives[1])
        {
            if (objective.ObjectiveReference.IsInColor != objective.ObjectiveReference.WasInColor)
            {
                objective.setTick(objective.ObjectiveReference.IsInColor);
            }
        }
    }

    public void DisplayNewObjectives(List<ColorableObject> objectives, int id)
    {
        int holderIndex = -1;
        if (id == 0)
        {
            if (PhotonNetwork.IsMasterClient)
                holderIndex = 0;
            else
                holderIndex = 1;
        }
        else
        {
            if (PhotonNetwork.IsMasterClient)
                holderIndex = 1;
            else
                holderIndex = 0;
        }

        if (holderIndex == 0)
        {
            StopCoroutine("LoadNewObjectivesPlayer");
            StartCoroutine(LoadNewObjectivesPlayer(objectives, id));
        }
        else
        {
            StopCoroutine("LoadNewObjectivesOpponent");
            StartCoroutine(LoadNewObjectivesOpponent(objectives, id));
        }
    }

    IEnumerator LoadNewObjectivesPlayer(List<ColorableObject> objectives, int id)
    {
        if (PlayerSetIdx >= 0 && PlayerSetIdx < 3)
            PlayerSets[PlayerSetIdx].gameObject.SetActive(true);
        PlayerSetIdx++;
        foreach (ObjectiveUI obj in Objectives[id])
        {
            obj.PopOut();
            Destroy(obj.gameObject, 0.1f);
        }
        Objectives[id].Clear();
        

        float Incrementer = DistanceBetweenTwoCards;
        float XPos = 50;

        foreach (ColorableObject obj in objectives)
        {
            GameObject objCard = Instantiate(ObjectiveCardPrefab, ObjectivesHolder[0].transform);
            objCard.GetComponent<RectTransform>().localPosition = new Vector3(XPos, 0, 0);
            ObjectiveUI objCardUI = objCard.GetComponent<ObjectiveUI>();
            objCardUI.ObjectiveReference = obj;
            objCardUI.setIcon(obj.IconImage);
            objCardUI.PopIn();
            Objectives[id].Add(objCardUI);
            XPos += Incrementer;
            yield return new WaitForSeconds(0.25f);
        }

        yield return null;
    }

    IEnumerator LoadNewObjectivesOpponent(List<ColorableObject> objectives, int id)
    {
        if (OpponentSetIdx >= 0 && OpponentSetIdx < 3)
            OpponentSets[OpponentSetIdx].gameObject.SetActive(true);
        
        OpponentSetIdx++;
        foreach (ObjectiveUI obj in Objectives[id])
        {
            obj.PopOut();
            Destroy(obj.gameObject, 0.1f);
        }
        Objectives[id].Clear();

        float Incrementer = -DistanceBetweenTwoCards;
        float XPos = -50;

        foreach (ColorableObject obj in objectives)
        {
            GameObject objCard = Instantiate(ObjectiveCardPrefab, ObjectivesHolder[1].transform);
            objCard.GetComponent<RectTransform>().localPosition = new Vector3(XPos, 0, 0);
            ObjectiveUI objCardUI = objCard.GetComponent<ObjectiveUI>();
            objCardUI.ObjectiveReference = obj;
            objCardUI.setIcon(obj.IconImage);
            objCardUI.PopIn();
            Objectives[id].Add(objCardUI);
            XPos += Incrementer;
            yield return new WaitForSeconds(0.25f);
        }

        yield return null;
    }

    public void UpdatePlayerInventory(int MirrorCount, int SplitterCount)
    {
        MirrorCountText.text = MirrorCount > 0 ? MirrorCount.ToString() : "NaN";
        SplitterCountText.text = SplitterCount > 0 ? SplitterCount.ToString() : "NaN";
    }

    public void TriggerMirrorCross()
    {
        SoundManager.instance.PlayError(Vector3.zero);
        MirrorCross.SetTrigger("Pop");
    }
    public void TriggerSplitterCross()
    {
        SoundManager.instance.PlayError(Vector3.zero);
        SplitterCross.SetTrigger("Pop");
    }

    public void DisplayTimer(float timer)
    {
        int mins = (int)(timer / 60);
        int secs = (int)(timer % 60);
        string Minutes = "";
        string Seconds = "";
        if (mins < 10)
            Minutes += "0";
        if (secs < 10)
            Seconds += "0";
        
        Minutes += mins.ToString();
        Seconds += secs.ToString();

        TimerText[0].text = Minutes + ":" + Seconds;
        TimerText[1].text = Minutes + ":" + Seconds;
    }

    public void DisplaySuccess()
    {
        SoundManager.instance.PlayGameWin();
        NetworkedGameManager.instance.GamePaused = true;
        MainHUD.SetBool("OnlyObjective", true);
        GameFinishedHUD.gameObject.SetActive(false);
        PauseHUD.gameObject.SetActive(false);
        SuccessHUD.gameObject.SetActive(true);
        SuccessHUD.Play("Success", 0);
    }

    public void DisplayPause()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SoundManager.instance.PlayPickUp(Vector3.zero);
        NetworkedGameManager.instance.GamePaused = true;
        SuccessHUD.gameObject.SetActive(false);
        GameFinishedHUD.gameObject.SetActive(false);
        PauseHUD.gameObject.SetActive(true);
        PauseHUD.Play("Pause_Networked", 0);
    }

    public void RemovePause()
    {
        SoundManager.instance.PlayPickUp(Vector3.zero);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        NetworkedGameManager.instance.GamePaused = false;
        SuccessHUD.gameObject.SetActive(false);
        GameFinishedHUD.gameObject.SetActive(false);
        PauseHUD.gameObject.SetActive(true);
        PauseHUD.Play("Pause_Out_Networked", 0);
    }

    public void DisplayGameFinished(int winner)
    {
        if (GameFinishedHUD.gameObject.active == true)
            return;

        Dictionary<int, Player> players = PhotonNetwork.CurrentRoom.Players;
        if (winner == 1)
        {
            Player2EndGameReference.SetActive(false);
            Player1EndGameReference.transform.position = EndGameReferencePosition.position;

            if (PhotonNetwork.IsMasterClient)
            {
                foreach (var item in players)
                {
                    if(item.Value == PhotonNetwork.LocalPlayer)
                    {
                        string nickName = item.Value.NickName.Substring(0, item.Value.NickName.Length - 6);
                        EndGameText[0].text = nickName + " Won this game!";
                        EndGameText[1].text = nickName + " Won this game!";
                    }
                }
            }
            else
            {
                foreach (var item in players)
                {
                    if (item.Value != PhotonNetwork.LocalPlayer)
                    {
                        string nickName = item.Value.NickName.Substring(0, item.Value.NickName.Length - 6);
                        EndGameText[0].text = nickName + " Won this game!";
                        EndGameText[1].text = nickName + " Won this game!";
                    }
                }
            }
        } 
        else if(winner == 2)
        {
            Player1EndGameReference.SetActive(false);
            Player2EndGameReference.transform.position = EndGameReferencePosition.position;

            if (PhotonNetwork.IsMasterClient)
            {
                foreach (var item in players)
                {
                    if (item.Value != PhotonNetwork.LocalPlayer)
                    {
                        string nickName = item.Value.NickName.Substring(0, item.Value.NickName.Length - 6);
                        EndGameText[0].text = nickName + " Won this game!";
                        EndGameText[1].text = nickName + " Won this game!";
                    }
                }
            }
            else
            {
                foreach (var item in players)
                {
                    if (item.Value == PhotonNetwork.LocalPlayer)
                    {
                        string nickName = item.Value.NickName.Substring(0, item.Value.NickName.Length - 6);
                        EndGameText[0].text = nickName + " Won this game!";
                        EndGameText[1].text = nickName + " Won this game!";
                    }
                }
            }
        }
        else
        {
            EndGameText[0].text = "It was a tie!";
            EndGameText[1].text = "It was a tie!";
        }

        NetworkedGameManager.instance.GamePaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SoundManager.instance.PlayGameWin();

        MainHUD.SetBool("AllClear", true);
        SuccessHUD.gameObject.SetActive(false);
        PauseHUD.gameObject.SetActive(false);
        GameFinishedHUD.gameObject.SetActive(true);
        GameFinishedHUD.Play("GameFinished_Networked", 0);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        SceneManager.LoadScene(0);
    }

    public void GoToMainMenu()
    {
        SoundManager.instance.PlayPickUp(Vector3.zero);
        PhotonNetwork.Disconnect();
    }

    public void TutBack()
    {
        SoundManager.instance.PlayPickUp(Vector3.zero);
        TutHUD.SetTrigger("Back");
        TutIndex--;
    }

    public void ShowNextTut()
    {
        SoundManager.instance.PlayPickUp(Vector3.zero);
        TutIndex++;
        TutHUD.SetTrigger("Next");
    }
}
