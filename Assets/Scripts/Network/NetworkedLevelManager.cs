using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class NetworkedLevelManager : MonoBehaviour
{
    public static NetworkedLevelManager instance;

    private void Awake()
    {
        instance = this;
    }

    [System.Serializable]
    public struct String_ColorableObject
    {
        public string Name;
        public ColorableObject CO;
    }

    public List<String_ColorableObject> AllPossibleColorables;

    public List<ColorableObject> SyncedPossibleColorables;

    List<ColorableObject> Player1Objective;
    List<ColorableObject> Player2Objective;

    public int Player1Wave;
    public int Player2Wave;

    PhotonView PV;

    void Start()
    {
        PV = GetComponent<PhotonView>();

        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < AllPossibleColorables.Count; i++)
            {
                String_ColorableObject temp = AllPossibleColorables[i];
                int randomIndex = Random.Range(i, AllPossibleColorables.Count);
                AllPossibleColorables[i] = AllPossibleColorables[randomIndex];
                AllPossibleColorables[randomIndex] = temp;
            }
            for (int i = 0; i < AllPossibleColorables.Count; i++)
            {
                PV.RPC("SetPool", RpcTarget.All, AllPossibleColorables[i].Name, i);
            }
        }
    }

    public void StartGame()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        NetworkedGameManager.instance.GamePaused = false;

        PV.RPC("SetReady", RpcTarget.All);
    }

    public void LoadObjectives()
    {
        Player1Objective = new List<ColorableObject>();
        Player2Objective = new List<ColorableObject>();

        for (int i = 0; i < 3; i++)
            Player1Objective.Add(SyncedPossibleColorables[i]);
        for (int i = 3; i < 6; i++)
            Player2Objective.Add(SyncedPossibleColorables[i]);

        NetworkedGameManager.instance.UIManagerObject.DisplayNewObjectives(Player1Objective, 0);
        NetworkedGameManager.instance.UIManagerObject.DisplayNewObjectives(Player2Objective, 1);
    }

    public void CheckEndGame()
    {
        if (Player1Wave != Player2Wave)
        {
            if (Player1Wave > Player2Wave)
                ShowEndGame(1);
            else
                ShowEndGame(2);
        }
        else
        {
            int player1Percentage = 0;
            int player2Percentage = 0;
            foreach (ColorableObject co in Player1Objective)
            {
                if (co.IsInColor)
                    player1Percentage++;
            }
            foreach (ColorableObject co in Player2Objective)
            {
                if (co.IsInColor)
                    player2Percentage++;
            }

            if (player1Percentage > player2Percentage)
                ShowEndGame(1);
            else if (player1Percentage < player2Percentage)
                ShowEndGame(2);
            else
                ShowEndGame(0);
        }
    }

    float m_timer_Player1 = 0.0f;
    float m_timer_Player2 = 0.0f;

    bool FinishedGame()
    {
        return Player1Wave==3 || Player2Wave==3;
    }

    public void CheckObjectiveStates()
    {
        if (FinishedGame())
            return;
        else
        {
            {
                bool objectivesCompleted = true;
                foreach (ColorableObject co in Player1Objective)
                {
                    if (co.IsInColor == false)
                    {
                        objectivesCompleted = false;
                        break;
                    }
                }

                if (objectivesCompleted)
                {
                    m_timer_Player1 += Time.deltaTime;
                    if (m_timer_Player1 >= 0.5f)
                    {
                        Player1Wave++;
                        if (!FinishedGame())
                        {
                            Player1Objective.Clear();
                            if (Player1Wave == 1)
                            {
                                for (int i = 6; i < 10; i++)
                                    Player1Objective.Add(SyncedPossibleColorables[i]);
                            }
                            else if (Player1Wave == 2)
                            {
                                for (int i = 14; i < 19; i++)
                                    Player1Objective.Add(SyncedPossibleColorables[i]);
                            }
                            NetworkedGameManager.instance.UIManagerObject.DisplayNewObjectives(Player1Objective, 0);
                            NetworkedGameManager.instance.DisplaySuccess(1);
                        }
                        else
                        {
                            Player1Objective.Clear();

                            NetworkedGameManager.instance.UIManagerObject.DisplayNewObjectives(Player1Objective, 0);
                            ShowEndGame(1);
                        }
                        m_timer_Player1 = 0.0f;
                    }
                }
                else
                {
                    m_timer_Player1 = 0.0f;
                }
            }

            {
                bool objectivesCompleted = true;
                foreach (ColorableObject co in Player2Objective)
                {
                    if (co.IsInColor == false)
                    {
                        objectivesCompleted = false;
                        break;
                    }
                }

                if (objectivesCompleted)
                {
                    m_timer_Player2 += Time.deltaTime;
                    if (m_timer_Player2 >= 0.5f)
                    {
                        Player2Wave++;
                        if (!FinishedGame())
                        {
                            Player2Objective.Clear();
                            if (Player2Wave == 1)
                            {
                                for (int i = 10; i < 14; i++)
                                    Player2Objective.Add(SyncedPossibleColorables[i]);
                            }
                            else if (Player2Wave == 2)
                            {
                                for (int i = 19; i < 24; i++)
                                    Player2Objective.Add(SyncedPossibleColorables[i]);
                            }
                            NetworkedGameManager.instance.UIManagerObject.DisplayNewObjectives(Player2Objective, 1);
                            NetworkedGameManager.instance.DisplaySuccess(2);
                        }
                        else
                        {
                            Player2Objective.Clear();

                            NetworkedGameManager.instance.UIManagerObject.DisplayNewObjectives(Player2Objective, 1);
                            ShowEndGame(2);
                        }
                        m_timer_Player2 = 0.0f;
                    }
                }
                else
                {
                    m_timer_Player2 = 0.0f;
                }
            }
        }
    }

    public void ShowEndGame(int index)
    {
        PV.RPC("ShowGameEnd", RpcTarget.All, index);
    }

    [PunRPC]
    void SetReady()
    {
        NetworkedGameManager.instance.readyPlayers += 1;
    }

    [PunRPC]
    void SetPool(string Objective, int id)
    {
        for(int i=0; i<AllPossibleColorables.Count; i++)
        {
            if (AllPossibleColorables[i].Name == Objective)
            {
                SyncedPossibleColorables[id] = AllPossibleColorables[i].CO;
                break;
            }
        }
    }

    [PunRPC]
    void ShowGameEnd(int index)
    {
        NetworkedGameManager.instance.UIManagerObject.DisplayGameFinished(index);
    }
}